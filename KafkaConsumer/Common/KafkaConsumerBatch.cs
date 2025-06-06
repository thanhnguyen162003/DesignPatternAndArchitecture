﻿using Confluent.Kafka;
using Polly;
using Polly.CircuitBreaker;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace KafkaConsumer.Common
{
    public abstract class KafkaConsumerBaseBatch<T> : BackgroundService
    {
        private readonly IConsumer<string, string> _consumer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<KafkaConsumerBaseBatch<T>> _logger;
        private readonly List<string> _topicNames;
        private readonly BatchProcessingOptions _options;
        private readonly ConcurrentBag<string> _messageBatch = new ConcurrentBag<string>();
        private readonly SemaphoreSlim _batchProcessingSemaphore;
        private int _consecutiveFailures = 0;
        private bool _circuitBroken = false;
        private DateTime _circuitResetTime = DateTime.MinValue;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly IAsyncPolicy _circuitBreakerPolicy;

        private readonly Dictionary<TopicPartition, ConsumeResult<string, string>> _uncommittedOffsets =
            new Dictionary<TopicPartition, ConsumeResult<string, string>>();
        private readonly object _offsetLock = new object();

        private readonly Stopwatch _batchStopwatch = new Stopwatch();
        private long _totalMessagesProcessed = 0;
        private long _totalBatchesProcessed = 0;
        private long _failedBatches = 0;
        private double _averageProcessingTimeMs = 0;
        private readonly ConcurrentDictionary<string, long> _messageCountByTopic = new ConcurrentDictionary<string, long>();

        private volatile bool _isHealthy = true;
        private DateTime _lastSuccessfulConsume = DateTime.UtcNow;
        private DateTime _lastSuccessfulBatchProcess = DateTime.UtcNow;

        public class BatchProcessingOptions
        {
            public int BatchSize { get; set; } = 100;
            public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromSeconds(5);
            public int MaxConcurrentBatches { get; set; } = Environment.ProcessorCount;
            public TimeSpan CommitInterval { get; set; } = TimeSpan.FromSeconds(10);
            public int MaxConsecutiveFailures { get; set; } = 5;
            public TimeSpan CircuitBreakerResetTime { get; set; } = TimeSpan.FromMinutes(1);
            public int MaxRetries { get; set; } = 3;
        }

        protected KafkaConsumerBaseBatch(
            IConfiguration configuration,
            ILogger<KafkaConsumerBaseBatch<T>> logger,
            IServiceProvider serviceProvider,
            List<string> topicNames,
            string groupId,
            BatchProcessingOptions options = null)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _topicNames = topicNames ?? throw new ArgumentNullException(nameof(topicNames));
            _options = options ?? new BatchProcessingOptions();
            _batchProcessingSemaphore = new SemaphoreSlim(_options.MaxConcurrentBatches);

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    _options.MaxRetries,
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount}/{_options.MaxRetries} after {timeSpan.TotalSeconds}s due to: {ex.Message}");
                    });

            _circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    _options.MaxConsecutiveFailures,
                    _options.CircuitBreakerResetTime,
                    (ex, duration) =>
                    {
                        _circuitBroken = true;
                        _circuitResetTime = DateTime.UtcNow.Add(duration);
                        _logger.LogError($"Circuit broken for {duration.TotalSeconds}s due to: {ex.Message}");
                    },
                    () =>
                    {
                        _circuitBroken = false;
                        _consecutiveFailures = 0;
                        _logger.LogInformation("Circuit reset");
                    });

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                //SaslUsername = configuration["Kafka:SaslUsername"],
                //SaslPassword = configuration["Kafka:SaslPassword"],
                //SecurityProtocol = SecurityProtocol.SaslSsl,
                //SaslMechanism = SaslMechanism.Plain,
                GroupId = groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                MaxPollIntervalMs = 300000,     // 5 minutes
                SessionTimeoutMs = 30000,       // 30 seconds
                FetchMaxBytes = 1048576,        // 1MB
                FetchMinBytes = 10240,          // 10KB
                FetchWaitMaxMs = 500,           // 500ms
                MaxPartitionFetchBytes = 1048576, // 1MB
                QueuedMaxMessagesKbytes = 2097151, //2mb
                StatisticsIntervalMs = 5000     // Enable statistics for monitoring
            };

            _consumer = new ConsumerBuilder<string, string>(consumerConfig)
                .SetErrorHandler((_, error) =>
                {
                    _logger.LogError($"Kafka error: {error.Reason}");
                    if (error.IsFatal)
                    {
                        _isHealthy = false;
                    }
                })
                .SetStatisticsHandler((_, stats) =>
                {
                    if (DateTime.UtcNow.Second == 0 && DateTime.UtcNow.Minute % 1 == 0)
                    {
                        _logger.LogDebug($"Kafka stats: {stats}");
                    }
                })
                .Build();
        }

        protected KafkaConsumerBaseBatch(
            IConfiguration configuration,
            ILogger<KafkaConsumerBaseBatch<T>> logger,
            IServiceProvider serviceProvider,
            string topicName,
            string groupId,
            BatchProcessingOptions options = null)
            : this(configuration, logger, serviceProvider, new List<string> { topicName }, groupId, options)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            _consumer.Subscribe(_topicNames);
            _logger.LogInformation($"Subscribed to topics: {string.Join(", ", _topicNames)}");

            using var commitTimer = new Timer(CommitOffsets, null, _options.CommitInterval, _options.CommitInterval);

            using var cancellationRegistration = stoppingToken.Register(() =>
            {
                _logger.LogInformation("Shutdown requested, processing remaining messages...");
            });

            var lastBatchTime = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_circuitBroken)
                    {
                        if (DateTime.UtcNow < _circuitResetTime)
                        {
                            _logger.LogDebug($"Circuit open, waiting for reset. Time remaining: {(_circuitResetTime - DateTime.UtcNow).TotalSeconds}s");
                            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                            continue;
                        }
                    }

                    if (_messageBatch.Count > _options.BatchSize * 3)
                    {
                        _logger.LogWarning($"Backpressure: {_messageBatch.Count} messages in queue");
                        await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);

                        if (_messageBatch.Count > 0)
                        {
                            await ProcessCurrentBatchAsync(stoppingToken);
                            lastBatchTime = DateTime.UtcNow;
                        }
                        continue;
                    }

                    var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));
                    if (consumeResult != null)
                    {
                        _lastSuccessfulConsume = DateTime.UtcNow;

                        lock (_offsetLock)
                        {
                            _uncommittedOffsets[consumeResult.TopicPartition] = consumeResult;
                        }

                        _messageBatch.Add(consumeResult.Message.Value);

                        _messageCountByTopic.AddOrUpdate(
                            consumeResult.Topic,
                            1,
                            (_, count) => count + 1);

                        if (_messageBatch.Count >= _options.BatchSize ||
                            DateTime.UtcNow - lastBatchTime >= _options.BatchTimeout)
                        {
                            await ProcessCurrentBatchAsync(stoppingToken);
                            lastBatchTime = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        // Process any remaining messages if we have a timeout
                        if (_messageBatch.Count > 0 &&
                            DateTime.UtcNow - lastBatchTime >= _options.BatchTimeout)
                        {
                            await ProcessCurrentBatchAsync(stoppingToken);
                            lastBatchTime = DateTime.UtcNow;
                        }
                    }
                }
                catch (ConsumeException e)
                {
                    _logger.LogError($"Consume error: {e.Error.Reason}");
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumption was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in consumer loop: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            try
            {
                if (_messageBatch.Count > 0)
                {
                    _logger.LogInformation($"Processing {_messageBatch.Count} remaining messages before shutdown");
                    await ProcessCurrentBatchAsync(stoppingToken);
                }

                CommitOffsets(null);

                _consumer.Unsubscribe();
                _consumer.Close();
                _logger.LogInformation("Kafka consumer closed gracefully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during consumer shutdown: {ex.Message}");
            }
        }

        private async Task ProcessCurrentBatchAsync(CancellationToken stoppingToken)
        {
            if (_messageBatch.IsEmpty)
                return;

            var messages = _messageBatch.ToArray();
            _messageBatch.Clear();

            await _batchProcessingSemaphore.WaitAsync(stoppingToken);

            try
            {
                _batchStopwatch.Restart();

                await _retryPolicy.WrapAsync(_circuitBreakerPolicy).ExecuteAsync(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var scopedProvider = scope.ServiceProvider;

                    await ProcessBatch(messages, scopedProvider);

                    Interlocked.Exchange(ref _consecutiveFailures, 0);
                });

                _batchStopwatch.Stop();

                _lastSuccessfulBatchProcess = DateTime.UtcNow;
                var messageCount = messages.Length;
                var batchTime = _batchStopwatch.ElapsedMilliseconds;

                Interlocked.Add(ref _totalMessagesProcessed, messageCount);
                Interlocked.Increment(ref _totalBatchesProcessed);

                var currentBatchCount = Interlocked.Read(ref _totalBatchesProcessed);
                if (currentBatchCount == 1)
                {
                    _averageProcessingTimeMs = batchTime;
                }
                else
                {
                    _averageProcessingTimeMs = (_averageProcessingTimeMs * (currentBatchCount - 1) + batchTime) / currentBatchCount;
                }

                _logger.LogInformation(
                    $"Batch processed: {messageCount} messages in {batchTime}ms " +
                    $"(avg: {batchTime / (double)messageCount:F2}ms per message) " +
                    $"Total: {_totalMessagesProcessed} messages in {_totalBatchesProcessed} batches " +
                    $"(avg batch size: {_totalMessagesProcessed / (double)_totalBatchesProcessed:F2})");
            }
            catch (BrokenCircuitException ex)
            {
                Interlocked.Increment(ref _failedBatches);
                _logger.LogError($"Circuit broken, batch processing suspended: {ex.Message}");

            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref _failedBatches);
                var failures = Interlocked.Increment(ref _consecutiveFailures);

                _logger.LogError($"Error processing batch (failure {failures}/{_options.MaxConsecutiveFailures}): {ex.Message}");

                if (failures < _options.MaxConsecutiveFailures)
                {
                    _logger.LogWarning($"Requeuing {messages.Length} messages for retry");
                    foreach (var message in messages)
                    {
                        _messageBatch.Add(message);
                    }
                }
            }
            finally
            {
                _batchProcessingSemaphore.Release();
            }
        }
        private void CommitOffsets(object state)
        {
            Dictionary<TopicPartition, ConsumeResult<string, string>> offsetsToCommit;

            lock (_offsetLock)
            {
                if (_uncommittedOffsets.Count == 0)
                    return;

                offsetsToCommit = new Dictionary<TopicPartition, ConsumeResult<string, string>>(_uncommittedOffsets);
                _uncommittedOffsets.Clear();
            }

            if (offsetsToCommit.Count > 0)
            {
                try
                {
                    var topicPartitionOffsets = offsetsToCommit
                        .Select(kv => new TopicPartitionOffset(
                            kv.Key,
                            new Offset(kv.Value.Offset.Value + 1)))
                        .ToList();

                    _consumer.Commit(topicPartitionOffsets);

                    _logger.LogDebug($"Committed offsets for {topicPartitionOffsets.Count} partitions");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error committing offsets: {ex.Message}");

                    lock (_offsetLock)
                    {
                        foreach (var offset in offsetsToCommit)
                        {
                            _uncommittedOffsets[offset.Key] = offset.Value;
                        }
                    }
                }
            }
        }

        protected abstract Task ProcessBatch(IEnumerable<string> messages, IServiceProvider serviceProvider);

        public bool IsHealthy() =>
            _isHealthy &&
            DateTime.UtcNow - _lastSuccessfulConsume < TimeSpan.FromMinutes(5) &&
            DateTime.UtcNow - _lastSuccessfulBatchProcess < TimeSpan.FromMinutes(5) &&
            !_circuitBroken;

        public ConsumerMetrics GetMetrics() => new ConsumerMetrics
        {
            MessagesProcessed = _totalMessagesProcessed,
            BatchesProcessed = _totalBatchesProcessed,
            FailedBatches = _failedBatches,
            AverageProcessingTimeMs = _averageProcessingTimeMs,
            AverageBatchSize = _totalBatchesProcessed > 0
                ? _totalMessagesProcessed / (double)_totalBatchesProcessed
                : 0,
            IsHealthy = IsHealthy(),
            CurrentBatchSize = _messageBatch.Count,
            CircuitBroken = _circuitBroken,
            ConsecutiveFailures = _consecutiveFailures
        };

        public DetailedConsumerMetrics GetDetailedMetrics() => new DetailedConsumerMetrics
        {
            BasicMetrics = GetMetrics(),
            MessageCountByTopic = new Dictionary<string, long>(_messageCountByTopic),
            LastSuccessfulConsume = _lastSuccessfulConsume,
            LastSuccessfulBatchProcess = _lastSuccessfulBatchProcess,
            CircuitResetTime = _circuitResetTime
        };

        public class ConsumerMetrics
        {
            public long MessagesProcessed { get; set; }
            public long BatchesProcessed { get; set; }
            public long FailedBatches { get; set; }
            public double AverageProcessingTimeMs { get; set; }
            public double AverageBatchSize { get; set; }
            public bool IsHealthy { get; set; }
            public int CurrentBatchSize { get; set; }
            public bool CircuitBroken { get; set; }
            public int ConsecutiveFailures { get; set; }
        }

        public class DetailedConsumerMetrics
        {
            public ConsumerMetrics BasicMetrics { get; set; }
            public Dictionary<string, long> MessageCountByTopic { get; set; }
            public DateTime LastSuccessfulConsume { get; set; }
            public DateTime LastSuccessfulBatchProcess { get; set; }
            public DateTime CircuitResetTime { get; set; }
        }
    }
}

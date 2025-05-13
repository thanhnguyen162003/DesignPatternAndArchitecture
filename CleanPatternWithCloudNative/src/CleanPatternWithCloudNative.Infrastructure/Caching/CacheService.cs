using System.Buffers;
using System.Text.Json;

using CleanPatternWithCloudNative.Application.Abstractions.Caching;

using Microsoft.Extensions.Caching.Distributed;

namespace CleanPatternWithCloudNative.Infrastructure.Caching
{
    public sealed class CacheService(IDistributedCache cache) : ICacheService
    {
        public async Task<T?> GetAsync<T>(
            string key,
            CancellationToken cancellationToken = default)
        {
            byte[]? bytes = await cache.GetAsync(key, cancellationToken);

            if (bytes is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(bytes!);
        }

        public Task RemoveAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            return cache.RemoveAsync(key, cancellationToken);
        }

        public Task SetAsync<T>(
            string key,
            T value,
            TimeSpan? expiration,
            CancellationToken cancellationToken = default)
        {
            var buffer = new ArrayBufferWriter<byte>();
            using var writer = new Utf8JsonWriter(buffer);
            JsonSerializer.Serialize(writer, value);
            byte[] bytes = buffer.WrittenSpan.ToArray();

            return cache.SetAsync(
                key,
                bytes,
                CacheOptions.Create(expiration),
                cancellationToken);
        }
    }
}
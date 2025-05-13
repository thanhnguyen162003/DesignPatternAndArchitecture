namespace CleanPatternWithCloudNative.Domain.Abstract
{
    public static class Constants
    {
        public const string AllProductsCacheKey = "AllProducts";
        public const string PubSubName = "pubsub";
        public const string PubSubTopicName = "message";
        public const string StateStoreName = "statestore";
        public const string SecretsStoreName = "secretstore";
        public const string ConfigStoreName = "configstore";
        public const string DatabaseConnectionstringName = "cleanpatternwithcloudnativedb";
        public const string CacheConnectionstringName = "redis";
    }
}
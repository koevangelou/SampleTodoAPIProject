
using Microsoft.Extensions.Caching.Distributed;

namespace TodoApi.Services.Caching
{
    public class CacheService : IRedisCacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;

        }
            public T? GetData<T>(string key)
        {
            
            var data = _cache.Get(key);
            if (data == null)
            {
                return default;
            }
            return System.Text.Json.JsonSerializer.Deserialize<T>(data);
        }

        public void RemoveData(string key)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(string key, T value, TimeSpan? expiry = null)
        {
            
            var options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiry
            };

            _cache.SetString(key, System.Text.Json.JsonSerializer.Serialize(value), options);
        }
    }
}

namespace TodoApi.Services.Caching
{
    public interface IRedisCacheService
    {
        T? GetData<T>(string key);
         void SetData<T>(string key, T value, TimeSpan? expiry = null);
         void RemoveData(string key);
    }
}

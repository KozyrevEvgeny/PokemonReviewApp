using Newtonsoft.Json;
using StackExchange.Redis;

namespace PokemonReviewApp.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }

        public string GetString(string key)
        {
            return _database.StringGet(key);
        }

        public T? GetValue<T>(string key)
        {
            var json = _database.StringGet(key);

            if (string.IsNullOrEmpty(json))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public void SetValue<T>(string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);

            SetValue(key, json);
        }

        // проверить, можно ли заюзать для обновления кеша
        public void SetValue(string key, string value)
        {
            _database.StringSet(key, value, expiry: TimeSpan.FromSeconds(60));
        }
    }
}

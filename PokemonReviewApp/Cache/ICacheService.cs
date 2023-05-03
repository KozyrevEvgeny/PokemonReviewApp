namespace PokemonReviewApp.Cache
{
    public interface ICacheService
    {
        string GetString(string key);
        T GetValue<T>(string key);
        void SetValue<T>(string key, T value);
        void SetValue (string key, string value);

    }
}

namespace PokemonReviewApp.Extensions
{
    public static class DbContextExtensions
    {
        public static void SeedData(this IServiceProvider services)
        {
            using (var scope = services.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<Seed>();
                service.SeedDataContext();
            }
        }
    }
}

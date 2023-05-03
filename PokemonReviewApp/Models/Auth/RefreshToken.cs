using PokemonReviewApp.Models;

namespace PokemonReviewApp.Models.Auth
{
    public class RefreshToken
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}

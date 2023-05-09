using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Data;
using PokemonReviewApp.Models.Auth;

namespace PokemonReviewApp.Services
{
    public class RefreshTokenService
    {
        private readonly DataContext _context;

        public RefreshTokenService(DataContext context)
        {
            _context = context;
        }

        public async Task<string> CreateRefreshTokenAsync(User user)
        {
            var token = $"{Guid.NewGuid()}{Guid.NewGuid()}".Replace("-", "");

            var existsToken = await _context.RefreshToken.SingleOrDefaultAsync(t => t.UserId == user.Id);

            if (existsToken != null)
            {
                _context.Remove(existsToken);
            }

            var newToken = new RefreshToken()
            {
                Token = token,
                UserId = user.Id
            };

            await _context.AddAsync(newToken);
            await _context.SaveChangesAsync();

            return token;
        }
    }
}

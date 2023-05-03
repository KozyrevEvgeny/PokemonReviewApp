using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Models;
using PokemonReviewApp.Models.Auth;

namespace PokemonReviewApp.Data
{
    public class DatabaseAuthContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public DbSet<RefreshToken> RefreshToken { get; set; }

        public DatabaseAuthContext(DbContextOptions<DatabaseAuthContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
                .HasKey(t => new { t.UserId, t.Token });
        }
    }
}

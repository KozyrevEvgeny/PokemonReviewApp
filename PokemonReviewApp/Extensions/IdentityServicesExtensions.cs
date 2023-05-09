using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PokemonReviewApp.Data;
using PokemonReviewApp.Models;
using PokemonReviewApp.Models.Auth;
using System.Text;

namespace PokemonReviewApp.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services,
            IConfiguration configuration) 
        {
            services.AddIdentityCore<User>(opt =>
            {
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequiredLength = 10;
            })
                .AddRoles<IdentityRole<int>>()
                .AddRoleManager<RoleManager<IdentityRole<int>>>()
                .AddUserManager<UserManager<User>>()
                .AddSignInManager<SignInManager<User>>()
                .AddEntityFrameworkStores<DataContext>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenSecret"])),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                    };
                });
            return services;
        }
    }
}

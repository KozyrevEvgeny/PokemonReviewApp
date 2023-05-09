using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Constants;
using PokemonReviewApp.Data;
using PokemonReviewApp.Dto;
using PokemonReviewApp.Models;
using PokemonReviewApp.Models.Auth;
using PokemonReviewApp.Services;

namespace PokemonReviewApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;
        private readonly RefreshTokenService _refreshTokenService;
        private readonly DataContext _context;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly UserManager<IdentityUser<int>> _user2Manager;

        public AuthController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            TokenService tokenService,
            RefreshTokenService refreshTokenService,
            DataContext context,
            RoleManager<IdentityRole<int>> roleManager,
            UserManager<IdentityUser<int>> user2Manager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _context = context;
            _roleManager = roleManager;
            _user2Manager = user2Manager;
        }

        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpDto dto)
        {
            var user = new User()
            {
                UserName = dto.Username,
            };

            var signUpResult = await _userManager.CreateAsync(user, dto.Password);
            if (!signUpResult.Succeeded)
            {
                return BadRequest(signUpResult.Errors);
            }
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<ActionResult<SignInResultDto>> SignInAsync(SignInDto dto)
        {
            var normalizedSignInfo = dto.Username.ToUpper();
            var user = await _userManager.Users
                .SingleOrDefaultAsync(u => u.NormalizedUserName == normalizedSignInfo ||
                                            u.NormalizedEmail == normalizedSignInfo);

            if (user == null)
            {
                return Unauthorized();
            }

            var signInResult = await _signInManager
                .CheckPasswordSignInAsync(user, dto.Password, false);

            if (!signInResult.Succeeded)
            {
                return Unauthorized();
            }

            var result = new SignInResultDto
            {
                AccessToken = _tokenService.CreateToken(user),
                RefreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user)
            };

            return result;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateUserAsync()
        {
            var roles = new List<IdentityRole<int>>()
            {
                new IdentityRole<int>()
                {
                    Name = CustomIdentityConstants.Roles.Admin
                },
                new IdentityRole<int>()
                {
                    Name = CustomIdentityConstants.Roles.User
                },
                new IdentityRole<int>()
                {
                    Name = CustomIdentityConstants.Roles.Reviewer
                },
            };
            var tasks = new List<Task>();

            foreach (var role in roles)
            {
                tasks.Add(_roleManager.CreateAsync(role));
            }

            await Task.WhenAll(tasks.ToArray());

            var adminUser = new IdentityUser<int>()
            {
                UserName = "admin",
                Email = "admin@admin.com"
            };

            await _user2Manager.CreateAsync(adminUser, "admin's_pa$$w0rd");

            await _user2Manager.AddToRoleAsync(adminUser, CustomIdentityConstants.Roles.Admin);

            return Ok();
        }
    }
}

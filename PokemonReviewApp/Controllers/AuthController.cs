using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly DatabaseAuthContext _context;

        public AuthController(UserManager<User> userManager,
            SignInManager<User> signInManager,
            TokenService tokenService,
            RefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

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

            if (signInResult.Succeeded)
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
    }
}

using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : BaseApiController
    {
        private readonly AuthService _authService;
        private readonly TokenService _tokenService;

        public AuthApiController(
            AuthService authService,
            TokenService tokenService) : base(tokenService)
        {
            _authService = authService;
            _tokenService = tokenService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] LoginRequest request)
        {
            var (success, user, error) = await _authService.LoginAsync(
                request.Email, request.Password);

            if (!success)
                return Unauthorized(new { error });

            var token = _tokenService.GenerateToken(user!.Id);
            return Ok(new { token, userId = user.Id });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

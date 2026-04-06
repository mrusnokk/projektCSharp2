using Microsoft.AspNetCore.Mvc;
using Web.Services;
namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {
        private readonly TokenService _tokenService;

        public BaseApiController(TokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected int? GetCurrentUserId()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
                return null;

            var token = authHeader.Substring("Bearer ".Length).Trim();
            return _tokenService.ValidateToken(token);
        }

        protected bool IsAuthenticated() => GetCurrentUserId() != null;
    }
}

using Microsoft.AspNetCore.Mvc;
using Web.Repositories;
using Web.Services;

namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : BaseApiController
    {
        private readonly UserRepository _userRepository;

        public UsersApiController(
            UserRepository userRepository,
            TokenService tokenService) : base(tokenService)
        {
            _userRepository = userRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var users = await _userRepository.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "Uživatel nenalezen" });

            return Ok(user);
        }

        [HttpPut("{id}/active")]
        public async Task<IActionResult> SetActive(int id, [FromBody] SetActiveRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { error = "Uživatel nenalezen" });

            await _userRepository.SetActiveAsync(id, request.IsActive);
            return Ok();
        }
    }

    public class SetActiveRequest
    {
        public bool IsActive { get; set; }
    }
}

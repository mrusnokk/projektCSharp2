using Microsoft.AspNetCore.Mvc;
using Web.Repositories;
using Web.Services;

namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/stations")]
    public class StationsApiController : BaseApiController
    {
        private readonly StationRepository _stationRepository;
        private readonly BikeRepository _bikeRepository;

        public StationsApiController(
            StationRepository stationRepository,
            BikeRepository bikeRepository,
            TokenService tokenService) : base(tokenService)
        {
            _stationRepository = stationRepository;
            _bikeRepository = bikeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var stations = await _stationRepository.GetAllAsync();
            return Ok(stations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
                return NotFound(new { error = "Stanoviště nenalezeno" });

            var availableBikes = await _bikeRepository.GetAvailableByStationAsync(id);
            var allBikes = await _bikeRepository.GetByStationAsync(id);

            return Ok(new
            {
                station,
                availableCount = availableBikes.Count(),
                totalCount = allBikes.Count()
            });
        }
    }

}

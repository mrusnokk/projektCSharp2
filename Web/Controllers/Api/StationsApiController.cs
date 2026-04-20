using Microsoft.AspNetCore.Mvc;
using Shared.Models;
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Station station)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            if (string.IsNullOrWhiteSpace(station.Name))
                return BadRequest(new { error = "Název je povinný" });

            var id = await _stationRepository.CreateAsync(station);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Station station)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var existing = await _stationRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Stanoviště nenalezeno" });

            station.Id = id;
            await _stationRepository.UpdateAsync(station);
            return Ok();
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
                return NotFound(new { error = "Stanoviště nenalezeno" });

            await _stationRepository.DeleteAsync(id);
            return Ok();
        }
    }

}

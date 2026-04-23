using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Web.Repositories;
using Web.Services;

namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/bikes")]
    public class BikesApiController : BaseApiController
    {
        private readonly BikeRepository _bikeRepository;
        private readonly BikeStatusHistoryRepository _historyRepository;

        public BikesApiController(
            BikeRepository bikeRepository,
            BikeStatusHistoryRepository historyRepository,
            TokenService tokenService) : base(tokenService)
        {
            _bikeRepository = bikeRepository;
            _historyRepository = historyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string sortBy = "Id", [FromQuery] string sortDir = "ASC")
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var bikes = await _bikeRepository.GetAllAsync(sortBy, sortDir);
            return Ok(bikes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var bike = await _bikeRepository.GetByIdAsync(id);
            if (bike == null)
                return NotFound(new { error = "Kolo nenalezeno" });

            return Ok(bike);
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var history = await _historyRepository.GetByBikeAsync(id);
            return Ok(history);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Bike bike)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            if (string.IsNullOrWhiteSpace(bike.Code))
                return BadRequest(new { error = "Kód kola je povinný" });

            if (string.IsNullOrWhiteSpace(bike.Model))
                return BadRequest(new { error = "Model kola je povinný" });

            if (await _bikeRepository.CodeExistsAsync(bike.Code))
                return BadRequest(new { error = "Kolo s tímto kódem již existuje" });

            bike.Status = "available";
            var id = await _bikeRepository.CreateAsync(bike);
            return Ok(new { id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Bike bike)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var existing = await _bikeRepository.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { error = "Kolo nenalezeno" });

            if (existing.Status == "rented")
                return BadRequest(new { error = "Nelze upravit půjčené kolo" });

            if (await _bikeRepository.CodeExistsAsync(bike.Code, id))
                return BadRequest(new { error = "Kolo s tímto kódem již existuje" });

            bike.Id = id;
            await _bikeRepository.UpdateAsync(bike);
            return Ok();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var bike = await _bikeRepository.GetByIdAsync(id);
            if (bike == null)
                return NotFound(new { error = "Kolo nenalezeno" });

            if (bike.Status == "rented")
                return BadRequest(new { error = "Stav půjčeného kola nelze měnit ručně" });

            var allowed = new[] { "available", "maintenance" };
            if (!allowed.Contains(request.NewStatus))
                return BadRequest(new { error = "Neplatný stav" });

            await _historyRepository.AddAsync(new BikeStatusHistory
            {
                BikeId = id,
                OldStatus = bike.Status,
                NewStatus = request.NewStatus,
                StationId = bike.CurrentStationId,
                Note = request.Note
            });

            await _bikeRepository.UpdateStatusAsync(id, request.NewStatus, bike.CurrentStationId);
            return Ok();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var bike = await _bikeRepository.GetByIdAsync(id);
            if (bike == null)
                return NotFound(new { error = "Kolo nenalezeno" });

            if (bike.Status == "rented")
                return BadRequest(new { error = "Nelze smazat půjčené kolo" });

            await _bikeRepository.DeleteAsync(id);
            return Ok();
        }
    }

    public class UpdateStatusRequest
    {
        public string NewStatus { get; set; }
        public string Note { get; set; }
    }
}

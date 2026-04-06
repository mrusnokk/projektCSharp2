using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Web.Repositories;
using Web.Services;

namespace Web.Controllers.Api
{
    [ApiController]
    [Route("api/rentals")]
    public class RentalsApiController : BaseApiController
    {
        private readonly RentalRepository _rentalRepository;
        private readonly BikeRepository _bikeRepository;
        private readonly BikeStatusHistoryRepository _historyRepository;
        private readonly StationRepository _stationRepository;

        public RentalsApiController(
            RentalRepository rentalRepository,
            BikeRepository bikeRepository,
            BikeStatusHistoryRepository historyRepository,
            StationRepository stationRepository,
            TokenService tokenService) : base(tokenService)
        {
            _rentalRepository = rentalRepository;
            _bikeRepository = bikeRepository;
            _historyRepository = historyRepository;
            _stationRepository = stationRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            if (!IsAuthenticated())
                return Unauthorized(new { error = "Chybí token" });

            var rentals = await _rentalRepository.GetAllAsync();
            return Ok(rentals);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMine()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = "Chybí token" });

            var rentals = await _rentalRepository.GetByUserAsync(userId.Value);
            return Ok(rentals);
        }

        [HttpGet("my/active")]
        public async Task<IActionResult> GetMyActive()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = "Chybí token" });

            var rental = await _rentalRepository.GetActiveByUserAsync(userId.Value);
            if (rental == null)
                return Ok(null);

            return Ok(rental);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRentalRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = "Chybí token" });

            // Zkontroluj jestli uživatel nemá aktivní půjčení
            var activeRental = await _rentalRepository.GetActiveByUserAsync(userId.Value);
            if (activeRental != null)
                return BadRequest(new { error = "Již máte aktivní půjčení" });

            // Zkontroluj kolo
            var bike = await _bikeRepository.GetByIdAsync(request.BikeId);
            if (bike == null)
                return NotFound(new { error = "Kolo nenalezeno" });

            if (bike.Status != "available")
                return BadRequest(new { error = "Kolo není dostupné" });

            // Zkontroluj stanoviště
            var station = await _stationRepository.GetByIdAsync(request.StationId);
            if (station == null)
                return NotFound(new { error = "Stanoviště nenalezeno" });

            // Vytvoř půjčení
            var rental = new Rental
            {
                UserId = userId.Value,
                BikeId = request.BikeId,
                StartStationId = request.StationId
            };

            var rentalId = await _rentalRepository.CreateAsync(rental);

            // Zapiš historii
            await _historyRepository.AddAsync(new BikeStatusHistory
            {
                BikeId = bike.Id,
                OldStatus = bike.Status,
                NewStatus = "rented",
                StationId = request.StationId,
                RentalId = rentalId,
                Note = "Půjčení kola"
            });

            // Aktualizuj stav kola
            await _bikeRepository.UpdateStatusAsync(bike.Id, "rented", null);

            return Ok(new { rentalId });
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> Return(int id, [FromBody] ReturnRentalRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { error = "Chybí token" });

            var rental = await _rentalRepository.GetByIdAsync(id);
            if (rental == null)
                return NotFound(new { error = "Půjčení nenalezeno" });

            if (rental.UserId != userId.Value)
                return Unauthorized(new { error = "Toto není vaše půjčení" });

            if (rental.Status == "completed")
                return BadRequest(new { error = "Půjčení již bylo ukončeno" });

            // Zkontroluj stanoviste vraceni
            var station = await _stationRepository.GetByIdAsync(request.EndStationId);
            if (station == null)
                return NotFound(new { error = "Stanoviště nenalezeno" });

            // Vypocitej dobu a cenu
            var duration = (decimal)(DateTime.UtcNow - rental.StartedAt).TotalMinutes;
            var price = Math.Round(duration / 60 * 30, 2); // 30 Kc za hodinu

            // Ukonceni a pujceni
            await _rentalRepository.ReturnAsync(id, request.EndStationId, duration, price);

            // Zapis historii
            var bike = await _bikeRepository.GetByIdAsync(rental.BikeId);
            await _historyRepository.AddAsync(new BikeStatusHistory
            {
                BikeId = rental.BikeId,
                OldStatus = "rented",
                NewStatus = "available",
                StationId = request.EndStationId,
                RentalId = id,
                Note = "Vrácení kola"
            });

            // Aktualizuj stav kola
            await _bikeRepository.UpdateStatusAsync(rental.BikeId, "available", request.EndStationId);

            return Ok(new { duration, price });
        }
    }

    public class CreateRentalRequest
    {
        public int BikeId { get; set; }
        public int StationId { get; set; }
    }

    public class ReturnRentalRequest
    {
        public int EndStationId { get; set; }
    }
}

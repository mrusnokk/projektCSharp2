using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Web.Repositories;
using Web.ViewModels;

namespace Web.Controllers
{
    public class RentalsController : Controller
    {
        private readonly RentalRepository _rentalRepository;
        private readonly BikeRepository _bikeRepository;
        private readonly StationRepository _stationRepository;
        private readonly BikeStatusHistoryRepository _historyRepository;

        public RentalsController(
            RentalRepository rentalRepository,
            BikeRepository bikeRepository,
            StationRepository stationRepository,
            BikeStatusHistoryRepository historyRepository)
        {
            _rentalRepository = rentalRepository;
            _bikeRepository = bikeRepository;
            _stationRepository = stationRepository;
            _historyRepository = historyRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var rentals = await _rentalRepository.GetByUserAsync(userId.Value);
            return View(rentals.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Rent(int bikeId, int stationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            // Zkontroluj jestli uživatel nemá aktivní půjčení
            var activeRental = await _rentalRepository.GetActiveByUserAsync(userId.Value);
            if (activeRental != null)
            {
                TempData["Error"] = "Již máte aktivní půjčení. Nejdřív vraťte kolo.";
                return RedirectToAction("Index", "Stations");
            }

            var bike = await _bikeRepository.GetByIdAsync(bikeId);
            if (bike == null || bike.Status != "available")
            {
                TempData["Error"] = "Kolo není dostupné.";
                return RedirectToAction("Detail", "Stations", new { id = stationId });
            }

            var station = await _stationRepository.GetByIdAsync(stationId);

            var model = new RentViewModel
            {
                Bike = bike,
                Station = station
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Rent(int bikeId, int stationId, bool confirm)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var bike = await _bikeRepository.GetByIdAsync(bikeId);
            if (bike == null || bike.Status != "available")
            {
                TempData["Error"] = "Kolo již není dostupné.";
                return RedirectToAction("Detail", "Stations", new { id = stationId });
            }

            // Vytvoř půjčení
            var rental = new Rental
            {
                UserId = userId.Value,
                BikeId = bikeId,
                StartStationId = stationId
            };

            var rentalId = await _rentalRepository.CreateAsync(rental);

            // Zapiš historii
            await _historyRepository.AddAsync(new BikeStatusHistory
            {
                BikeId = bikeId,
                OldStatus = bike.Status,
                NewStatus = "rented",
                StationId = stationId,
                RentalId = rentalId,
                Note = "Půjčení kola"
            });

            // Aktualizuj stav kola
            await _bikeRepository.UpdateStatusAsync(bikeId, "rented", null);

            TempData["Success"] = "Kolo bylo úspěšně půjčeno!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Return(int rentalId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental == null || rental.UserId != userId.Value || rental.Status == "completed")
                return RedirectToAction("Index");

            var stations = await _stationRepository.GetAllAsync();

            var model = new ReturnViewModel
            {
                Rental = rental,
                Stations = stations.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Return(int rentalId, int endStationId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var rental = await _rentalRepository.GetByIdAsync(rentalId);
            if (rental == null || rental.UserId != userId.Value || rental.Status == "completed")
                return RedirectToAction("Index");

            // Vypočítej dobu a cenu
            var duration = (decimal)(DateTime.UtcNow - rental.StartedAt).TotalMinutes;
            var price = Math.Round(duration / 60 * 30, 2);

            // Ukonči půjčení
            await _rentalRepository.ReturnAsync(rentalId, endStationId, duration, price);

            // Zapiš historii
            await _historyRepository.AddAsync(new BikeStatusHistory
            {
                BikeId = rental.BikeId,
                OldStatus = "rented",
                NewStatus = "available",
                StationId = endStationId,
                RentalId = rentalId,
                Note = "Vrácení kola"
            });

            // Aktualizuj stav kola
            await _bikeRepository.UpdateStatusAsync(rental.BikeId, "available", endStationId);

            TempData["Success"] = $"Kolo vráceno. Doba půjčení: {Math.Round(duration)} minut, cena: {price} Kč.";
            return RedirectToAction("Index");
        }
    }
}

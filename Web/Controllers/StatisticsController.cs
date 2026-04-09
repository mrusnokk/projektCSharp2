using Microsoft.AspNetCore.Mvc;
using Web.Repositories;

namespace Web.Controllers
{
    public class StatisticsController : Controller
    {
        private readonly StationRepository _stationRepository;
        private readonly RentalRepository _rentalRepository;

        public StatisticsController(
            StationRepository stationRepository,
            RentalRepository rentalRepository)
        {
            _stationRepository = stationRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var stations = await _stationRepository.GetAllAsync();
            return View(stations.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Data(int? month, int? year)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var targetMonth = month ?? DateTime.Now.Month;
            var targetYear = year ?? DateTime.Now.Year;

            var stations = await _stationRepository.GetAllAsync();
            var allRentals = await _rentalRepository.GetAllAsync();

            var rentalsInPeriod = allRentals.Where(r =>
                r.StartedAt.Month == targetMonth && r.StartedAt.Year == targetYear);

            var result = stations.Select(s => new
            {
                stationName = s.Name,
                rentalsStarted = rentalsInPeriod.Count(r => r.StartStationId == s.Id),
                rentalsEnded = rentalsInPeriod.Count(r => r.EndStationId == s.Id && r.Status == "completed")
            });

            return Json(new
            {
                month = targetMonth,
                year = targetYear,
                data = result
            });
        }
    }
}

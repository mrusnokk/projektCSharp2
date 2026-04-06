using Microsoft.AspNetCore.Mvc;
using Web.Repositories;
using Web.ViewModels;

namespace Web.Controllers
{
    public class StationsController : Controller
    {
        private readonly StationRepository _stationRepository;
        private readonly BikeRepository _bikeRepository;

        public StationsController(
            StationRepository stationRepository,
            BikeRepository bikeRepository)
        {
            _stationRepository = stationRepository;
            _bikeRepository = bikeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var stations = await _stationRepository.GetAllAsync();

            var stationViewModels = new List<StationViewModel>();
            foreach (var station in stations)
            {
                var availableBikes = await _bikeRepository.GetAvailableByStationAsync(station.Id);
                var allBikes = await _bikeRepository.GetByStationAsync(station.Id);
                stationViewModels.Add(new StationViewModel
                {
                    Station = station,
                    AvailableCount = availableBikes.Count(),
                    TotalCount = allBikes.Count()
                });
            }

            return View(stationViewModels);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null)
                return NotFound();

            var availableBikes = await _bikeRepository.GetAvailableByStationAsync(id);
            var allBikes = await _bikeRepository.GetByStationAsync(id);

            var model = new StationDetailViewModel
            {
                Station = station,
                AvailableBikes = availableBikes.ToList(),
                TotalCount = allBikes.Count()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AvailabilityPartial(int id)
        {
            var availableBikes = await _bikeRepository.GetAvailableByStationAsync(id);
            var allBikes = await _bikeRepository.GetByStationAsync(id);

            return Json(new
            {
                availableCount = availableBikes.Count(),
                totalCount = allBikes.Count()
            });
        }
    }
}

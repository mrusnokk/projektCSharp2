using Microsoft.AspNetCore.Mvc;
using Web.Repositories;
using Web.ViewModels;

namespace Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly RentalRepository _rentalRepository;

        public ProfileController(
            UserRepository userRepository,
            RentalRepository rentalRepository)
        {
            _userRepository = userRepository;
            _rentalRepository = rentalRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = await _userRepository.GetByIdAsync(userId.Value);
            var rentals = await _rentalRepository.GetByUserAsync(userId.Value);

            var model = new ProfileViewModel
            {
                User = user,
                TotalRentals = rentals.Count(),
                ActiveRental = rentals.FirstOrDefault(r => r.Status == "active"),
                TotalSpent = rentals.Where(r => r.Price.HasValue).Sum(r => r.Price.Value)
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Stats()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var rentals = await _rentalRepository.GetByUserAsync(userId.Value);

            return Json(new
            {
                totalRentals = rentals.Count(),
                completedRentals = rentals.Count(r => r.Status == "completed"),
                activeRental = rentals.Any(r => r.Status == "active"),
                totalSpent = rentals.Where(r => r.Price.HasValue).Sum(r => r.Price.Value),
                totalMinutes = rentals.Where(r => r.DurationMinutes.HasValue).Sum(r => r.DurationMinutes.Value)
            });
        }
    }
}

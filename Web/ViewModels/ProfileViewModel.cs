using Shared.Models;

namespace Web.ViewModels
{
    public class ProfileViewModel
    {
        public User User { get; set; }
        public int TotalRentals { get; set; }
        public Rental ActiveRental { get; set; }
        public decimal TotalSpent { get; set; }
    }
}

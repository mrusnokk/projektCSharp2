using Shared.Models;

namespace Web.ViewModels
{
    public class ReturnViewModel
    {
        public Rental Rental { get; set; }
        public List<Station> Stations { get; set; }
    }
}

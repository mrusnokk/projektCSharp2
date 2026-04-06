using Shared.Models;

namespace Web.ViewModels
{
    public class StationDetailViewModel
    {
        public Station Station { get; set; }
        public List<Bike> AvailableBikes { get; set; }
        public int TotalCount { get; set; }
    }
}

using Shared.Models;

namespace Web.ViewModels
{
    public class StationViewModel
    {
        public Station Station { get; set; }
        public int AvailableCount { get; set; }
        public int TotalCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class BikeStatusHistory
    {
        public int Id { get; set; }
        public int BikeId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public int? StationId { get; set; }
        public int? RentalId { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.Now;
        public string Note { get; set; }
    }
}

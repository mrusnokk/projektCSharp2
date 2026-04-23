using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BikeId { get; set; }
        public int StartStationId { get; set; }
        public int? EndStationId { get; set; }
        public DateTime StartedAt { get; set; } = DateTime.Now;
        public DateTime? EndedAt { get; set; }
        public decimal? DurationMinutes { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
    }
}

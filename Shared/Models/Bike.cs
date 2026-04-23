namespace Shared.Models
{
    public class Bike
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Model { get; set; }
        public int? CurrentStationId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
    

}

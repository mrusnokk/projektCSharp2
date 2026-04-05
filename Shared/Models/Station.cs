using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class Station
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Lat { get; set; }
        public decimal Lng { get; set; }
        public string Address { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
    }
}

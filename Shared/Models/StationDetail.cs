using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public class StationDetail
    {
        public Station Station { get; set; }
        public int AvailableCount { get; set; }
        public int TotalCount { get; set; }
    }
}

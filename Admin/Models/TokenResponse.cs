using System;
using System.Collections.Generic;
using System.Text;

namespace Admin.Models
{
    public class TokenResponse
    {
        [Newtonsoft.Json.JsonProperty("token")]
        public string? Token { get; set; }
    }
}
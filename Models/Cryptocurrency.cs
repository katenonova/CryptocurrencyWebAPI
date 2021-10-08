using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebAPI.Models
{
    public class Cryptocurrency
    {
        [JsonPropertyName("date")]
        public DateTime date { get; set; }
        [JsonPropertyName("metrics")]
        public Dictionary<string, string> metrics { get; set; }
    }
}

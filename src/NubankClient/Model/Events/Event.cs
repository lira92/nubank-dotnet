using Newtonsoft.Json;
using NubankClient.Converters;
using System;

namespace NubankClient.Model.Events
{
    public class Event
    {
        public string Description { get; set; }
        [JsonConverter(typeof(TolerantEnumConverter))]
        public EventCategory Category { get; set; }
        public decimal Amount { get; set; }
        public decimal CurrencyAmount => Amount / 100;
        public DateTime Time { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
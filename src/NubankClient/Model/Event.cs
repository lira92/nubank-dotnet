using System;

namespace NubankClient.Model
{
    public class Event
    {
        public string Description { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Time { get; set; }
        public string Title { get; set; }
    }
}
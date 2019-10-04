using Newtonsoft.Json;
using NubankClient.Converters;
using System;
using System.Diagnostics;

namespace NubankClient.Model.Savings
{
    [DebuggerDisplay("{PostDate} - {Title} - {Amount} - {TypeName}")]
    public class Saving
    {
        public string Id { get; set; }
        [JsonProperty("__typename")]
        [JsonConverter(typeof(TolerantEnumConverter))]
        public SavingType TypeName { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public DateTime PostDate { get; set; }
        public decimal? Amount { get; set; }
        public Account OriginAccount { get; set; }
        public Account DestinationAccount { get; set; }
    }

    public class Account
    {
        public string Name { get; set; }
    }
}

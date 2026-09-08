using System;

namespace TasaPlus.Shared.Models
{
    public class CalculoModel
    {
        public decimal ForeignAmount { get; set; } = 1.00m;
        public string CurrencyCode { get; set; } = "USD";
        public decimal VesAmount { get; set; }
        public decimal AppliedRateVes { get; set; }
        public decimal AppliedRateUsd { get; set; } = 1.00m;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string FormattedTimestamp => Timestamp.ToString("HH:mm:ss");
    }
}

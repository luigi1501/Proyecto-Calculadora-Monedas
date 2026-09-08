using System.Text.Json.Serialization;

namespace DolarMonitorAPI.Models
{
    public class TasaDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("rateVes")]
        public decimal RateVes { get; set; }

        [JsonPropertyName("rateUsd")]
        public decimal? RateUsd { get; set; }

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = "$";

        [JsonPropertyName("symbolVes")]
        public string SymbolVes { get; set; } = "Bs";

        [JsonPropertyName("type")]
        public string Type { get; set; } = "fiat";

        [JsonPropertyName("formattedVes")]
        public string FormattedVes { get; set; } = string.Empty;

        [JsonPropertyName("formattedUsd")]
        public string FormattedUsd { get; set; } = string.Empty;

        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonPropertyName("fechaActualizacion")]
        public string FechaActualizacion { get; set; } = string.Empty;
    }
}

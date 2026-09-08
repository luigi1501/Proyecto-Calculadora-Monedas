using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TasaPlus.Shared.Services
{
    public class BinanceService
    {
        private readonly HttpClient _httpClient;
        private static readonly CultureInfo CultureUS = new CultureInfo("en-US");

        public BinanceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Obtiene el precio Spot global en $ USD para una criptomoneda desde Binance Spot v3 API.
        /// Ejemplo: BTCUSDT, ETHUSDT, BNBUSDT.
        /// </summary>
        public async Task<decimal> GetBinanceSpotPriceUsdAsync(string symbol)
        {
            try
            {
                var url = $"https://api.binance.com/api/v3/ticker/price?symbol={symbol}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("price", out var priceElem) &&
                        decimal.TryParse(priceElem.GetString(), NumberStyles.Any, CultureUS, out var price))
                    {
                        return price;
                    }
                }
            }
            catch
            {
                // Fallback silencioso
            }
            return 0m;
        }

        /// <summary>
        /// Consulta la tasa real de USDT en Bolívares (VES) a través de la API Binance P2P v2.
        /// Si la llamada directa a Binance P2P falla, realiza fallback resiliente a DolarApi paralelo / oficial.
        /// </summary>
        public async Task<decimal> GetUsdtP2PRateVesAsync(decimal usdBcvFallback)
        {
            try
            {
                var p2pUrl = "https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search";
                var payload = new
                {
                    asset = "USDT",
                    fiat = "VES",
                    merchantCheck = false,
                    page = 1,
                    payTypes = new string[] { },
                    publisherType = (string?)null,
                    rows = 5,
                    tradeType = "BUY"
                };

                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var request = new HttpRequestMessage(HttpMethod.Post, p2pUrl);
                request.Content = content;
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("data", out var dataArr) && dataArr.ValueKind == JsonValueKind.Array)
                    {
                        decimal sumPrices = 0m;
                        int count = 0;
                        foreach (var item in dataArr.EnumerateArray())
                        {
                            if (item.TryGetProperty("adv", out var adv) &&
                                adv.TryGetProperty("price", out var priceElem) &&
                                decimal.TryParse(priceElem.GetString(), NumberStyles.Any, CultureUS, out var price) &&
                                price > 0)
                            {
                                sumPrices += price;
                                count++;
                            }
                        }

                        if (count > 0)
                        {
                            return Math.Round(sumPrices / count, 2);
                        }
                    }
                }
            }
            catch
            {
                // Ignorar error y proceder al fallback resiliente
            }

            // Fallback resiliente paralelo
            try
            {
                var resParallel = await _httpClient.GetAsync("https://ve.dolarapi.com/v1/dolares/paralelo");
                if (resParallel.IsSuccessStatusCode)
                {
                    var json = await resParallel.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("promedio", out var promElem))
                    {
                        return promElem.GetDecimal();
                    }
                }
            }
            catch { }

            return usdBcvFallback > 0 ? usdBcvFallback : 800.00m;
        }
    }
}

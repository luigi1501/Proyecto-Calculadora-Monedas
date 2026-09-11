using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TasaPlus.Shared.Models;

namespace TasaPlus.Shared.Services
{
    public class TasasDataService
    {
        private readonly HttpClient _httpClient;
        private readonly BinanceService _binanceService;
        private static readonly CultureInfo CultureVE = new CultureInfo("es-VE");
        private static readonly CultureInfo CultureUS = new CultureInfo("en-US");

        private List<TasaModel> _currentTasas = GetDefaultInitialTasas();
        private DateTime _lastUpdated = DateTime.MinValue;
        private readonly SemaphoreSlim _refreshLock = new SemaphoreSlim(1, 1);
        private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(20);

        private PeriodicTimer? _periodicTimer;
        private CancellationTokenSource? _cts;

        public event Action? OnTasasUpdated;

        public TasasDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _binanceService = new BinanceService(httpClient);
        }

        private static List<TasaModel> GetDefaultInitialTasas()
        {
            var now = DateTime.Now;
            string fechaFormatted = now.ToString("dd/MM/yyyy HH:mm:ss", CultureUS);
            return new List<TasaModel>
            {
                new TasaModel { Code = "USD", Name = "Dólar BCV", RateVes = 820.10m, RateUsd = 1.00m, Symbol = "$", SymbolVes = "Bs", Type = "fiat", FormattedVes = "820,10 Bs", FormattedUsd = "$1,00 USD", LastUpdated = now, FechaActualizacion = fechaFormatted },
                new TasaModel { Code = "EUR", Name = "Euro BCV", RateVes = 954.02m, RateUsd = 1.16m, Symbol = "€", SymbolVes = "Bs", Type = "fiat", FormattedVes = "954,02 Bs", FormattedUsd = "€1,00 EUR", LastUpdated = now, FechaActualizacion = fechaFormatted },
                new TasaModel { Code = "USDT", Name = "USDT (Binance P2P)", RateVes = 963.72m, RateUsd = 1.00m, Symbol = "USDT", SymbolVes = "Bs", Type = "crypto", FormattedVes = "963,72 Bs", FormattedUsd = "$1,00 USD", LastUpdated = now, FechaActualizacion = fechaFormatted },
                new TasaModel { Code = "BNB", Name = "Binance Coin", RateVes = 520000.00m, RateUsd = 650.00m, Symbol = "BNB", SymbolVes = "Bs", Type = "crypto", FormattedVes = "520.000,00 Bs", FormattedUsd = "$650,00 USD", LastUpdated = now, FechaActualizacion = fechaFormatted },
                new TasaModel { Code = "BTC", Name = "Bitcoin", RateVes = 75000000.00m, RateUsd = 90000.00m, Symbol = "₿", SymbolVes = "Bs", Type = "crypto", FormattedVes = "75.000.000,00 Bs", FormattedUsd = "$90.000,00 USD", LastUpdated = now, FechaActualizacion = fechaFormatted },
                new TasaModel { Code = "ETH", Name = "Ethereum", RateVes = 2500000.00m, RateUsd = 3000.00m, Symbol = "Ξ", SymbolVes = "Bs", Type = "crypto", FormattedVes = "2.500.000,00 Bs", FormattedUsd = "$3.000,00 USD", LastUpdated = now, FechaActualizacion = fechaFormatted }
            };
        }

        public List<TasaModel> GetCurrentTasas() => _currentTasas;
        public DateTime GetLastUpdated() => _lastUpdated;
        public string GetFormattedLastUpdated() => _lastUpdated == DateTime.MinValue ? "--:--:--" : _lastUpdated.ToString("HH:mm:ss");

        public void StartPeriodicRefresh(TimeSpan interval)
        {
            StopPeriodicRefresh();
            _cts = new CancellationTokenSource();
            _periodicTimer = new PeriodicTimer(interval);

            Task.Run(async () =>
            {
                while (_periodicTimer != null && await _periodicTimer.WaitForNextTickAsync(_cts.Token))
                {
                    await RefreshTasasAsync(forceRefresh: true);
                }
            }, _cts.Token);
        }

        public void StopPeriodicRefresh()
        {
            _cts?.Cancel();
            _periodicTimer?.Dispose();
            _periodicTimer = null;
        }

        public async Task<List<TasaModel>> RefreshTasasAsync(bool forceRefresh = false)
        {
            // Cache en memoria: si los datos tienen menos de 20s y no se forzó el refresco, retornar al instante (0ms)
            if (!forceRefresh && _currentTasas.Count > 0 && (DateTime.Now - _lastUpdated) < CacheDuration)
            {
                return _currentTasas;
            }

            await _refreshLock.WaitAsync();
            try
            {
                // Re-verificar cache despues de adquirir el candado
                if (!forceRefresh && _currentTasas.Count > 0 && (DateTime.Now - _lastUpdated) < CacheDuration)
                {
                    return _currentTasas;
                }

                // 🚀 Ejecutar todas las llamadas HTTP externamente en PARALELO con Task.WhenAll
                var tBcvUsd = FetchBcvUsdAsync();
                var tBcvEur = FetchBcvEurAsync();
                var tBtcUsd = _binanceService.GetBinanceSpotPriceUsdAsync("BTCUSDT");
                var tEthUsd = _binanceService.GetBinanceSpotPriceUsdAsync("ETHUSDT");
                var tBnbUsd = _binanceService.GetBinanceSpotPriceUsdAsync("BNBUSDT");

                await Task.WhenAll(tBcvUsd, tBcvEur, tBtcUsd, tEthUsd, tBnbUsd);

                decimal usdBcvRate = tBcvUsd.Result;
                decimal eurBcvRate = tBcvEur.Result;
                decimal btcRateUsd = tBtcUsd.Result;
                decimal ethRateUsd = tEthUsd.Result;
                decimal bnbRateUsd = tBnbUsd.Result;

                // Tasa USDT en Bolívares tomada de Binance P2P
                decimal usdtRateVes = await _binanceService.GetUsdtP2PRateVesAsync(usdBcvRate);
                decimal usdtRateUsd = 1.00m;

                decimal btcRateVes = btcRateUsd * usdtRateVes;
                decimal ethRateVes = ethRateUsd * usdtRateVes;
                decimal bnbRateVes = bnbRateUsd * usdtRateVes;

                var now = DateTime.Now;
                string fechaFormatted = now.ToString("dd/MM/yyyy HH:mm:ss", CultureUS);

                _currentTasas = new List<TasaModel>
                {
                    new TasaModel
                    {
                        Code = "USD",
                        Name = "Dólar BCV",
                        RateVes = usdBcvRate,
                        RateUsd = 1.00m,
                        Symbol = "$",
                        SymbolVes = "Bs",
                        Type = "fiat",
                        FormattedVes = $"{usdBcvRate.ToString("N2", CultureVE)} Bs",
                        FormattedUsd = "$1,00 USD",
                        LastUpdated = now,
                        FechaActualizacion = fechaFormatted
                    },
                    new TasaModel
                    {
                        Code = "EUR",
                        Name = "Euro BCV",
                        RateVes = eurBcvRate,
                        RateUsd = (usdBcvRate > 0) ? Math.Round(eurBcvRate / usdBcvRate, 4) : 1.08m,
                        Symbol = "€",
                        SymbolVes = "Bs",
                        Type = "fiat",
                        FormattedVes = $"{eurBcvRate.ToString("N2", CultureVE)} Bs",
                        FormattedUsd = "€1,00 EUR",
                        LastUpdated = now,
                        FechaActualizacion = fechaFormatted
                    },
                    new TasaModel
                    {
                        Code = "USDT",
                        Name = "USDT (Binance P2P)",
                        RateVes = usdtRateVes,
                        RateUsd = usdtRateUsd,
                        Symbol = "USDT",
                        SymbolVes = "Bs",
                        Type = "crypto",
                        FormattedVes = $"{usdtRateVes.ToString("N2", CultureVE)} Bs",
                        FormattedUsd = "$1,00 USD",
                        LastUpdated = now,
                        FechaActualizacion = fechaFormatted
                    },
                    new TasaModel
                    {
                        Code = "BNB",
                        Name = "Binance Coin",
                        RateVes = bnbRateVes,
                        RateUsd = bnbRateUsd,
                        Symbol = "BNB",
                        SymbolVes = "Bs",
                        Type = "crypto",
                        FormattedVes = $"{bnbRateVes.ToString("N2", CultureVE)} Bs",
                        FormattedUsd = $"${bnbRateUsd.ToString("N2", CultureVE)} USD",
                        LastUpdated = now,
                        FechaActualizacion = fechaFormatted
                    },
                    new TasaModel
                    {
                        Code = "BTC",
                        Name = "Bitcoin",
                        RateVes = btcRateVes,
                        RateUsd = btcRateUsd,
                        Symbol = "₿",
                        SymbolVes = "Bs",
                        Type = "crypto",
                        FormattedVes = $"{btcRateVes.ToString("N2", CultureVE)} Bs",
                        FormattedUsd = $"${btcRateUsd.ToString("N2", CultureVE)} USD",
                        LastUpdated = now,
                        FechaActualizacion = fechaFormatted
                    },
                    new TasaModel
                    {
                        Code = "ETH",
                        Name = "Ethereum",
                        RateVes = ethRateVes,
                        RateUsd = ethRateUsd,
                        Symbol = "Ξ",
                        SymbolVes = "Bs",
                        Type = "crypto",
                        FormattedVes = $"{ethRateVes.ToString("N2", CultureVE)} Bs",
                        FormattedUsd = $"${ethRateUsd.ToString("N2", CultureVE)} USD",
                        LastUpdated = now,
                        FechaActualizacion = fechaFormatted
                    }
                };

                _lastUpdated = now;
                OnTasasUpdated?.Invoke();
                return _currentTasas;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private async Task<decimal> FetchBcvUsdAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("https://ve.dolarapi.com/v1/dolares/oficial");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("promedio", out var promElem))
                    {
                        return promElem.GetDecimal();
                    }
                }
            }
            catch { }
            return 0m;
        }

        private async Task<decimal> FetchBcvEurAsync()
        {
            try
            {
                var res = await _httpClient.GetAsync("https://ve.dolarapi.com/v1/euros/oficial");
                if (res.IsSuccessStatusCode)
                {
                    var json = await res.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("promedio", out var promElem))
                    {
                        return promElem.GetDecimal();
                    }
                }
            }
            catch { }
            return 0m;
        }
    }
}

export default async function handler(req, res) {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Cache-Control', 's-maxage=30, stale-while-revalidate=60');

  try {
    const [bcvUsdRes, bcvEurRes, btcRes, ethRes, bnbRes] = await Promise.allSettled([
      fetch('https://ve.dolarapi.com/v1/dolares/oficial').then(r => r.json()),
      fetch('https://ve.dolarapi.com/v1/euros/oficial').then(r => r.json()),
      fetch('https://api.binance.com/api/v3/ticker/price?symbol=BTCUSDT').then(r => r.json()),
      fetch('https://api.binance.com/api/v3/ticker/price?symbol=ETHUSDT').then(r => r.json()),
      fetch('https://api.binance.com/api/v3/ticker/price?symbol=BNBUSDT').then(r => r.json()),
    ]);

    const usdBcvRate = bcvUsdRes.status === 'fulfilled' && bcvUsdRes.value?.promedio ? Number(bcvUsdRes.value.promedio) : 0;
    const eurBcvRate = bcvEurRes.status === 'fulfilled' && bcvEurRes.value?.promedio ? Number(bcvEurRes.value.promedio) : 0;
    const btcUsd = btcRes.status === 'fulfilled' && btcRes.value?.price ? parseFloat(btcRes.value.price) : 0;
    const ethUsd = ethRes.status === 'fulfilled' && ethRes.value?.price ? parseFloat(ethRes.value.price) : 0;
    const bnbUsd = bnbRes.status === 'fulfilled' && bnbRes.value?.price ? parseFloat(bnbRes.value.price) : 0;

    let usdtRateVes = usdBcvRate;
    try {
      const p2pRes = await fetch('https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'User-Agent': 'Mozilla/5.0' },
        body: JSON.stringify({ asset: 'USDT', fiat: 'VES', merchantCheck: false, page: 1, payTypes: [], publisherType: null, rows: 5, tradeType: 'BUY' })
      }).then(r => r.json());

      if (p2pRes.data && Array.isArray(p2pRes.data) && p2pRes.data.length > 0) {
        const prices = p2pRes.data
          .map(item => parseFloat(item.adv?.price))
          .filter(p => !isNaN(p) && p > 0);
        if (prices.length > 0) {
          usdtRateVes = prices.reduce((a, b) => a + b, 0) / prices.length;
        }
      }
    } catch (e) {
      console.warn('Binance P2P fallback used', e);
    }

    const btcRateVes = btcUsd * usdtRateVes;
    const ethRateVes = ethUsd * usdtRateVes;
    const bnbRateVes = bnbUsd * usdtRateVes;

    const now = new Date();
    const fechaFormatted = now.toLocaleDateString('es-VE') + ' ' + now.toLocaleTimeString('es-VE');

    const formatVes = val => (val || 0).toLocaleString('es-VE', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' Bs';
    const formatUsd = val => '$' + (val || 0).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + ' USD';

    const tasas = [
      { code: 'USD', name: 'Dólar BCV', rateVes: usdBcvRate, rateUsd: 1.0, symbol: '$', symbolVes: 'Bs', type: 'fiat', formattedVes: formatVes(usdBcvRate), formattedUsd: '$1,00 USD', lastUpdated: now, fechaActualizacion: fechaFormatted },
      { code: 'EUR', name: 'Euro BCV', rateVes: eurBcvRate, rateUsd: usdBcvRate > 0 ? Number((eurBcvRate / usdBcvRate).toFixed(4)) : 1.08, symbol: '€', symbolVes: 'Bs', type: 'fiat', formattedVes: formatVes(eurBcvRate), formattedUsd: '€1,00 EUR', lastUpdated: now, fechaActualizacion: fechaFormatted },
      { code: 'USDT', name: 'USDT (Binance P2P)', rateVes: usdtRateVes, rateUsd: 1.0, symbol: 'USDT', symbolVes: 'Bs', type: 'crypto', formattedVes: formatVes(usdtRateVes), formattedUsd: '$1,00 USD', lastUpdated: now, fechaActualizacion: fechaFormatted },
      { code: 'BNB', name: 'Binance Coin', rateVes: bnbRateVes, rateUsd: bnbUsd, symbol: 'BNB', symbolVes: 'Bs', type: 'crypto', formattedVes: formatVes(bnbUsd), formattedUsd: formatUsd(bnbUsd), lastUpdated: now, fechaActualizacion: fechaFormatted },
      { code: 'BTC', name: 'Bitcoin', rateVes: btcRateVes, rateUsd: btcUsd, symbol: '₿', symbolVes: 'Bs', type: 'crypto', formattedVes: formatVes(btcRateVes), formattedUsd: formatUsd(btcUsd), lastUpdated: now, fechaActualizacion: fechaFormatted },
      { code: 'ETH', name: 'Ethereum', rateVes: ethRateVes, rateUsd: ethUsd, symbol: 'Ξ', symbolVes: 'Bs', type: 'crypto', formattedVes: formatVes(ethRateVes), formattedUsd: formatUsd(ethUsd), lastUpdated: now, fechaActualizacion: fechaFormatted }
    ];

    res.status(200).json(tasas);
  } catch (error) {
    res.status(500).json({ error: 'Failed to fetch tasas', details: error.message });
  }
}

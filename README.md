# 🚀 TasaPlus — Monitor Inteligente y Calculadora Financiera

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-emerald.svg)](LICENSE)
[![Vercel Ready](https://img.shields.io/badge/Vercel-Deployed-black?logo=vercel)](https://vercel.com)
[![Platforms](https://img.shields.io/badge/Platforms-Web_%7C_Android_%7C_Windows-06B6D4)](#-multiplataforma)

**TasaPlus** es una solución financiera multiplataforma diseñada para unificar en un solo lugar las cotizaciones oficiales del **Banco Central de Venezuela (BCV)** y el mercado de criptomonedas **Binance (Spot & P2P)** en tiempo real.

---

## 🌟 Características Principales

- ⚡ **Cotizaciones en Tiempo Real:** Sincronización automática de USD BCV, EUR BCV, USDT (Binance P2P), Bitcoin (BTC), Ethereum (ETH) y Binance Coin (BNB).
- 🇻🇪 **Formato Regional Venezolano (`es-VE`):** Todos los montos en Bolívares y Dólares utilizan el formato estándar con **punto (`.`) para miles** y **coma (`,`) para decimales** (ejemplo: `2.456.821,78 Bs`).
- 🧮 **Calculadora Bidireccional Inteligente:** Convierte instantáneamente de Bolívares a Moneda Extranjera/Cripto y viceversa.
- 📸 **Generador de Comprobantes:** Exporta e imprime comprobantes de cotización en imagen HD (PNG) y compártelos en 1 clic por WhatsApp o Telegram.
- 🛡️ **Guía P2P de Seguridad:** Consejos integrados para evitar fraudes en transferencias entre personas.
- 🟡 **Integración de Referidos Binance:** Enlace directo de registro oficial con beneficio del 10% de descuento en comisiones (Código: `luigix`).

---

## 💻 Multiplataforma

- 🌐 **Web & PWA:** Compatible con cualquier navegador moderno y optimizado para **Vercel**.
- 📱 **Android (.APK):** Compilado nativo mediante .NET MAUI / Capacitor.
- 🖥️ **Windows (.EXE):** Aplicación de escritorio independiente ultrarrápida.

---

## 🛠️ Tecnologías Utilizadas

- **Backend:** C# ASP.NET Core 10 Web API
- **Librería Compartida:** Blazor Components (`TasaPlus.Shared`)
- **Frontend Web:** HTML5, Tailwind CSS, JavaScript (ES6+), Font Inter
- **APIs de Datos:** API Oficial BCV (`ve.dolarapi.com`), Binance Spot API & Binance P2P API

---

## 🚀 Ejecución en Entorno Local

### 1. Iniciar la API Backend

```bash
dotnet run --project backend/DolarMonitorAPI.csproj
```
*La API iniciará en `http://localhost:5000` sirviendo las tasas en tiempo real y la Landing Page.*

### 2. Compilar Librería Compartida

```bash
dotnet build TasaPlus.Shared/TasaPlus.Shared.csproj
```

---

## 📦 Despliegue en Vercel

El proyecto incluye la configuración [vercel.json](vercel.json) lista para producción:

1. Haz push de este repositorio a tu cuenta de **GitHub**.
2. Conecta el repositorio en [Vercel](https://vercel.com).
3. Vercel desplegará automáticamente la Landing Page y la Calculadora Web.

---

## 📄 Licencia y Créditos

Desarrollado con ❤️ para comerciantes y usuarios en Venezuela. Licencia MIT.
- Enlace oficial de referido Binance: [Registrarse en Binance](https://accounts.binance.com/register?ref=948971471) (Código: `luigix`)

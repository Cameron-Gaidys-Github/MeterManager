# MeterManager Web Portal

The **MeterManager** ASP.NET site provides a web interface for viewing, analyzing, and managing real-time water flow data collected by the EtherMeterService. It visualizes live flow rates from multiple meters across a resort or facility and offers functionality such as zoomable graphs, historical data inspection, and system monitoring.

---

## 🔧 Features

- 📈 **Live Flow Graph** — Real-time chart displaying per-meter flow rates
- 🔍 **Zoom & Pan** — Interactive zoom and pan support using Chart.js plugin
- 📊 **Expandable** — Easy to add new meters or visualizations
- 🔄 **Auto-Update** — Graph data refreshes every second from `/Api/ActiveFlow`
- 🧰 **Backend Integration** — Connects to EtherMeterService for meter data
- 📁 **Lightweight Setup** — Built on ASP.NET Core Razor Pages

---

## 📂 Project Structure

MeterManager/
│
├── Pages/
│ ├── Index.cshtml
│ └── LiveGraph.cshtml ← Main graph view
│
├── Api/
│ └── ActiveFlowController.cs ← Returns live meter flow JSON
│
├── Models/
│ └── EtherMeterReading.cs ← Data structure for meter readings
│
├── wwwroot/
│ └── (Static content like JS/CSS)
│
└── MeterManager.csproj

yaml
Copy
Edit

---

## 🧪 API Endpoint

- **`GET /Api/ActiveFlow`**
  - Returns a list of active meters and their current flow rates in JSON.
  - Expected response format:
    ```json
    [
      {
        "meterID": "1",
        "flowRate": 12.5
      },
      {
        "meterID": "2",
        "flowRate": 9.3
      }
    ]
    ```

---

## 🚀 Getting Started

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/MeterManager.git
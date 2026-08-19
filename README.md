# Rakawatch

Web server untuk serve data telemetry hardware live dari **LibreHardwareMonitor** sebagai JSON. Berbasis ASP.NET Core (Kestrel) di .NET 10.

## Fitur

- Serve snapshot lengkap semua sensor hardware (CPU, GPU, RAM, motherboard, storage, network, fan, dan lainnya) sebagai JSON.
- Endpoint full snapshot + per kategori + detail per hardware.
- Data di-sample secara periodik oleh background service (default tiap 1 detik). Snapshot diserve dari hasil sampling, sehingga aman untuk request paralel (LibreHardwareMonitor tidak thread-safe).
- Optional auth token: jika `AUTH_TOKEN` di-set, semua endpoint wajib menyertakan `Authorization: Bearer <token>`.
- OpenAPI spec otomatis di `/openapi/v1.json`.
- Enforce hak Administrator: jika tidak dijalankan sebagai admin, muncul notifikasi dan aplikasi langsung keluar.
- CORS enabled (bisa dikonsumsi frontend/browser).
- HTTP request logging aktif.
- Host, port, dan interval sampling bisa dikonfigurasi via environment variable.

## Requirements

- Windows (LibreHardwareMonitor hanya berjalan di Windows)
- .NET 10 SDK
- **Harus dijalankan sebagai Administrator** untuk membaca sensor hardware

## Cara Menjalankan

```powershell
dotnet run
```

Aplikasi akan listen di `http://localhost:8080`.

### Konfigurasi

| Environment variable | Default | Keterangan |
|---|---|---|
| `HOST` | `localhost` | Host yang di-bind |
| `PORT` | `8080` | Port HTTP |
| `SAMPLE_INTERVAL_MS` | `1000` | Interval sampling hardware dalam milidetik (nilai < 1 dianggap 1000) |
| `AUTH_TOKEN` | *(kosong)* | Jika diisi, semua endpoint butuh header `Authorization: Bearer <token>` |

## Endpoint

| Method | Path | Keterangan |
|---|---|---|
| `GET` | `/` | Status: versi, timestamp, daftar endpoint, jumlah hardware per tipe |
| `GET` | `/api/hardware` | Full snapshot semua hardware + sensor |
| `GET` | `/api/hardware/{type}` | Snapshot per kategori |
| `GET` | `/api/hardware/{type}/{name}` | Detail satu hardware |
| `GET` | `/openapi/v1.json` | OpenAPI spec |

### Kategori `{type}`

`cpu`, `gpu` (NVIDIA + AMD + Intel), `memory`, `motherboard`, `storage`, `network`, `battery`, `controller`, `psu`, `power`. Nama enum `HardwareType` juga diterima (mis. `GpuNvidia`).

### Bentuk JSON

```json
[
  {
    "id": "/intelcpu/0",
    "name": "12th Gen Intel Core i5-12400",
    "type": "Cpu",
    "sensors": [
      { "id": "/intelcpu/0/temperature/0", "name": "Core (Tctl/Tdie)", "type": "Temperature", "value": 45.0, "unit": "°C", "min": 40.0, "max": 100.0, "index": 0 }
    ],
    "subHardware": []
  }
]
```

> Catatan: `unit` adalah satuan nilai sensor yang diturunkan dari tipe sensor (mis. `°C`, `%`, `RPM`, `W`). Nilai `min`/`max` adalah minimum/maksimum kumulatif sejak aplikasi berjalan (bukan per-request). Sensor yang bernilai `NaN`/`Infinity` diserialisasi sebagai `null`.

## Struktur Proyek

```
rakawatch/
├── Program.cs                    → bootstrap ASP.NET Core + konfigurasi Kestrel + auth token + logging
├── AdminGuard.cs                 → check hak Administrator + notifikasi
├── Controllers/
│   └── HardwareController.cs     → endpoint API
├── Services/
│   ├── HardwareMonitorService.cs → wrapper singleton Computer (LibreHardwareMonitor)
│   └── HardwareSamplerService.cs → background service sampling periodik
└── Models/
    └── HardwareDtos.cs           → DTO record
```

## Teknologi

- .NET 10 (`net10.0-windows`)
- ASP.NET Core Minimal Hosting (Kestrel)
- LibreHardwareMonitorLib
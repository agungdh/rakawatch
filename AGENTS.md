# AGENTS.md

Panduan untuk AI agent (dan kontributor) yang bekerja di repo ini.

## Proyek

Rakawatch = web server JSON telemetry hardware dari LibreHardwareMonitor (LHM). Windows-only, .NET 10.

## Build / verifikasi

- Build: `dotnet build`
- Restore: `dotnet restore`
- **Tidak ada test suite** di repo ini. CI (`.github/workflows/build.yml`) hanya menjalankan `dotnet test` (kosong). Jangan menambahkan test framework tanpa persetujuan.
- Build & test jalan di mesin Windows.

## Konvensi

- Bahasa kode: Inggris. Dokumentasi: boleh Indonesia/Inggris.
- Target framework `net10.0-windows` (JANGAN ubah ke `net10.0` — LHM dan `WindowsIdentity` butuh Windows).
- `ImplicitUsings` + `Nullable` enabled.
- Gunakan **MVC Controller** (`[ApiController]`), bukan Minimal API, untuk endpoint.
- DTO sebagai `record` (`sealed record`), enum diserialisasi sebagai string.
- Jangan pakai komentar di kode kecuali diminta.

## Gotchas (sangat penting)

- **LibreHardwareMonitor TIDAK thread-safe.** Memanggil `hardware.Update()` dari request paralel akan mengkorup state internal (`Hardware.ActivateSensor` → `InvalidOperationException` → HTTP 500). Selalu akses via `HardwareMonitorService` yang sudah di-lock (`lock (_lock)` di dalam service), jangan langsung pegang instance `Computer`.
- **Wajib admin.** `AdminGuard` memeriksa `WindowsPrincipal.IsInRole(Administrator)` di awal `Program.cs`; non-admin → MessageBox + `Environment.Exit(1)`. Jangan lewati guard ini.
- **Sensor bisa bernilai `Infinity`/`NaN`** → System.Text.Json menolak menulisnya. Sudah disanitasi jadi `null` via `Finite()` di `HardwareMonitorService`. Kalau ada path serialisasi baru, pastikan nilai float dilewati `Finite()` atau set `JsonNumberHandling.AllowNamedFloatingPointLiterals`.
- `HardwareType` punya `GpuNvidia`, `GpuAmd`, **dan `GpuIntel`** — kategori `gpu` di controller harus mencakup ketiganya.
- Package `Microsoft.EntityFrameworkCore.Sqlite` ada di csproj tapi **belum terpakai** (dibiarkan sengaja). Jangan dihapus tanpa izin pemilik.
- `Computer` singleton didaftarkan via `AddSingleton`; DI container akan memanggil `Dispose()` (menutup LHM) saat shutdown. Tidak perlu handler manual.

## Cara menguji

1. Buka PowerShell sebagai Administrator.
2. `dotnet run` (default `http://localhost:8080`).
3. Verifikasi cepat:
   - `Invoke-WebRequest http://localhost:8080/`
   - `Invoke-WebRequest http://localhost:8080/api/hardware`
   - `Invoke-WebRequest http://localhost:8080/api/hardware/cpu`
4. Tes konkurrensi (mis. 5 request paralel) untuk memastikan lock tetap aman, lalu cek log tidak ada baris `fail:`.

Note: menjalankan/mematikan proses elevated dari shell non-admin butuh `Start-Process -Verb RunAs` (UAC prompt).
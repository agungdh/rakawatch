using LibreHardwareMonitor.Hardware;
using Rakawatch.Models;

namespace Rakawatch.Services;

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;
    private readonly object _lock = new();
    private IReadOnlyList<HardwareDto> _snapshot = Array.Empty<HardwareDto>();

    public HardwareMonitorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsStorageEnabled = true,
            IsNetworkEnabled = true,
            IsControllerEnabled = true,
            IsBatteryEnabled = true,
            IsPsuEnabled = true,
            IsPowerMonitorEnabled = true
        };
        _computer.Open();
    }

    public void Update()
    {
        lock (_lock)
            _snapshot = UpdateAll().Select(Map).ToList();
    }

    public IReadOnlyList<HardwareDto> GetSnapshot() => _snapshot;

    public IReadOnlyList<HardwareDto> GetByType(HardwareType type) =>
        _snapshot.Where(h => h.Type == type.ToString()).ToList();

    public HardwareDto? GetByName(HardwareType type, string name) =>
        _snapshot.FirstOrDefault(h =>
            h.Type == type.ToString() && h.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    private IEnumerable<IHardware> UpdateAll()
    {
        foreach (var hardware in _computer.Hardware)
        {
            Update(hardware);
            yield return hardware;
        }
    }

    private static void Update(IHardware hardware)
    {
        hardware.Update();

        foreach (var sub in hardware.SubHardware)
            Update(sub);
    }

    private static HardwareDto Map(IHardware hardware) => new(
        hardware.Identifier.ToString(),
        hardware.Name,
        hardware.HardwareType.ToString(),
        hardware.Sensors.Select(Map).ToList(),
        hardware.SubHardware.Select(Map).ToList());

    private static SensorDto Map(ISensor sensor) => new(
        sensor.Identifier.ToString(),
        sensor.Name,
        sensor.SensorType.ToString(),
        Finite(sensor.Value),
        MapUnit(sensor.SensorType),
        Finite(sensor.Min),
        Finite(sensor.Max),
        sensor.Index);

    private static string MapUnit(SensorType type) => type switch
    {
        SensorType.Voltage => "V",
        SensorType.Current => "A",
        SensorType.Power => "W",
        SensorType.Clock => "MHz",
        SensorType.Temperature => "°C",
        SensorType.Load => "%",
        SensorType.Frequency => "Hz",
        SensorType.Fan => "RPM",
        SensorType.Flow => "L/h",
        SensorType.Control => "%",
        SensorType.Level => "%",
        SensorType.Factor => "Ratio",
        SensorType.Data => "GB",
        SensorType.SmallData => "MB",
        SensorType.Throughput => "B/s",
        SensorType.TimeSpan => "s",
        SensorType.Timing => "s",
        SensorType.Energy => "Wh",
        SensorType.Noise => "dBA",
        SensorType.Conductivity => "µS/cm",
        SensorType.Humidity => "%",
        _ => string.Empty
    };

    private static float? Finite(float? value) =>
        value is { } v && float.IsFinite(v) ? v : null;

    public void Dispose() => _computer.Close();
}
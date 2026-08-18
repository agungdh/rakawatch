using LibreHardwareMonitor.Hardware;
using Rakawatch.Models;

namespace Rakawatch.Services;

public sealed class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;
    private readonly object _lock = new();

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

    public IReadOnlyList<HardwareDto> GetSnapshot()
    {
        lock (_lock)
            return UpdateAll().Select(Map).ToList();
    }

    public IReadOnlyList<HardwareDto> GetByType(HardwareType type)
    {
        lock (_lock)
            return UpdateAll()
                .Where(h => h.HardwareType == type)
                .Select(Map)
                .ToList();
    }

    public HardwareDto? GetByName(HardwareType type, string name)
    {
        lock (_lock)
            return UpdateAll()
                .FirstOrDefault(h => h.HardwareType == type && h.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                is { } hardware ? Map(hardware) : null;
    }

    private IEnumerable<IHardware> UpdateAll()
    {
        foreach (var hardware in _computer.Hardware)
            Update(hardware);

        return _computer.Hardware;
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
        Finite(sensor.Min),
        Finite(sensor.Max),
        sensor.Index);

    private static float? Finite(float? value) =>
        value is { } v && float.IsFinite(v) ? v : null;

    public void Dispose() => _computer.Close();
}
namespace Rakawatch.Models;

public sealed record SensorDto(
    string Id,
    string Name,
    string Type,
    float? Value,
    float? Min,
    float? Max,
    int Index);

public sealed record HardwareDto(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<SensorDto> Sensors,
    IReadOnlyList<HardwareDto> SubHardware);
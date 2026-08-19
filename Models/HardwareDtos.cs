namespace Rakawatch.Models;

public sealed record SensorDto(
    string Id,
    string Name,
    string Type,
    float? Value,
    string Unit,
    float? Min,
    float? Max,
    int Index);

public sealed record HardwareDto(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<SensorDto> Sensors,
    IReadOnlyList<HardwareDto> SubHardware);

public sealed record ErrorDto(string Error);

public sealed record StatusDto(
    string App,
    string Version,
    DateTimeOffset Timestamp,
    IReadOnlyList<string> Endpoints,
    IReadOnlyDictionary<string, int> Hardware);
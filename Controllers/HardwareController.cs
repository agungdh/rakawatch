using LibreHardwareMonitor.Hardware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rakawatch.Models;
using Rakawatch.Services;

namespace Rakawatch.Controllers;

[ApiController]
[Route("api/hardware")]
public sealed class HardwareController(HardwareMonitorService monitor) : ControllerBase
{
    private static readonly IReadOnlyDictionary<string, HardwareType[]> Categories =
        new Dictionary<string, HardwareType[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["cpu"] = [HardwareType.Cpu],
            ["gpu"] = [HardwareType.GpuNvidia, HardwareType.GpuAmd, HardwareType.GpuIntel],
            ["memory"] = [HardwareType.Memory],
            ["motherboard"] = [HardwareType.Motherboard],
            ["storage"] = [HardwareType.Storage],
            ["network"] = [HardwareType.Network],
            ["battery"] = [HardwareType.Battery],
            ["controller"] = [HardwareType.SuperIO, HardwareType.EmbeddedController],
            ["psu"] = [HardwareType.Psu],
            ["power"] = [HardwareType.PowerMonitor]
        };

    [HttpGet("/")]
    [ProducesResponseType<StatusDto>(StatusCodes.Status200OK)]
    public IActionResult GetStatus()
    {
        var snapshot = monitor.GetSnapshot();

        var status = new StatusDto(
            "Rakawatch",
            typeof(Program).Assembly.GetName().Version?.ToString() ?? "0.0.0",
            DateTimeOffset.UtcNow,
            new[] { "/", "/api/hardware", "/api/hardware/{type}", "/api/hardware/{type}/{name}" },
            snapshot.GroupBy(h => h.Type).ToDictionary(g => g.Key, g => g.Count()));

        return Ok(status);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<HardwareDto>>(StatusCodes.Status200OK)]
    public IActionResult GetAll() => Ok(monitor.GetSnapshot());

    [HttpGet("{type}")]
    [ProducesResponseType<IReadOnlyList<HardwareDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status400BadRequest)]
    public IActionResult GetByType(string type)
    {
        var types = ResolveType(type);
        if (types is null)
            return BadRequest(new ErrorDto($"Unknown hardware type '{type}'."));

        var result = types.SelectMany(t => monitor.GetByType(t)).ToList();
        return Ok(result);
    }

    [HttpGet("{type}/{name}")]
    [ProducesResponseType<HardwareDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorDto>(StatusCodes.Status404NotFound)]
    public IActionResult GetByName(string type, string name)
    {
        var types = ResolveType(type);
        if (types is null)
            return BadRequest(new ErrorDto($"Unknown hardware type '{type}'."));

        var hardware = types
            .Select(t => monitor.GetByName(t, name))
            .FirstOrDefault(h => h is not null);

        return hardware is null
            ? NotFound(new ErrorDto($"Hardware '{name}' of type '{type}' not found."))
            : Ok(hardware);
    }

    private static HardwareType[]? ResolveType(string type)
    {
        if (Categories.TryGetValue(type, out var mapped))
            return mapped;

        return Enum.TryParse<HardwareType>(type, ignoreCase: true, out var parsed)
            && !type.All(char.IsDigit)
            && Enum.IsDefined(parsed)
            ? [parsed]
            : null;
    }
}
using System.Text.Json.Serialization;

namespace AnalysisService.DTO;

public record FuelPressureMessage(
    [property: JsonPropertyName("fuel_pressure")]
    double FuelPressure);
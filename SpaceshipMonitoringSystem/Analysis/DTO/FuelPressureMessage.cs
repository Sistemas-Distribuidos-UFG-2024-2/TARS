using System.Text.Json.Serialization;

namespace Analysis.DTO;

public record FuelPressureMessage(
    [property: JsonPropertyName("fuel_pressure")]
    double FuelPressure);
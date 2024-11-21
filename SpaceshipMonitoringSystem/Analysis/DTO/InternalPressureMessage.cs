using System.Text.Json.Serialization;

namespace Analysis.DTO;

public record InternalPressureMessage(
    [property: JsonPropertyName("internal_pressure")]
    double InternalPressure);
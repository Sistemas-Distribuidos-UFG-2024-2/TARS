using System.Text.Json.Serialization;

namespace AnalysisService.DTO;

public record InternalPressureMessage(
    [property: JsonPropertyName("internal_pressure")]
    double InternalPressure);
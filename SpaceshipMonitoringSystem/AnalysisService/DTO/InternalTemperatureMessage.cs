using System.Text.Json.Serialization;

namespace AnalysisService.DTO;

public record InternalTemperatureMessage(
    [property: JsonPropertyName("internal_temperature")]
    double InternalTemperature);
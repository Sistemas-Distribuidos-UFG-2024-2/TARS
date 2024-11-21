using System.Text.Json.Serialization;

namespace AnalysisService.DTO;

public record ExternalTemperatureMessage(
    [property: JsonPropertyName("external_temperature")]
    double Externaltemperature);
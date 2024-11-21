using System.Text.Json.Serialization;

namespace Analysis.DTO;

public record ExternalTemperatureMessage(
    [property: JsonPropertyName("external_temperature")]
    double Externaltemperature);
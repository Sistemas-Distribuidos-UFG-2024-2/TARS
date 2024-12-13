using System.Text.Json.Serialization;

namespace Analysis.DTO;

public record InternalTemperatureMessage(
    [property: JsonPropertyName("internal_temperature")]
    double InternalTemperature,

    DateTime Timestamp
);
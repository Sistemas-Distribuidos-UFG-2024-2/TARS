using MongoDB.Bson.Serialization.Attributes;
using Analysis.Database;

namespace Analysis.Entities;

public class Sensor : BaseEntity
{

    [BsonElement("timestamp")]
    public required string Timestamp { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("value")]
    public required double Value { get; set; }
}

public class Acceleration :Sensor;
public class ExternalTemperature : Sensor;


using Houston.Database;
using MongoDB.Bson.Serialization.Attributes;

namespace Houston.Entities;

public class Sensor : BaseEntity
{

    [BsonElement("timestamp")]
    public required string Timestamp { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("value")]
    public required double Value { get; set; }
}

public class Acceleration : Sensor;

public class ExternalTemperature : Sensor;

public class FuelPressure : Sensor;

public class InternalPressure : Sensor;

public class InternalTemperature : Sensor;

public class Radiation : Sensor;

public class Gyroscope : BaseEntity
{
    [BsonElement("timestamp")]
    public required string Timestamp { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("X")]
    public required double X { get; set; }

    [BsonElement("Y")]
    public required double Y { get; set; }

    [BsonElement("Z")]
    public required double Z { get; set; }
}


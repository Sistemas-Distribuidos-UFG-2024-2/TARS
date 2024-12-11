using MongoDB.Bson.Serialization.Attributes;
using Analysis.Database;

namespace Analysis.Entities;

public class Sensor : BaseEntity
{

    [BsonElement("timestamp")]
    public required string Timestamp { get; set; }

    [BsonElement("name")]
    public required name Name { get; set; }

    [BsonElement("value")]
    public required double Value { get; set; }
}
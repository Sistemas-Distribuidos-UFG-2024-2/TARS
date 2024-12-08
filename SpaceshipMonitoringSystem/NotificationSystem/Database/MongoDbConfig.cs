namespace NotificationSystem.Database;

public class MongoDbConfig : IMongoDbConfig
{
    public MongoDbConfig(IConfiguration configuration)
    {
        var section = configuration.GetSection("MongoDB");
        ConnectionString = section.GetValue("ConnectionString", "mongodb://localhost:27017")!;
        DatabaseName = section.GetValue("Database", "sms_db")!;
    }
    public string ConnectionString { get; }
    public string DatabaseName { get; }
}
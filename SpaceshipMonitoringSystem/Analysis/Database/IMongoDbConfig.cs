namespace Analysis.Database;

public interface IMongoDbConfig
{
    string ConnectionString { get; }
    string DatabaseName { get; }
}
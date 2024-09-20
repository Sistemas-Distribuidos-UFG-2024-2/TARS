namespace Client.Services;

public interface IServerService
{
    Task<string?> GetServerResponse();
}
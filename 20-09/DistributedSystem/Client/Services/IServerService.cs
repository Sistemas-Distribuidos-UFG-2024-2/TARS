using Common;

namespace Client.Services;

public interface IServerService
{
    Task<SumResponse?> GetServerResponse(SumRequest request);
}
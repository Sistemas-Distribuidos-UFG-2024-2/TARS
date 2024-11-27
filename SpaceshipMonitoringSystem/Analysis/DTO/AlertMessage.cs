namespace Analysis.DTO;
using MassTransit;

[MessageUrn("alert-message")]
public record AlertMessage 
{
    public string Type { get; init; }
    public string Message { get; init; }
}
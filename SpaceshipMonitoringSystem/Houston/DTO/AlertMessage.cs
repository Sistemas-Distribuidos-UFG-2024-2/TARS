namespace Houston.DTO;
using MassTransit;

[MessageUrn("alert-message")]
public record AlertMessage 
{
    public string AlertType { get; init; }
    public string Description { get; init; }
}
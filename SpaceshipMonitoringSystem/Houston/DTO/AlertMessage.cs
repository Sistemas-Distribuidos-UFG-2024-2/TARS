namespace Commom.DTO;

public record AlertMessage 
{
    public string AlertType { get; init; }
    public string Description { get; init; }
}
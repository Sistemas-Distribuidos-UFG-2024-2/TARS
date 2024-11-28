namespace Houston.DTO;
using MassTransit;

[MessageUrn("houston-spaceship-message")]
public record SpaceshipMessage(string Text);
namespace SpaceshipInterface.DTO;
using MassTransit;

[MessageUrn("houston-spaceship-message")]
public record HoustonMessage(string Text);
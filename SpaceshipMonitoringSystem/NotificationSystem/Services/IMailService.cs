namespace NotificationSystem.Services;

public interface IMailService
{
    Task SendAlert(string type, string text);
}
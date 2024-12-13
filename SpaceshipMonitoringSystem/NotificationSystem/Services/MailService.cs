using System.Net;
using System.Net.Mail;

namespace NotificationSystem.Services;

public class MailService : IMailService
{
    private readonly SmtpClient _smtpClient;
    private readonly IPersonsService _personsService;
    private readonly ILogger<MailService> _logger;

    public MailService(ILogger<MailService> logger, IPersonsService personsService)
    {
        _logger = logger;
        _personsService = personsService;

        _smtpClient = new SmtpClient();
        _smtpClient.Host = "smtp.maileroo.com";
        _smtpClient.Port = 587;
        _smtpClient.UseDefaultCredentials = false;
        _smtpClient.Credentials =
            new NetworkCredential("tars.sms@d2dc82ee88f1f7fb.maileroo.org", "a564fab63b743977ea06b98f");
        _smtpClient.EnableSsl = true;
    }

    public async Task SendAlert(string type, string text)
    {
        try
        {
            var persons = await _personsService.GetAll();
            if (!persons.Any()) return;

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("tars.sms@d2dc82ee88f1f7fb.maileroo.org");

            foreach (var person in persons)
            {
                mailMessage.To.Add(person.Email);
            }

            mailMessage.Subject = "NEW ALERT!!!";
            mailMessage.Body = $"New alert thrown by {type} sensor!!\n{text}";

            _smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error sending alert email");
        }
    }
}
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ordering.Application.Common.Contracts.Notifications;
using Ordering.Application.Common.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Ordering.Infrastructure.Notifications;

public class EmailService : IEmailService
{
    private readonly EmailSettings emailSettings;
    private readonly ILogger<EmailService> logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        this.emailSettings = emailSettings.Value;
        this.logger = logger;
    }

    public async Task<bool> SendEmail(Email email)
    {
        if (string.IsNullOrWhiteSpace(emailSettings.ApiKey))
        {
            logger.LogInformation("Email sending is disabled (no SendGrid API key configured). Skipped '{Subject}'.", email.Subject);
            return false;
        }

        var client = new SendGridClient(emailSettings.ApiKey);

        var from = new EmailAddress
        {
            Email = emailSettings.FromAddress,
            Name = emailSettings.FromName
        };

        var sendGridMessage = MailHelper.CreateSingleEmail(from, new EmailAddress(email.To), email.Subject, email.Body, email.Body);
        var response = await client.SendEmailAsync(sendGridMessage);

        if (response.IsSuccessStatusCode)
        {
            logger.LogInformation("Email '{Subject}' sent.", email.Subject);
            return true;
        }

        logger.LogWarning("Email '{Subject}' failed with status {StatusCode}.", email.Subject, response.StatusCode);
        return false;
    }
}

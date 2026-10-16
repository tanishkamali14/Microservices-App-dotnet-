using Ordering.Application.Common.Contracts.Notifications;
using Ordering.Application.Common.Models;

namespace Ordering.Application.Tests.Fakes;

public class FakeEmailService : IEmailService
{
    public List<Email> Sent { get; } = new();

    public bool ThrowOnSend { get; set; }

    public Task<bool> SendEmail(Email email)
    {
        if (ThrowOnSend)
        {
            throw new InvalidOperationException("SMTP is down");
        }

        Sent.Add(email);
        return Task.FromResult(true);
    }
}

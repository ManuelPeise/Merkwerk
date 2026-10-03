namespace Logic.Notifications.Tests;

public sealed class SmtpMailServiceTests
{
    [Fact]
    public async Task SendAsync_ServerUnreachable_ThrowsMailDeliveryException()
    {
        // Port 1 on localhost: nothing listens there.
        var service = MailpitTests.CreateService("127.0.0.1", 1, timeoutSeconds: 2);
        var request = new MailMessageRequest(
            "anna@example.org", "Anna", MailTemplate.ConfirmEmail, "de", TemplateValues.For(MailTemplate.ConfirmEmail));

        var exception = await Assert.ThrowsAsync<MailDeliveryException>(() => service.SendAsync(request, CancellationToken.None));

        Assert.Equal(MailTemplate.ConfirmEmail, exception.Template);
        Assert.DoesNotContain("anna@example.org", exception.Message);
    }
}

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace IngBackend.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(
            new MailboxAddress(
                _configuration["Email:SenderName"],
                _configuration["Email:SenderAddress"]
            )
        );
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("html") { Text = message };

        using var client = new SmtpClient();
        await client.ConnectAsync(
            _configuration["Email:SmtpServer"],
            int.Parse(_configuration["Email:Port"]),
            SecureSocketOptions.StartTls
        );
        await client.AuthenticateAsync(
            _configuration["Secrets:EmailUsername"],
            _configuration["Secrets:EmailPassword"]
        );
        var result = await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}

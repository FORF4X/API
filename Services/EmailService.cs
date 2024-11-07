using System.Threading.Tasks;
using API.Services;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();
        smtp.Connect(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:Port"]), MailKit.Security.SecureSocketOptions.StartTls);
        smtp.Authenticate(_configuration["EmailSettings:UserName"], _configuration["EmailSettings:Password"]);
        await smtp.SendAsync(email);
        smtp.Disconnect(true);
    }
}
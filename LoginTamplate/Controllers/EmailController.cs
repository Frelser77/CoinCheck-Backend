using LoginTamplate.Data;
using LoginTamplate.Model.Dto.Email;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(EmailDto emailDto)
    {
        var from = new MailAddress("coincheck@outlook.it", "CoinCheck");
        var to = new MailAddress(emailDto.to);
        var fromPassword = _configuration["EmailSettings:Password"];
        var smtp = new SmtpClient
        {
            Host = "smtp-mail.outlook.com",
            Port = 587,
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(from.Address, fromPassword)
        };

        using (var message = new MailMessage(from, to)
        {
            Subject = emailDto.subject,
            Body = emailDto.body,
            IsBodyHtml = true,
        })
        {
            await smtp.SendMailAsync(message);
        }
    }
}

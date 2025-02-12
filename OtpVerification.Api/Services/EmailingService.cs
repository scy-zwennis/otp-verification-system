using System.Net;
using System.Net.Mail;
using FluentResults;
using Microsoft.Extensions.Options;
using OtpVerification.Api.Configuration;
using OtpVerification.Api.Services.Interfaces;

namespace OtpVerification.Api.Services;

public class EmailingService : IEmailingService
{
    private readonly SmtpSettings smtpSettings;

    public EmailingService(IOptions<SmtpSettings> smtpSettings)
    {
        this.smtpSettings = smtpSettings.Value;
    }

    public Result SendEmail(string email, string subject, string body)
    {
        var smtpClient = new SmtpClient(smtpSettings.Host)
        {
            Port = smtpSettings.Port,
            Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password),
            EnableSsl = false
        };

        var message = new MailMessage
        {
            From = new MailAddress(smtpSettings.From),
            Subject = subject,
            Body = body
        };

        message.To.Add(email);

        try
        {
            smtpClient.Send(message);
            return Result.Ok();
        }
        catch
        {
            return Result.Fail("Failed to send email");
        }
    }
}
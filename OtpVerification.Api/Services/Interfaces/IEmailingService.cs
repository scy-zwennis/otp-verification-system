using FluentResults;

namespace OtpVerification.Api.Services.Interfaces;

public interface IEmailingService
{
    Result SendEmail(string email, string subject, string body);
}
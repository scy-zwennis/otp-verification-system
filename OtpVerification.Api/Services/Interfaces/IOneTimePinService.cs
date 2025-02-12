using FluentResults;

namespace OtpVerification.Api.Services.Interfaces;

public interface IOneTimePinService
{
    Task<Result<string>> CreateOneTimePinAsync(string email);
    Task<Result> ValidateOneTimePinAsync(string email, string code);
}
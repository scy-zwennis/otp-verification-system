using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FluentResults;
using OtpVerification.Api.Data;
using OtpVerification.Api.Data.Entities;
using OtpVerification.Api.Services.Interfaces;
using OtpVerification.Api.Configuration;

namespace OtpVerification.Api.Services;

public class OneTimePinService : IOneTimePinService
{
    private readonly OtpDbContext context;
    private readonly OtpSettings otpSettings;

    public OneTimePinService(OtpDbContext context, IOptions<OtpSettings> otpSettings)
    {
        this.context = context;
        this.otpSettings = otpSettings.Value;
    }

    public async Task<Result<string>> CreateOneTimePinAsync(string email)
    {
        try
        {
            if (!IsValidEmail(email))
            {
                return Result.Fail("Provided email is not valid, please provide a valid email");
            }

            var user = await context.Users.Include(u => u.LastIssuedOtp).SingleOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                var code = OneTimePinUtils.GenerateCode();
                await CreateUserWithCodeAsync(email, code);
                return code;
            }

            if (CanReissueOtp(user.LastIssuedOtp))
            {
                await ReissueOtpAsync(user.LastIssuedOtp!);
                return user.LastIssuedOtp!.Code;
            }

            var recentOtps = await FetchUserOneTimePinsAsync(user.UserId, DateTime.UtcNow.AddDays(-1));
            if (HasReachedMaxHourlyRequest(recentOtps))
            {
                return Result.Fail("You have reached the max number of request, please come again later.");
            }

            {
                var code = OneTimePinUtils.GenerateCode(recentOtps.Select(otp => otp.Code));
                await CreateUserOneTimePinAsync(user, code);
                return Result.Ok(code);
            }
        }
        catch
        {
            return Result.Fail($"Failed to create OTP, please try again later.");
        }
    }

    public async Task<Result> ValidateOneTimePinAsync(string email, string code)
    {
        if (!IsValidEmail(email))
        {
            return Result.Fail("Provided email is not valid, please provide a valid email");
        }

        var user = await context.Users.Include(u => u.LastIssuedOtp).SingleOrDefaultAsync(u => u.Email == email);
        
        var lastIssuedOtp = user?.LastIssuedOtp;
        if (lastIssuedOtp is null || code != lastIssuedOtp.Code)
        {
            return Result.Fail("Provided email or OTP is incorrect");
        }

        if (lastIssuedOtp.IsUsed)
        {
            return Result.Fail("Provided OTP has been used");
        }

        if (lastIssuedOtp.IsExpired)
        {
            return Result.Fail("Provided OTP has expired");
        }

        lastIssuedOtp.Use();
        await context.SaveChangesAsync();
        return Result.Ok();
    }

    private async Task<OneTimePin> CreateUserOneTimePinAsync(User user, string code)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var otp = OneTimePin.Create(user.UserId, code, otpSettings.ExpiresInSeconds);
        context.OneTimePins.Add(otp);
        await context.SaveChangesAsync();

        user.LastIssuedOtp?.Expire();
        user.LastIssuedOtpId = otp.OneTimePinId;
        await context.SaveChangesAsync();

        transaction.Complete();
        return otp;
    }

    private async Task<User> CreateUserWithCodeAsync(string email, string code)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        
        var user = User.Create(email);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var otp = OneTimePin.Create(user.UserId, code, otpSettings.ExpiresInSeconds);
        context.OneTimePins.Add(otp);
        await context.SaveChangesAsync();

        user.LastIssuedOtp = otp;
        user.LastIssuedOtpId = otp.OneTimePinId;
        await context.SaveChangesAsync();

        transaction.Complete();
        return user;
    }

    private Task<List<OneTimePin>> FetchUserOneTimePinsAsync(int userId, DateTime createdAfter)
    {
        return context.OneTimePins
            .Where(otp => otp.UserId == userId && otp.CreatedAt > createdAfter)
            .ToListAsync();
    }

    private async Task ReissueOtpAsync(OneTimePin oneTimePin)
    {
        oneTimePin.RequestCount += 1;
        oneTimePin.SetExpiryDt(otpSettings.ExpiresInSeconds);
        await context.SaveChangesAsync();
    }

    private bool CanReissueOtp(OneTimePin? otp) {
        if (otp is null || otp.IsUsed || otp.IsExpired)
            return false;            

        if (otp.RequestCount >= otpSettings.AllowedReissueRequests)
            return false;

        var timespanSinceCreated = DateTime.UtcNow.Subtract(otp.CreatedAt);
        return timespanSinceCreated.Minutes < otpSettings.AllowedReissueMinutes;
    }

    private bool HasReachedMaxHourlyRequest(IEnumerable<OneTimePin> recentOtps)
    {
        var requestsInLastHour = recentOtps.Where(otp => otp.CreatedAt > DateTime.UtcNow.AddHours(-1)).Count();
        return requestsInLastHour >= otpSettings.RequestsPerHour;
    }

    private static bool IsValidEmail(string email)
    {
        return new EmailAddressAttribute().IsValid(email.Trim());
    }
}
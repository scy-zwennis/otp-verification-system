using System.ComponentModel.DataAnnotations;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using FluentResults;
using OtpVerification.Api.Data;
using OtpVerification.Api.Data.Entities;
using OtpVerification.Api.Services.Interfaces;

namespace OtpVerification.Api.Services;

public class OneTimePinService(OtpDbContext context) : IOneTimePinService
{
    private const int OTP_EXPIRES_IN_SECONDS = 30;
    private const int OTP_REISSUE_MINUTES = 5;
    private const int OTP_REISSUE_MAX = 3;
    private const int OTP_PER_HOUR_MAX = 100;

    public async Task<Result<string>> CreateOneTimePinAsync(string email)
    {
        try
        {
            if (!IsValidEmail(email))
            {
                return Result.Fail("Provided email is not valid, please enter a valid email");
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

    private async Task<OneTimePin> CreateUserOneTimePinAsync(User user, string code)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var otp = OneTimePin.Create(user.UserId, code, OTP_EXPIRES_IN_SECONDS);
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

        var otp = OneTimePin.Create(user.UserId, code, OTP_EXPIRES_IN_SECONDS);
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
        oneTimePin.SetExpiryDt(OTP_EXPIRES_IN_SECONDS);
        await context.SaveChangesAsync();
    }

    private bool CanReissueOtp(OneTimePin? otp) {
        if (otp is null || otp.HasExpired)
            return false;            

        if (otp.RequestCount >= OTP_REISSUE_MAX)
            return false;

        var timespanSinceCreated = DateTime.UtcNow.Subtract(otp.CreatedAt);
        return timespanSinceCreated.Minutes < OTP_REISSUE_MINUTES;
    }

    private bool HasReachedMaxHourlyRequest(IEnumerable<OneTimePin> recentOtps)
    {
        var requestsInLastHour = recentOtps.Where(otp => otp.CreatedAt > DateTime.UtcNow.AddHours(-1)).Count();
        return requestsInLastHour >= OTP_PER_HOUR_MAX;
    }

    private bool IsValidEmail(string email)
    {
        return new EmailAddressAttribute().IsValid(email.Trim());
    }
}
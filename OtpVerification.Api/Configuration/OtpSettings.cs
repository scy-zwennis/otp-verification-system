namespace OtpVerification.Api.Configuration;

public class OtpSettings
{
    public int ExpiresInSeconds { get; set; } = 30;
    public int AllowedReissueMinutes { get; set; } = 5;
    public int AllowedReissueRequests { get; set; } = 3;
    public int RequestsPerHour { get; set; } = 3;
}
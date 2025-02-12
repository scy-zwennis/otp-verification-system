namespace OtpVerification.Api.Configuration;

public class SmtpSettings
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 0;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string From { get; set; } = "";
}
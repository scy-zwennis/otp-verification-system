namespace OtpVerification.Api.Data.Entities;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace OtpVerification.Api.Models;

public record VerifyOtpModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Required]
    public string Code { get; set; } = "";
}
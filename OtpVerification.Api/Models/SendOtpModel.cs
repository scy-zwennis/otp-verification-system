using System.ComponentModel.DataAnnotations;

namespace OtpVerification.Api.Models;

public record SendOtpModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";
}
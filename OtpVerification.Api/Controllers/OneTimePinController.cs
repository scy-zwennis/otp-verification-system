using Microsoft.AspNetCore.Mvc;
using OtpVerification.Api.Models;
using OtpVerification.Api.Services.Interfaces;

namespace OtpVerification.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OneTimePinController(IOneTimePinService oneTimePinService, IEmailingService emailingService) : ControllerBase
{
    [HttpPost("Send")]
    public async Task<IActionResult> Send(SendOtpModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
            
        var result = await oneTimePinService.CreateOneTimePinAsync(model.Email);
        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        var emailResult = emailingService.SendEmail(model.Email, "One Time Pin", CreateOneTimePinMessage(result.Value));
        if (emailResult.IsFailed)
        {
            return BadRequest("Failed to send OTP, please try again later");
        }

        return Ok();
    }

    [HttpPut("Validate")]
    public async Task<IActionResult> Validate(VerifyOtpModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await oneTimePinService.ValidateOneTimePinAsync(model.Email, model.Code);
        return result.IsSuccess
            ? Ok()
            : BadRequest(result.Errors);
    }

    private string CreateOneTimePinMessage(string code)
    {
        return $"Your OTP is {code}";
    }
}
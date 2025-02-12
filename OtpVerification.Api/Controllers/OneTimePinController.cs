using Microsoft.AspNetCore.Mvc;
using OtpVerification.Api.Models;
using OtpVerification.Api.Services.Interfaces;

namespace OtpVerification.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OneTimePinController(IOneTimePinService oneTimePinService) : ControllerBase
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

        return Ok();
    }
}
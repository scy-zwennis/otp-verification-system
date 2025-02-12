using Microsoft.AspNetCore.Mvc;

namespace OtpVerification.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class OneTimePinController : ControllerBase
{
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }
}
using System.Net.Mime;
using Lib;
using Microsoft.AspNetCore.Mvc;

namespace WebPushNotificationEndPointServer.Controllers;

[ApiController]
[Route("api/vapid")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
public class VapidGeneratorController : ControllerBase
{
    public VapidGeneratorController()
    {
    }

    [HttpGet("")]
    public IActionResult GenerateVapidJwtToken()
    {
        var expiration = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();

        var (publicKey, privateKey) = VapidKeyGenerator.GenerateVapidKeys();

        return Ok(new {publicKey, privateKey, expiration});
    }

    [HttpGet("jwt")]
    public IActionResult GenerateVapidJwtToken(string publicKey, string privateKey)
    {
        var expiration = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();

        var jwtToken = VapidAuthorization.GenerateJwtToken("https://localhost:7275/", "https://localhost:7275/", privateKey);

        return Ok(new {jwtToken, publicKey, privateKey, expiration});
    }
}
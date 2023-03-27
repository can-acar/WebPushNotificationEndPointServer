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

    private string GenerateUniqueId()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        string timestampBase36 = timestamp.ToString("X").ToLowerInvariant();
        string randomString = Guid.NewGuid().ToString("N").Substring(0, 16);

        return timestampBase36 + randomString;
    }

    [HttpGet("")]
    public IActionResult GenerateVapidJwtToken()
    {
        var expiration = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();

        var vapidKeys = VapidKeyGenerator.GenerateVapidKeys();

        return Ok(new
        {
            UserId = GenerateUniqueId(),
            vapidKeys.PublicKey,
            vapidKeys.PrivateKey,
        });
    }

    [HttpGet("jwt")]
    public IActionResult GenerateVapidJwtToken(string userId, string publicKey, string privateKey)
    {
        var expiration = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds();

        var jwtToken = VapidAuthorization.GenerateJwtToken("https://localhost:7275/", "https://localhost:7275/", privateKey);

        return Ok(new {jwtToken, publicKey, privateKey, userId, expiration});
    }
}
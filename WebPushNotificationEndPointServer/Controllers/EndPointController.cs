using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using EndpointEntities.Models;
using Lib;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebPushNotificationEndPointServer.Controllers;

[ApiController]
[Route("api/service/notification")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
public class EndPointController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EndPointController> _logger;
    private readonly IMediator _mediator;
    private readonly string _vapidSubject;
    private readonly string _vapidPublicKey;
    private readonly string _vapidPrivateKey;

    public EndPointController(IConfiguration configuration, ILogger<EndPointController> logger, IMediator mediator)
    {
        _configuration = configuration;
        _logger = logger;
        _mediator = mediator;

        _vapidSubject = _configuration["VapidSubject"];
        _vapidPublicKey = _configuration["VapidPublicKey"];
        _vapidPrivateKey = _configuration["VapidPrivateKey"];
    }


 


    [HttpPost("send-closing-notification")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> SendClosingNotificationToAllUsers([FromBody] string message)
    {
        await _mediator.Send(new SendNotificationCommand {Message = message, Title = "Closing Notification"});

        return Ok();
    }
}
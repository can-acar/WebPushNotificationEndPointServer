using System.Net.Mime;
using Lib;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebPushNotificationEndPointServer.Controllers;

[ApiController]
[Route("api/subscribe")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
public class SubscribeController : ControllerBase
{
    private readonly ILogger<SubscribeController> _logger;
    private readonly IMediator _mediator;

    public SubscribeController(ILogger<SubscribeController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Subscribe([FromBody] Subscription subscription)
    {
        try
        {
            await _mediator.Send(new SubscribeRequest {Subscription = subscription});

            return Ok();
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception.Message, exception);

            return BadRequest(new {status = false, exception.Message});
        }
    }


    [HttpDelete("{GUID:id}")]
    public async Task<IActionResult> Unsubscribe(Guid id)
    {
        await _mediator.Send(new UnsubscribeRequest {Id = id});
        return Ok();
    }
}
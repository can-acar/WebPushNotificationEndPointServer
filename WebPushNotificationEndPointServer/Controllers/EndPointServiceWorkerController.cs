using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebPushNotificationEndPointServer.Controllers;

[ApiController]
[Route("api/service")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces(MediaTypeNames.Application.Json)]
public class EndPointServiceWorkerController : ControllerBase
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<EndPointController> _logger;
    private readonly IMediator _mediator;

    public EndPointServiceWorkerController(IHostEnvironment environment, ILogger<EndPointController> logger, IMediator mediator)
    {
        _environment = environment;
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet("service-worker.js")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public IActionResult ServiceWorker()
    {
        var path = Path.Combine(_environment.ContentRootPath, "scripts", "service-worker.js");

        return PhysicalFile(path, "application/javascript");
    }
}
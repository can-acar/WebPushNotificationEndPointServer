using MediatR;

namespace Lib;

public class SendNotificationCommand : IRequest
{
    public string Title { get; set; }

    public string Message { get; set; }
    // public string Body { get; set; }
    // public string Icon { get; set; }
    // public string Url { get; set; }
    // public string Tag { get; set; }
    // public string Vibrate { get; set; }
    // public string Data { get; set; }
    // public string Actions { get; set; }
    // public string RequireInteraction { get; set; }
    // public string Silent { get; set; }
    // public string Timestamp { get; set; }
    // public string Badge { get; set; }
}
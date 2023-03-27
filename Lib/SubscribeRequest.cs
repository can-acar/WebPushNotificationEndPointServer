using MediatR;

namespace Lib;

public class SubscribeRequest : IRequest
{
    public Subscription Subscription { get; set; }
}
using MediatR;

namespace Lib;

public class UnsubscribeRequest : IRequest
{
    public Guid Id { get; set; }
}
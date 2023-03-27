using EndpointEntities;
using EndpointEntities.Models;
using Lib;
using MediatR;

namespace WebPushNotificationEndPointServer.Handlers;

public class SubscribeHandler : IRequestHandler<SubscribeRequest>
{
    private readonly ILogger<SubscribeHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly PushSubscriptionContext _context;

    public SubscribeHandler(ILogger<SubscribeHandler> logger, IConfiguration configuration, PushSubscriptionContext context)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
    }


    public async Task Handle(SubscribeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var current = DateTime.Now;

            var subscriptionEntity = new PushSubscription
            {
                Endpoint = request.Subscription.Endpoint,
                P256dh = request.Subscription.P256dh,
                Auth = request.Subscription.Auth,
                UserId = request.Subscription.UserId,
                Created = current,
                IsActive = true
            };

            await _context.PushSubscription.AddAsync(subscriptionEntity, cancellationToken);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}
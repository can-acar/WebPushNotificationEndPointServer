using EndpointEntities;
using Lib;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebPushNotificationEndPointServer.Services;

namespace WebPushNotificationEndPointServer.Handlers;

public class PushMessageHandler : IRequestHandler<SendNotificationCommand>
{
    private readonly ILogger<PushMessageHandler> _logger;

    private readonly PushSubscriptionContext _context;
    private readonly IPushNotificationService _pushNotificationService;

    // private string? _vapidSubject = "mailto:your@email.com";
    // private string? _vapidPublicKey = "YOUR_PUBLIC_KEY";
    // private string? _vapidPrivateKey = "YOUR_PRIVATE_KEY";

    public PushMessageHandler(ILogger<PushMessageHandler> logger,
        IConfiguration configuration,
        PushSubscriptionContext context,
        IPushNotificationService pushNotificationService)
    {
        _logger = logger;
        _context = context;
        _pushNotificationService = pushNotificationService;
    }


    public async Task Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await _context.PushSubscription.ToListAsync(cancellationToken);

            foreach (var subscriptionEntity in subscriptions)
            {
                var subscription = new Subscription
                {
                    Endpoint = subscriptionEntity.Endpoint,
                    Keys = new SubscriptionKeys
                    {
                        P256DH = subscriptionEntity.P256dh,
                        Auth = subscriptionEntity.Auth
                    }
                };

                await _pushNotificationService.SendNotification(subscription, "Your session is closing!");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending notification");
        }
    }
}
using System;
using System.Collections.Generic;

namespace EndpointEntities.Models;

public class PushSubscription
{
    public Guid Id { get; set; }

    public string Endpoint { get; set; } = null!;


    public string P256dh { get; set; } = null!;

    public string Auth { get; set; } = null!;

    public bool? IsActive { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
    public string UserId { get; set; }
}
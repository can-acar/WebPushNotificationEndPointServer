using Newtonsoft.Json;

namespace Lib;

public class Subscription
{
    public string UserId { get; set; }
    public string Endpoint { get; set; }
    public string P256dh { get; set; }
    public string Auth { get; set; }
}

public class Unsubscribe
{
    public Guid Id { get; set; }
}
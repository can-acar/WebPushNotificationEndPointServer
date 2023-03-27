using Newtonsoft.Json;

namespace Lib;

public class SubscriptionKeys
{
    [JsonProperty("p256dh")]
    public string P256DH { get; set; }

    [JsonProperty("auth")]
    public string Auth { get; set; }
}
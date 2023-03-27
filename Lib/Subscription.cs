using Newtonsoft.Json;

namespace Lib;

public class Subscription
{
    [JsonProperty("endpoint")] public string Endpoint { get; set; }

    [JsonProperty("keys")] public SubscriptionKeys Keys { get; set; }
}
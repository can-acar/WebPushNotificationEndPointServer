using System.Net.Http.Headers;
using System.Text;
using EndpointEntities;
using Lib;

namespace WebPushNotificationEndPointServer.Services;

public class VapidDetails
{
    public VapidDetails(string publicKey, string privateKey, string subject)
    {
        PublicKey = publicKey;
        PrivateKey = privateKey;
        Subject = subject;
    }


    public string PublicKey;
    public string PrivateKey;
    public string Subject;
}

public class PushNotificationService : IPushNotificationService
{
    private readonly HttpClient _httpClient;
    private readonly PushSubscriptionContext _context;
    private readonly IConfiguration _configuration;


    public PushNotificationService(IConfiguration configuration,
        HttpClient httpClient,
        PushSubscriptionContext context)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _context = context;
    }


    public async Task SendNotification(Subscription subscription, string payload)
    {
        var vapidDetails = new VapidDetails(
            _configuration["VapidSubject"],
            _configuration["VapidPublicKey"],
            _configuration["VapidPrivateKey"]
        );


        try
        {
            var audience = new Uri(subscription.Endpoint).Authority;

            var jwt = VapidAuthorization.GenerateJwtToken(audience, vapidDetails.Subject, vapidDetails.PrivateKey);

            var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("vapid", $"t={jwt};k={vapidDetails.PublicKey}");
            request.Headers.TryAddWithoutValidation("TTL", "180");
            request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error sending push notification: {response.StatusCode}");
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}

public interface IPushNotificationService
{
    Task SendNotification(Subscription subscription, string yourSessionİsClosing);
}
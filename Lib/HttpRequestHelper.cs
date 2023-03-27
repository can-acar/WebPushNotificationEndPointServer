namespace Lib;

public static class HttpRequestHelper
{
    public static async Task<HttpResponseMessage> SendAsync(HttpMethod method,
        string requestUri,
        string content,
        string contentType,
        string authorization)
    {
        using var httpClient = new HttpClient();
        using var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorization);

        if (!string.IsNullOrEmpty(content))
        {
            request.Content = new StringContent(content, System.Text.Encoding.UTF8, contentType);
        }

        return await httpClient.SendAsync(request);
    }
}
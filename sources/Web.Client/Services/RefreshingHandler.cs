using System.Net;

namespace Web.Client.Services;

/// <summary>
/// LP-006 spike: when an API call returns 401, refreshes the JWT cookie once via /api/v1/auth/refresh
/// and repeats the request. Only for requests without a body (GET) in the spike.
/// </summary>
public sealed class RefreshingHandler : DelegatingHandler
{
    private const string RefreshPath = "/api/v1/auth/refresh";

    public int RefreshCount { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.Unauthorized
            || request.RequestUri is null
            || request.RequestUri.AbsolutePath.StartsWith(RefreshPath, StringComparison.Ordinal)
            || request.Content is not null)
        {
            return response;
        }

        response.Dispose();

        using (var refreshRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(request.RequestUri, RefreshPath)))
        using (var refreshResponse = await base.SendAsync(refreshRequest, cancellationToken))
        {
            if (!refreshResponse.IsSuccessStatusCode)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = request };
            }
        }

        RefreshCount++;
        var retry = new HttpRequestMessage(request.Method, request.RequestUri);
        return await base.SendAsync(retry, cancellationToken);
    }
}

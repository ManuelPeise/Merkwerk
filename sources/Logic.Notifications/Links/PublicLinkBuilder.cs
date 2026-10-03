using Microsoft.Extensions.Options;

namespace Logic.Notifications.Links;

internal sealed class PublicLinkBuilder(IOptions<PublicUrlOptions> options) : IPublicLinkBuilder
{
    public string Build(string path, params (string Name, string Value)[] query)
    {
        var baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        var link = baseUrl + "/" + path.TrimStart('/');

        if (query.Length == 0)
        {
            return link;
        }

        var parameters = query.Select(p => Uri.EscapeDataString(p.Name) + "=" + Uri.EscapeDataString(p.Value));
        return link + "?" + string.Join("&", parameters);
    }
}

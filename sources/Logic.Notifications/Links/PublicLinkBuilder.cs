using Logic.Shared.Interfaces;
using Microsoft.Extensions.Options;

namespace Logic.Notifications.Links;

internal sealed class PublicLinkBuilder : IPublicLinkBuilder
{
    private readonly IOptions<PublicUrlOptions> _options;

    public PublicLinkBuilder(IOptions<PublicUrlOptions> options)
    {
        _options = options;
    }

    public string Build(string path, params (string Name, string Value)[] query)
    {
        var baseUrl = _options.Value.PublicBaseUrl.TrimEnd('/');
        var link = baseUrl + "/" + path.TrimStart('/');

        if (query.Length == 0)
        {
            return link;
        }

        var parameters = query.Select(p => Uri.EscapeDataString(p.Name) + "=" + Uri.EscapeDataString(p.Value));
        return link + "?" + string.Join("&", parameters);
    }
}

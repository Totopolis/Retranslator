using Application.Abstractions;
using Microsoft.Extensions.Options;
using System.Text;
using System.Xml;

namespace Persistence.Webhook;

internal class WebhookSender : IWebhookSender
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Uri _uri;

    public WebhookSender(
        IOptions<WebhookSettings> settings,
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
        _uri = new Uri(settings.Value.DestinationUri);
    }

    public async Task PostXml(XmlDocument document, CancellationToken ct = default)
    {
        string content = document.InnerXml;
        using var httpClient = _httpClientFactory
            .CreateClient(PersistenceConstants.WebhookHttpCLientName);

        var httpContent = new StringContent(content, Encoding.UTF8, "text/xml");
        await httpClient.PostAsync(_uri, httpContent);
    }
}

using System.Xml;

namespace Application.Abstractions;

public interface IWebhookSender
{
    Task PostXml(XmlDocument document, CancellationToken ct = default!);
}

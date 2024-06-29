using Application.Abstractions;
using System.Xml;

namespace Server.IntegrationTests;

public class AutoresetWebhookSender : IWebhookSender
{
    private readonly CancellationTokenSource _cts;

    public AutoresetWebhookSender(CancellationTokenSource cts)
    {
        _cts = cts;
    }

    public Task PostXml(XmlDocument document, CancellationToken ct = default)
    {
        _cts.Cancel();
        return Task.CompletedTask;
    }
}

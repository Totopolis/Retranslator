using System.ComponentModel.DataAnnotations;

namespace Persistence.Webhook;

public class WebhookSettings
{
    public const string SectionName = "Webhook";

    [Required]
    public string DestinationUri { get; set; } = default!;
}

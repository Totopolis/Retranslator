namespace Persistence.MessageBroker;

public class RabbitSettings
{
    public const string SectionName = "Rabbit";

    public string Host { get; set; } = default!;

    public string UserName { get; set; } = default!;

    public string Password { get; set; } = default!;
}

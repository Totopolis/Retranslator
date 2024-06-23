using System.ComponentModel.DataAnnotations;

namespace Persistence.Repositories;

public class PostgreSettings
{
    public const string SectionName = "Postgre";

    [Required]
    public string ConnectionString { get; set; } = default!;
}

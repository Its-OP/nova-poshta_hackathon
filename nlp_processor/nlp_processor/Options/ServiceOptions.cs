using System.ComponentModel.DataAnnotations;

namespace nlp_processor.Options;

public class ServiceOptions
{
    public const string PropertyName = "Service";

    /// <summary>
    /// Configuration Key Vault URI
    /// </summary>
    [Url]
    public string? KeyVault { get; set; }

    /// <summary>
    /// Local directory in which to load semantic skills.
    /// </summary>
    public string? SemanticSkillsDirectory { get; set; }
}

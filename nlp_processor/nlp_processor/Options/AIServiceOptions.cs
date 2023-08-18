using System.ComponentModel.DataAnnotations;

namespace nlp_processor.Options;

/// <summary>
/// Configuration options for AI services, such as Azure OpenAI and OpenAI.
/// </summary>
public sealed class AIServiceOptions
{
    public const string PropertyName = "AIService";

    /// <summary>
    /// AI models to use.
    /// </summary>
    public class ModelTypes
    {
        /// <summary>
        /// Azure OpenAI deployment name or OpenAI model name to use for completions.
        /// </summary>
        public string Completion { get; set; } = string.Empty;

        /// <summary>
        /// Azure OpenAI deployment name or OpenAI model name to use for embeddings.
        /// </summary>
        public string Embedding { get; set; } = string.Empty;

        /// <summary>
        /// Azure OpenAI deployment name or OpenAI model name to use for planner.
        /// </summary>
        public string Planner { get; set; } = string.Empty;
    }

    /// <summary>
    /// Models/deployment names to use.
    /// </summary>
    [Required]
    public ModelTypes Models { get; set; } = new ModelTypes();

    /// <summary>
    /// (Azure OpenAI only) Azure OpenAI endpoint.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Key to access the AI service.
    /// </summary>
    public string Key { get; set; } = string.Empty;
}

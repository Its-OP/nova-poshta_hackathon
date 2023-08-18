using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.SemanticKernel;
using nlp_processor.Options;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using System.Net.Http;

namespace nlp_processor.Extensions;

public static class SKExtension
{
    public delegate Task RegisterSkillsWithKernel(IServiceProvider sp, IKernel kernel);

    internal static IServiceCollection AddSemanticKernelServices(this IServiceCollection services, string modelKey)
    {
        // Semantic Kernel
        services.AddScoped<IKernel>(sp =>
        {
            IKernel kernel = Kernel.Builder
                .WithLogger(sp.GetRequiredService<ILogger<IKernel>>())
                .WithMemory(sp.GetRequiredService<ISemanticTextMemory>())
                .WithOpenAIChatCompletionService("gpt-3.5-turbo", modelKey, alsoAsTextCompletion: false, setAsDefault: true)
                .WithOpenAITextEmbeddingGenerationService("text-embedding-ada-002", modelKey, setAsDefault: true)
                .Build();

            sp.GetRequiredService<RegisterSkillsWithKernel>()(sp, kernel);
            return kernel;
        });

        // Semantic memory
        services.AddSemanticTextMemory(modelKey);

        // Register skills
        services.AddScoped<RegisterSkillsWithKernel>(sp => RegisterSkillsAsync);

        return services;
    }
    private static Task RegisterSkillsAsync(IServiceProvider sp, IKernel kernel)
    {
        // Copilot chat skills
        //kernel.RegisterChatSkill(sp);

        // Time skill
        kernel.ImportSkill(new TimeSkill(), nameof(TimeSkill));

        // Semantic skills
        ServiceOptions options = sp.GetRequiredService<IOptions<ServiceOptions>>().Value;
        if (!string.IsNullOrWhiteSpace(options.SemanticSkillsDirectory))
        {
            foreach (string subDir in Directory.GetDirectories(options.SemanticSkillsDirectory))
            {
                try
                {
                    kernel.ImportSemanticSkillFromDirectory(options.SemanticSkillsDirectory, Path.GetFileName(subDir)!);
                }
                catch (TemplateException e)
                {
                    kernel.Logger.LogError("Could not load skill from {Directory}: {Message}", subDir, e.Message);
                }
            }
        }

        return Task.CompletedTask;
    }

    private static void AddSemanticTextMemory(this IServiceCollection services, string apiKey)
    {
        services.AddSingleton<IMemoryStore, VolatileMemoryStore>();
        services.AddScoped<ISemanticTextMemory>(sp => new SemanticTextMemory(
            sp.GetRequiredService<IMemoryStore>(),
            new OpenAITextEmbeddingGeneration("text-embedding-ada-002", apiKey)));
    }
}

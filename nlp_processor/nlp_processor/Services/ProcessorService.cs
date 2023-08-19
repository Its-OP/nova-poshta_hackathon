using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using nlp_processor.Services.Utils;

namespace nlp_processor.Services;

public class ProcessorService : IProcessorService
{
    private readonly IKernel _kernel;
    private readonly IDictionary<string, ISKFunction> _orchestrationPlugin;
    private readonly IDictionary<string, ISKFunction> _counselingPlugin;

    public ProcessorService(IKernel kernel)
    {
        _kernel = kernel;
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        _orchestrationPlugin = _kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");
        _counselingPlugin = _kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "ConsultationPlugin");
    }

    public async Task<string> Process(string input)
    {
        var intention = await GetClassification(input);

    }

    private async Task<string> GetClassification(string input)
    {
        var context = _kernel.CreateNewContext();
        context.Variables["input"] = input;

        var result = await _orchestrationPlugin[nameof(GetClassification)].InvokeAsync(context);

        return result.Result;
    }

    private async Task<string> GetTosCounseling(string input)
    {
        await _kernel.SaveDocumentToMemory(Utils.Utils.GetTextFromPdf("Terms_of_Service.pdf"));

        var context = _kernel.CreateNewContext();

        var list = new List<MemoryQueryResult?>();

        await foreach (var item in _kernel.Memory.SearchAsync("collection", input, 5))
        {
            list.Add(item);
        }

        if (!list.Any())
        {
            return "Failed to find any relevant information";
        }

        context.Variables["context"] = list.First()!.Metadata.Text;
        context.Variables["input"] = input;

        var result = await _counselingPlugin[nameof(GetTosCounseling)].InvokeAsync(context);

        return result.Result;
    }
}

public interface IProcessorService
{
    Task<string> Process(string input);
}

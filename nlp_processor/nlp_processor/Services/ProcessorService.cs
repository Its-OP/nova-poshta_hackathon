using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Skills.Core;

namespace nlp_processor.Services;

public class ProcessorService : IProcessorService
{
    private readonly IKernel _kernel;
    private readonly IDictionary<string, ISKFunction> _orchestrationPlugin;
    private readonly IDictionary<string, ISKFunction> _counselingPlugin;
    private readonly IMemoryCache _memoryCache;

    public ProcessorService(IKernel kernel, IMemoryCache memoryCache)
    {
        _kernel = kernel;
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        _orchestrationPlugin = _kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");
        _counselingPlugin = _kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "ConsultationPlugin");
        _memoryCache = memoryCache;
    }

    public async Task<string> Process(string input)
    {
        var intention = await GetClassification(input);

        var response = intention switch
        {
            $"{nameof(GetCounselingOnCompanyProcessesAndServices)}" => await GetCounselingOnCompanyProcessesAndServices(input),
            $"{nameof(GetInformationAboutConcreteShipment)}" => await GetInformationAboutConcreteShipment(input),
            _ => "Failed to understand the prompt"
        };

        return response;
    }

    private async Task<string> GetClassification(string input)
    {
        var context = _kernel.CreateNewContext();
        context.Variables["input"] = input;
        context.Variables["options"] = $"{nameof(GetCounselingOnCompanyProcessesAndServices)}, {nameof(GetInformationAboutConcreteShipment)}, CalculateDeliveryPrice";

        var result = await _orchestrationPlugin[nameof(GetClassification)].InvokeAsync(context);

        return result.Result;
    }

    private async Task<string> GetCounselingOnCompanyProcessesAndServices(string input)
    {
        var context = _kernel.CreateNewContext();
        var memorySkill = new TextMemorySkill(_kernel.Memory);
        _kernel.ImportSkill(memorySkill);

        context.Variables[TextMemorySkill.CollectionParam] = "collection";
        context.Variables[TextMemorySkill.LimitParam] = "3";
        context.Variables[TextMemorySkill.RelevanceParam] = "0.8";

        context.Variables["input"] = input;

        var result = await _counselingPlugin[nameof(GetCounselingOnCompanyProcessesAndServices)].InvokeAsync(context);

        return result.Result;
    }

    private async Task<string> GetInformationAboutConcreteShipment(string input)
    {
        return "test";
    }
}

public interface IProcessorService
{
    Task<string> Process(string input);
}

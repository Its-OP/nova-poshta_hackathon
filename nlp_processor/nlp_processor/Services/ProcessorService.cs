using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Skills.Core;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using nlp_processor.Services.Utils;
using System.Text.RegularExpressions;
using GetInvoice;
using System.Text.Json;

namespace nlp_processor.Services;

public class ProcessorService : IProcessorService
{
    private readonly IKernel _kernel;
    private readonly IDictionary<string, ISKFunction> _orchestrationPlugin;
    private readonly IDictionary<string, ISKFunction> _counselingPlugin;
    private readonly IMemoryCache _memoryCache;

    private string? _shipmentInfo = null;

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
        var history = string.Empty;
        if (_memoryCache.TryGetValue("history", out var value))
        {
            history = (string)value!;
        }

        var intention = await GetClassification(input, history);

        var response = intention switch
        {
            $"{nameof(GetCounselingOnCompanyProcessesAndServices)}" => await GetCounselingOnCompanyProcessesAndServices(input, history),
            $"{nameof(GetInformationAboutConcreteShipment)}" => await GetInformationAboutConcreteShipment(input, history),
            _ => "Failed to understand the prompt"
        };

        return response;
    }

    private async Task<string> GetClassification(string input, string history)
    {
        var context = _kernel.CreateNewContext();
        context.Variables["input"] = input;
        context.Variables["history"] = history;
        context.Variables["options"] = $"{nameof(GetCounselingOnCompanyProcessesAndServices)}, {nameof(GetInformationAboutConcreteShipment)}, CalculateDeliveryPrice";
        context.Variables["examples"] = GetExamples();

        var result = await _orchestrationPlugin[nameof(GetClassification)].InvokeAsync(context);

        return result.Result;
    }

    private async Task<string> GetCounselingOnCompanyProcessesAndServices(string input, string history)
    {
        var context = _kernel.CreateNewContext();
        var memorySkill = new TextMemorySkill(_kernel.Memory);
        _kernel.ImportSkill(memorySkill);

        context.Variables[TextMemorySkill.CollectionParam] = "collection";
        context.Variables[TextMemorySkill.LimitParam] = "3";
        context.Variables[TextMemorySkill.RelevanceParam] = "0.8";

        context.Variables["input"] = input;
        context.Variables["history"] = history;

        var result = await _counselingPlugin[nameof(GetCounselingOnCompanyProcessesAndServices)].InvokeAsync(context);

        return result.Result;
    }

    private async Task<string> TryGetShipmentNumberFromInput(string input)
    {
        if (_shipmentInfo != null)
            return _shipmentInfo;

        string shipmentPattern = @"\d{14}";
        var mathes = Regex.Matches(input, shipmentPattern).ToList();

        if (mathes.Count > 0)
        {
            try
            {
                _shipmentInfo = JsonSerializer.Serialize(await InvoiceHandler.RequestInvoiceAsync(mathes[0].ToString()));
                return _shipmentInfo;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        else
        {
            return "Please, enter shipment number (14 digits).";
        }
    }

    private async Task<string> GetInformationAboutConcreteShipment(string input, string history)
    {
        var context = _kernel.CreateNewContext();

        _kernel.ImportSkill(new TimeSkill(), "time");

        context.Variables["history"] = history;
        context.Variables["input"] = input;

        context.Variables["shipmentInfo"] = await TryGetShipmentNumberFromInput(input);

        var result = await _counselingPlugin[nameof(GetInformationAboutConcreteShipment)].InvokeAsync(context);
        return result.Result;
    }

    private static string GetExamples()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"INPUT - 'Який документ мені потрібен щоб отримати посилку ?', OUTPUT - {nameof(GetCounselingOnCompanyProcessesAndServices)}");
        sb.AppendLine($"INPUT - 'Чи можна перевозити їжу в посилках ?', OUTPUT - {nameof(GetCounselingOnCompanyProcessesAndServices)}");
        sb.AppendLine($"INPUT - 'Як мені дізнатись вартість посилки ?', OUTPUT - {nameof(GetCounselingOnCompanyProcessesAndServices)}");

        sb.AppendLine($"INPUT - 'Коли приїде моя посилка ?', OUTPUT - {nameof(GetInformationAboutConcreteShipment)}");
        sb.AppendLine($"INPUT - 'Який статус має моя посилка ? Її номер - 21907687690432 ?', OUTPUT - {nameof(GetInformationAboutConcreteShipment)}");
        sb.AppendLine($"INPUT - 'З якого міста їде моя посилка ?', OUTPUT - {nameof(GetInformationAboutConcreteShipment)}");

        return sb.ToString();
    }
}

public interface IProcessorService
{
    Task<string> Process(string input);
}

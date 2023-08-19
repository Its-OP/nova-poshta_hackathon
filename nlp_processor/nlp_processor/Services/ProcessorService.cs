using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.SkillDefinition;
using nlp_processor.Services.Utils;
using System.Text.RegularExpressions;
using GetInvoice;

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

        var result = await _counselingPlugin[nameof(GetCounselingOnCompanyProcessesAndServices)].InvokeAsync(context);

        return result.Result;
    }

    private async Task<string> GetInformationAboutConcreteShipment(string input)
    {
        string shipmentPattern = @"\d{14}";
        string phoneNumberPattern = @"380\d{9)}";

        var mathes = Regex.Matches(input, shipmentPattern).ToList();

        if (mathes.Count > 0)
        {
            var invoiceDTO = await InvoiceHandler.RequestInvoiceAsync(mathes[0].ToString());

            if (invoiceDTO.Success)
            {
                return invoiceDTO.Data[0].ToString();
            }
            else
            {
                return String.Join("\n", invoiceDTO.Errors);
            }
        }
        else
        {
            return "Будь ласка, введіть номер накладної (14 цифр).";
        }
    }
}

public interface IProcessorService
{
    Task<string> Process(string input);
}

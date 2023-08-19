using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;
using System.Text;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;
using System.Text.RegularExpressions;

namespace nlp_processor.Services;

public class DocumentsService : IDocumentsService
{
    private readonly IKernel _kernel;
    private readonly IDictionary<string, ISKFunction> _documentsPlugin;

    public DocumentsService(IKernel kernel)
    {
        _kernel = kernel;
        var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
        _documentsPlugin = _kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "DocumentsPlugin");
    }

    public async Task SaveDocumentToMemory()
    {
        var text = GetTextFromPdf();
        var targetCollectionName = "collection";

        var pattern = @"(?<=\n|^)(\d+\.\d+\.)(?!\d)";
        var sections = Regex.Split(text, pattern).ToList();

        sections.RemoveAt(0);
        sections.RemoveAll(x => Regex.IsMatch(x, pattern));

        for (var i = 0; i < sections.Count; i++)
        {
            sections[i] = sections[i].Trim();
        }

        for (var i = 0; i < sections.Count; i++)
        {
            var paragraph = sections[i];
            var key = $"GetTos-{i}";
            await _kernel.Memory.SaveInformationAsync(
                collection: targetCollectionName,
                text: paragraph,
                id: key,
                description: "Document: GetTos");
        }
    }

    public async Task SaveTos()
    {
        var text = GetTextFromPdf();
        var sections = SplitDocumentOnMajorSections(text);

        for (var i = 0; i < sections.Count; i++)
        {
            var context = _kernel.CreateNewContext();
            context.Variables["input"] = sections[i];
            var summary = await _documentsPlugin["SummarizeSection"].InvokeAsync(context);

            var section = sections[i];
            var sectionTitle = section.Split('\n')[0];
            sectionTitle = sectionTitle.Trim();
            var key = $"Section-{i}";

            await _kernel.Memory.SaveInformationAsync(
                collection: "Terms_of_Service",
                text: summary.Result,
                id: key,
                description: sectionTitle);
        }

    }

    private static string GetTextFromPdf()
    {
        var fileContent = new StringBuilder();

        using var file = File.OpenRead("Terms_of_Service.pdf");
        using var pdfDocument = PdfDocument.Open(file);

        foreach (var page in pdfDocument.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);
            fileContent.Append(text);
        }

        return fileContent.ToString();
    }

    private static List<string> SplitDocumentOnMajorSections(string text)
    {
        var pattern = @"(?<=\n|^)(\d+\.)(?!\d)";
        var sections = Regex.Split(text, pattern).ToList();
        sections.RemoveAt(0);

        sections.RemoveAll(x => Regex.IsMatch(x, pattern));

        return sections.ToList();
    }
}

public interface IDocumentsService
{
    Task SaveTos();
    Task SaveDocumentToMemory();
}

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

    public DocumentsService(IKernel kernel)
    {
        _kernel = kernel;
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
}

public interface IDocumentsService
{
    Task SaveDocumentToMemory();
}

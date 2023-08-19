using System.Text;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace nlp_processor.Services.Utils;
public static class Utils
{
    public static string GetTextFromPdf(string pdfPath)
    {
        var fileContent = new StringBuilder();

        using var file = File.OpenRead(pdfPath);
        using var pdfDocument = PdfDocument.Open(file);

        foreach (var page in pdfDocument.GetPages())
        {
            var text = ContentOrderTextExtractor.GetText(page);
            fileContent.Append(text);
        }

        return fileContent.ToString();
    }

    public static async Task SaveDocumentToMemory(this IKernel kernel, string text)
    {
        text = await File.ReadAllTextAsync("content_ukr.txt");
        var targetCollectionName = "collection";

        var pattern = @"(?<=\n|^)(\d+\.\d+\.)(?!\d)";
        var sections = Regex.Split(text, pattern);

        var chunks = new List<string>();
        var currentChunk = new StringBuilder();

        // Iterate through the sections and create chunks based on the headers
        for (int i = 0; i < sections.Length; i++)
        {
            if (i % 2 == 0)
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                }
            }
            currentChunk.Append(sections[i]);
        }

        // Add the last chunk if there's anything left
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk.ToString());
        }

        for (var i = 0; i < chunks.Count; i++)
        {
            var paragraph = chunks[i];
            var key = $"GetTos-{i}";
            await kernel.Memory.SaveInformationAsync(
                collection: targetCollectionName,
                text: paragraph,
                id: key,
                description: "Document: GetTos");
        }
    }
}

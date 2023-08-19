using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

var fileContent = new StringBuilder();

using var file = File.OpenRead("C:/Projects/novaposhtahack/docs/Terms_of_Service.pdf");
using var pdfDocument = PdfDocument.Open(file);

foreach (var page in pdfDocument.GetPages())
{
    var text = ContentOrderTextExtractor.GetText(page);
    fileContent.Append(text);
}

var fileText = fileContent.ToString();
Console.WriteLine(fileText);

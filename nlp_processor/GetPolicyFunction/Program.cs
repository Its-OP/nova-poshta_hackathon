using GetPolicyFunction;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using System.Threading;

var modelKey = File.ReadAllText(".key.txt");

var kernel = new KernelBuilder()
    .WithMemory(new SemanticTextMemory(new VolatileMemoryStore(), new OpenAITextEmbeddingGeneration("text-embedding-ada-002", modelKey)))
    .WithOpenAIChatCompletionService("gpt-3.5-turbo", modelKey)
    .Build();

await kernel.SaveDocumentToMemory(Utils.GetTextFromPdf("Terms_of_Service.pdf"));

var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins");
var orchestrationPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "ResponderPlugin");

var context = kernel.CreateNewContext();
var query = "Який документ мені потрібен для отримання посилки ?";

var list = new List<MemoryQueryResult?>();

await foreach (var item in kernel.Memory.SearchAsync("collection", query, 5))
{
    list.Add(item);
}

context.Variables["context"] = list[0].Metadata.Text;
context.Variables["input"] = query;

var result = await orchestrationPlugin["GetIntent"].InvokeAsync(context);
Console.WriteLine(result.Result);

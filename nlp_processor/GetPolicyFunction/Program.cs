using GetPolicyFunction;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;

var modelKey = File.ReadAllText(".key.txt");

var kernel = new KernelBuilder()
    .WithMemory(new SemanticTextMemory(new VolatileMemoryStore(), new OpenAITextEmbeddingGeneration("text-embedding-ada-002", modelKey)))
    .WithOpenAIChatCompletionService("gpt-4", modelKey)
    .Build();

await kernel.SaveDocumentToMemory(Utils.GetTextFromPdf("Terms_of_Service.pdf"));

var pluginsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
var orchestrationPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "ResponderPlugin");

var context = kernel.CreateNewContext();
var query = "Які документи зазначені в пунктах 12.2.1 - 12.2.11 ?";

var list = new List<MemoryQueryResult?>();

await foreach (var item in kernel.Memory.SearchAsync("collection", query, 5))
{
    list.Add(item);
}

context.Variables["context"] = list[0].Metadata.Text;
context.Variables["input"] = query;

var result = await orchestrationPlugin["GetIntent"].InvokeAsync(context);
Console.WriteLine(result.Result);

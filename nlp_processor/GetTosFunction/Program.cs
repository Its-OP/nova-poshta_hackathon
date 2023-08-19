using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Memory;

var modelKey = File.ReadAllText(".key.txt");

var kernel = new KernelBuilder()
    .WithMemory(new SemanticTextMemory(new VolatileMemoryStore(), new OpenAITextEmbeddingGeneration("text-embedding-ada-002", modelKey)))
    .WithOpenAIChatCompletionService("gpt-4-0613", modelKey)
    .Build();

var pluginsDirectory = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Plugins");
var orchestrationPlugin = kernel.ImportSemanticSkillFromDirectory(pluginsDirectory, "OrchestratorPlugin");

var context = kernel.CreateNewContext();
context.Variables["input"] = "Скільки буде коштувати доставка ноутбука з Києва до Львова ?";
context.Variables["options"] = "GetInformationAboutProcessesOfCompany, FindParcelStatus, GetDeliveryPrice, GetDeliveryDate";

var result = await orchestrationPlugin["GetIntent"].InvokeAsync(context);

Console.WriteLine(result.Result);


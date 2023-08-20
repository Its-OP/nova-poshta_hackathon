using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Memory;
using nlp_processor.Services;

var builder = WebApplication.CreateBuilder(args);

var modelKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY") ?? "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";
var azureKey = Environment.GetEnvironmentVariable("AZURE_KEY") ?? "c6GE0yg9jAJXbmK5hSZXcQyf7bjF4TPZWm9qIyH4BBAzSeCLOLKP";

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProcessorService, ProcessorService>();
builder.Services.AddScoped<IDocumentsService, DocumentsService>();

var memoryStore = new AzureCognitiveSearchMemoryStore("https://database.search.windows.net", azureKey);

builder.Services.AddScoped<IKernel>(_ =>
{
    var kernel = new KernelBuilder()
        .WithMemory(new SemanticTextMemory(memoryStore, new OpenAITextEmbeddingGeneration("text-embedding-ada-002", modelKey)))
        .WithOpenAIChatCompletionService("gpt-4", modelKey)
        .Build();

    return kernel;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

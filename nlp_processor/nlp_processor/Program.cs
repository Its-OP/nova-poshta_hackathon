using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.TextEmbedding;
using Microsoft.SemanticKernel.Memory;
using nlp_processor.Services;

var builder = WebApplication.CreateBuilder(args);

var modelKey = File.ReadAllText(".key.txt");
// Add services to the container.
builder.Services.AddScoped<IProcessorService, ProcessorService>();
builder.Services.AddScoped<IKernel>(_ =>
{
    var kernel = new KernelBuilder()
        .WithMemory(new SemanticTextMemory(new VolatileMemoryStore(), new OpenAITextEmbeddingGeneration("text-embedding-ada-002", modelKey)))
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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

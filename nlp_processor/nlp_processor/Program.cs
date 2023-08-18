using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel;
using nlp_processor.Options;

var builder = WebApplication.CreateBuilder(args);
var modelKey = File.ReadAllText(".key.txt");


// Add services to the container.
builder.Services.AddScoped<IKernel>(sp =>
{
    IKernel kernel = Kernel.Builder
        .WithLogger(sp.GetRequiredService<ILogger<IKernel>>())
        .WithMemory(sp.GetRequiredService<ISemanticTextMemory>())
        .WithOpenAIChatCompletionService("gpt-3.5-turbo", modelKey, alsoAsTextCompletion: false, setAsDefault: true)
        .WithOpenAITextEmbeddingGenerationService("text-embedding-ada-002", modelKey, setAsDefault: true)
        .Build();

    sp.GetRequiredService<RegisterSkillsWithKernel>()(sp, kernel);
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

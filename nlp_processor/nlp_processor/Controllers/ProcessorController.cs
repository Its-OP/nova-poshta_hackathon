using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using nlp_processor.DTOs;
using nlp_processor.Services;

namespace nlp_processor.Controllers;

[Route("processor")]
public class ProcessorController : Controller
{
    private readonly IProcessorService _service;
    private readonly IMemoryCache _memoryCache;

    public ProcessorController(IProcessorService service, IMemoryCache memoryCache)
    {
        _service = service;
        _memoryCache = memoryCache;
    }

    [HttpPost("process")]
    public async Task<ActionResult<string>> Process([FromBody] InputDTO input)
    {
        Console.WriteLine($"Received request: {input.Text}");
        var response = await _service.Process(input.Text);

        var history = (string?)_memoryCache.Get("history") ?? string.Empty;
        _memoryCache.Set("history", ConstructHistoryMessage(input.Text, response, history));

        Console.WriteLine($"Sending response: {response}");

        return Json(new { text = response });
    }

    [HttpPost("erase")]
    public ActionResult EraseHistory()
    {
        _memoryCache.Remove("history");
        return Ok("History erased");
    }

    private static string ConstructHistoryMessage(string request, string response, string history)
    {
        var sb = new StringBuilder();

        if (history.Length != 0)
        {
            sb.AppendLine(history);
        }

        sb.AppendLine("User:");
        sb.AppendLine(request);
        sb.AppendLine(string.Empty);

        sb.AppendLine("AI Assistant:");
        sb.AppendLine(response);
        sb.AppendLine(string.Empty);

        return sb.ToString();
    }
}

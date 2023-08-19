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

    [HttpPost]
    public async Task<ActionResult<string>> Process([FromBody] InputDTO input)
    {
        _memoryCache.Set("history", "User:" + "\n" + input.Text);
        var response = await _service.Process(input.Text);

        var history = (string)_memoryCache.Get("history")!;
        _memoryCache.Set("history", history + "\n" + "AI Assistant:" + "\n" + response);

        return response;
    }
}

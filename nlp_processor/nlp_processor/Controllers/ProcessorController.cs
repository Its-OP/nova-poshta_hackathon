using Microsoft.AspNetCore.Mvc;
using nlp_processor.DTOs;
using nlp_processor.Services;

namespace nlp_processor.Controllers;

[Route("processor")]
public class ProcessorController : Controller
{
    private readonly IProcessorService _service;

    public ProcessorController(IProcessorService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<string>> Process([FromBody] InputDTO input)
    {
        return await _service.Process(input.Text);
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using nlp_processor.Services;

namespace nlp_processor.Controllers;

[Route("documents")]
public class DocumentsController : Controller
{
    private readonly IDocumentsService _documentsService;

    public DocumentsController(IDocumentsService documentsService)
    {
        _documentsService = documentsService;
    }

    [HttpGet]
    public async Task<ActionResult> SaveTos()
    {
        await _documentsService.SaveDocumentToMemory();

        return Ok();
    }
}

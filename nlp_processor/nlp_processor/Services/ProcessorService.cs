using Microsoft.SemanticKernel;

namespace nlp_processor.Services;

public class ProcessorService
{
    private IKernel _kernel;

    public ProcessorService(IKernel kernel)
    {
        _kernel = kernel;
    }
}

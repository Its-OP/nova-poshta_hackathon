namespace nlp_processor.Services;

public class ProcessorService : IProcessorService
{
    public Task<string> Process(string input)
    {

    }
}

public interface IProcessorService
{
    Task<string> Process(string input);
}

using Microsoft.SemanticKernel.SkillDefinition;
using System.ComponentModel;

namespace nlp_processor.Plugins;

public class TosPlugin
{
    [SKFunction, Description("Explains policies and processes of Nova Poshta")]
    [SKParameter("input", "Client's question")]
    public string GetInfo()
    {
        return "";
    }
}

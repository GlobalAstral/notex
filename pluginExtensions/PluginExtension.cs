using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;

namespace pluginExtension;

public sealed class PluginExtension : IMarkdownExtension
{
  private readonly BlockParser Parser;
  public PluginExtension(BlockParser parser)
  {
    Parser = parser;
    PluginRegistry.RegisterExtension(this);
  }
  public void Setup(MarkdownPipelineBuilder pipeline)
  {
    if (!pipeline.BlockParsers.Contains(Parser))
      pipeline.BlockParsers.Insert(0, Parser);
  }
  public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

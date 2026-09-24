using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

namespace ast;

public static class RawLatexExtensions
{
  public static MarkdownPipelineBuilder UseRawLatex(this MarkdownPipelineBuilder pipeline)
  {
    pipeline.Extensions.AddIfNotAlready<RawLatexExtension>();
    return pipeline;
  }
}

public sealed class RawLatexBlock(BlockParser? parser) : LeafBlock(parser);

public sealed class RawLatexParser : BlockParser
{
  public RawLatexParser()
  {
    OpeningCharacters = ['%'];
  }
  public override BlockState TryOpen(BlockProcessor processor)
  {
    var line = processor.Line;

    if (!line.ToString().Trim().Equals("%%%"))
      return BlockState.None;

    var block = new RawLatexBlock(this)
    {
      Span = new SourceSpan(processor.Start, processor.Line.End)
    };

    processor.NewBlocks.Push(block);

    return BlockState.ContinueDiscard;
  }

  public override BlockState TryContinue(BlockProcessor processor, Block block)
  {
    if (processor.Line.ToString().Trim() == "%%%")
      return BlockState.BreakDiscard;
    return BlockState.Continue;
  }
}

public sealed class RawLatexExtension : IMarkdownExtension
{
  public void Setup(MarkdownPipelineBuilder pipeline)
  {
    if (!pipeline.BlockParsers.Contains<RawLatexParser>())
      pipeline.BlockParsers.Insert(0, new RawLatexParser());
  }

  public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer) { }
}

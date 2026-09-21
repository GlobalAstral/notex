using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;

namespace ast;

public class Converter(MarkdownDocument document)
{
  public static IEnumerable<IHolder> Container2Holders(ContainerBlock block) => block.Select(b => Convert(b));
  private static IHolder Convert(Block block) => block switch
  {
    MathBlock math => (MathHolder) math,
    FencedCodeBlock fenced => (CodeHolder) fenced,
    YamlFrontMatterBlock yaml => (YamlHolder) yaml,
    CodeBlock code => (CodeHolder) code,
    Abbreviation abbr => (AbbrHolder) abbr,

    MarkdownDocument doc => (DocumentHolder) doc,
    AlertBlock alert => (AlertHolder) alert,
    QuoteBlock quote => (QuoteHolder) quote,
    CustomContainer custom => (CustomContainerHolder) custom,
  };

  public IHolder Convert() => Convert(document);
}

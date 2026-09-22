using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.DefinitionLists;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.SmartyPants;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ast;

public class Converter(MarkdownDocument document)
{
  public static IEnumerable<IHolder> Container2Holders(ContainerBlock block) => block.Select(b => Convert(b));
  public static IEnumerable<IHolder> Container2Holders(ContainerInline inline) => inline.Select(b => Convert(b));

  private static IHolder Convert(Inline inline) => inline switch
  {
    EmojiInline emoji => (LiteralInlineHolder) emoji,
    LiteralInline literalInline => (LiteralInlineHolder) literalInline,
    HtmlInline html => (LiteralInlineHolder) html,
    HtmlEntityInline htmlEntity => (LiteralInlineHolder) htmlEntity,
    CodeInline codeInline => (CodeInlineHolder) codeInline,
    AutolinkInline autolink => (AutolinkInlineHolder) autolink,
    LineBreakInline linebreak => (LineBreakInlineHolder) linebreak,
    EmphasisInline emphasis => (EmphasisInlineHolder) emphasis,
    LinkInline linkInline => (LinkInlineHolder) linkInline,
    AbbreviationInline abbreviation => (AbbreviationInlineHolder) abbreviation,
    MathInline math => (MathInlineHolder) math,
    TaskList taskList => (TaskListHolder) taskList,
    SmartyPant smarty => (SmartyPantHolder) smarty,
    FootnoteLink footnoteLink => (FootnoteLinkHolder) footnoteLink,
    _ => Empty(inline),
  };

  private static EmptyHolder Empty(Block block)
  {
    Console.WriteLine($"Warning: EmptyHolder generated for block {block.GetType().FullName}");
    return new EmptyHolder();
  }

  private static EmptyHolder Empty(Inline inline)
  {
    Console.WriteLine($"Warning: EmptyHolder generated for inline {inline.GetType().FullName}");
    return new EmptyHolder();
  }

  public static IHolder Convert(Block block) => block switch
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
    ListBlock listBlock => (ListHolder) listBlock,
    ListItemBlock listItem => (ListItemHolder) listItem,

    ParagraphBlock paragraph => (ParagraphBlockHolder) paragraph,
    HeadingBlock heading => (HeadingBlockHolder) heading,
    DefinitionTerm definitionTerm => (DefinitionTermHolder) definitionTerm,
    FigureCaption figureCaption => (FigureCaptionHolder) figureCaption,
    
    DefinitionList definitionList => (DefinitionListHolder) definitionList,
    DefinitionItem definitionItem => (DefinitionItemHolder) definitionItem,
    Figure figure => (FigureHolder) figure,
    FooterBlock footer => (FooterBlockHolder) footer,
    FootnoteGroup footnoteGroup => (FootnoteGroupHolder) footnoteGroup,
    Footnote footnote => (FootnoteHolder) footnote,

    Table table => (TableHolder) table,
    TableRow row => (TableRowHolder) row,
    TableCell cell => (TableCellHolder) cell,
    LinkReferenceDefinitionGroup group => new ContainerHolder(Container2Holders(group)),
    FootnoteLinkReferenceDefinition footnote => (FootnoteHolder) footnote,
    _ => Empty(block),
  };

  public IHolder Convert() => Convert(document);
}

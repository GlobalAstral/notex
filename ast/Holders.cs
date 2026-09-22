using System.Text;
using compiler;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.AutoIdentifiers;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.DefinitionLists;
using Markdig.Extensions.Figures;
using Markdig.Extensions.Footers;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.Tables;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using YamlDotNet.Serialization;

namespace ast;

public record CodeHolder(string Code, string? Lang) : IHolder
{
  public static explicit operator CodeHolder(CodeBlock block) => new(IHolder.CollapseLines(block.Lines.Lines), null);
  public static explicit operator CodeHolder(FencedCodeBlock block) => new(IHolder.CollapseLines(block.Lines.Lines), block.Info);
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("listings");
    if (Lang == null)
    {
      IHolder.InsideEnv(builder, context, "lstlisting", Code);
      return;
    }
    IHolder.InsideEnv(builder, context, "lstlisting",  new() {["language"] = Lang}, Code);
  }

  public void Setup(CompilerContext context) { }
};

public record MathHolder(string Math) : IHolder
{
  public static explicit operator MathHolder(MathBlock block) => new(IHolder.CollapseLines(block.Lines.Lines));
  public void Generate(StringBuilder builder, CompilerContext context) => IHolder.InsideEnv(builder, context, "displaymath", Math);
  public void Setup(CompilerContext context) { }
}

public record YamlHolder(YamlHandler Handler) : IHolder
{
  public static explicit operator YamlHolder(YamlFrontMatterBlock block)
  {
    string yaml = IHolder.CollapseLines(block.Lines.Lines);
    var deserializer = new DeserializerBuilder()
      .IgnoreUnmatchedProperties()
      .Build();
    YamlHandler handler = deserializer.Deserialize<YamlHandler>(yaml);
    return new(handler);
  }
  public void Generate(StringBuilder builder, CompilerContext context) { }
  public void Setup(CompilerContext context) => context.ApplyYaml(Handler);
}

public record AbbrHolder(string Label, string Definition) : IHolder
{
  public static explicit operator AbbrHolder(Abbreviation abbr) => new(abbr.Label ?? throw new InvalidOperationException("Abbreviation has no label"), abbr.Text.ToString());
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("glossaries", "acronym");
    context.AddAfterPackages(@"\makeglossaries");
    context.DeclareAbbreviation(Label, out string id);
    builder.AppendLine($@"\newacronym{{{id}}}{{{Label}}}{{{Definition}}}");
  }
  public void Setup(CompilerContext context) { }
}

public record ContainerHolder(IEnumerable<IHolder> Holders) : IHolder
{
  public virtual void Generate(StringBuilder builder, CompilerContext context)
  {
    foreach (IHolder child in Holders)
      child.Run(builder, context);
  }
  public virtual void Setup(CompilerContext context) { }
}

public record DocumentHolder(IEnumerable<IHolder> Holders, Dictionary<string, Abbreviation> Abbreviations) : ContainerHolder(Holders)
{
  public static explicit operator DocumentHolder(MarkdownDocument block) => new(Converter.Container2Holders(block), block.GetAbbreviations() ?? []);
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    IHolder.InsideEnv(builder, context, "document", (builder, context) =>
    {
      foreach (IHolder child in Abbreviations.Values.Select(abbr => (AbbrHolder) abbr))
        child.Run(builder, context);
      base.Generate(builder, context);
      context.ResolveFooters(builder);
    });
  }
}

public record QuoteHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator QuoteHolder(QuoteBlock block) => new(Converter.Container2Holders(block));
  public override void Generate(StringBuilder builder, CompilerContext context)
    => IHolder.InsideEnv(builder, context, "quotation", base.Generate);
}

public record AlertHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator AlertHolder(AlertBlock block) => new(Converter.Container2Holders(block));
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("tcolorbox", "most");
    var props = context.TryGetCustomContainer("alert", new() {["colback"] = "red!5!white", ["colframe"] = "red!75!black"});
    IHolder.InsideEnv(builder, context, "tcolorbox", props, base.Generate);
  }
}

public record CustomContainerHolder(string Label, IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator CustomContainerHolder(CustomContainer block) 
    => new(block.Info ?? throw new NotSupportedException("Nameless Custom Containers are not supported"), Converter.Container2Holders(block));

  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("tcolorbox", "most");
    var props = context.GetCustomContainer(Label) ?? throw new KeyNotFoundException($"Custom Container {Label} was not previously declared");
    IHolder.InsideEnv(builder, context, "tcolorbox", props, base.Generate);
  }
}

public record ListHolder(IEnumerable<IHolder> Holders, bool Ordered, string OrderedStart) : ContainerHolder(Holders)
{
  public static explicit operator ListHolder(ListBlock block) => new(Converter.Container2Holders(block), block.IsOrdered, block.OrderedStart ?? "1");
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    if (Ordered)
    {
      context.AddPackage("enumitem");
      Dictionary<string, string> props = new(){["start"] = OrderedStart};
      IHolder.InsideEnv(builder, context, "enumerate", props, base.Generate);
      return;
    }
    IHolder.InsideEnv(builder, context, "itemize", base.Generate);
  }
}

public record ListItemHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator ListItemHolder(ListItemBlock block) => new(Converter.Container2Holders(block));
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    builder.AppendLine(@"\item");
    base.Generate(builder, context);
  }
}

public record ParagraphBlockHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator ParagraphBlockHolder(ParagraphBlock block) => new(Converter.Container2Holders(block.Inline ?? throw new NullReferenceException("Expected inline content")));
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    base.Generate(builder, context);
    builder.AppendLine();
    builder.AppendLine();
  }
}

public record HeadingBlockHolder(int Level, IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator HeadingBlockHolder(HeadingBlock block) => new(block.Level, Converter.Container2Holders(block.Inline ?? throw new NullReferenceException("Expected inline content")));
  public static explicit operator HeadingBlockHolder(HeadingLinkReferenceDefinition block) => (HeadingBlockHolder) block.Heading;
  private string GetCommand() => Level switch
  {
    1 => "section",
    2 => "subsection",
    3 => "subsubsection",
    4 => "paragraph",
    5 => "subparagraph",
    _ => throw new NotSupportedException($"Heading level {Level} is not supported")
  };
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    builder.Append($@"\{GetCommand()}{{");
    base.Generate(builder, context);
    builder.Append('}');
  }
}

public record DefinitionTermHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator DefinitionTermHolder(DefinitionTerm definitionTerm) => new(Converter.Container2Holders(definitionTerm.Inline ?? throw new NullReferenceException("Expected inline content")));
}

public record FigureCaptionHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator FigureCaptionHolder(FigureCaption figureCaption) => new(Converter.Container2Holders(figureCaption.Inline ?? throw new NullReferenceException("Expected inline content")));
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    builder.Append(@"\caption{");
    base.Generate(builder, context);
    builder.Append('}');
  }
}

public record DefinitionListHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator DefinitionListHolder(DefinitionList definitionList) => new(Converter.Container2Holders(definitionList));
  public override void Generate(StringBuilder builder, CompilerContext context) => IHolder.InsideEnv(builder, context, "description", base.Generate);
}

public record DefinitionItemHolder(IEnumerable<DefinitionTermHolder> Terms, IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator DefinitionItemHolder(DefinitionItem definitionItem)
  {
    List<DefinitionTermHolder> termHolders = [];
    List<IHolder> holders = [];
    foreach (Block child in definitionItem)
    {
      if (child is DefinitionTerm term)
        termHolders.Add((DefinitionTermHolder) term);
      else
        holders.Add(Converter.Convert(child));
    }
    return new(termHolders, holders);
  }

  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    builder.Append(@"\item");
    if (Terms.Any())
    {
      int count = 0;
      builder.Append('[');
      foreach (DefinitionTermHolder term in Terms)
      {
        if (count > 0)
          builder.Append(", ");
        ((IHolder) term).Run(builder, context);
        count++;
      }
      builder.Append(']');
    }
    builder.Append(' ');
    base.Generate(builder, context);
  }
}

public record FigureHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator FigureHolder(Figure figure) => new(Converter.Container2Holders(figure));
  public override void Generate(StringBuilder builder, CompilerContext context) => IHolder.InsideEnv(builder, context, "figure", base.Generate);
}

public record FooterBlockHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator FooterBlockHolder(FooterBlock footer) => new(Converter.Container2Holders(footer));
  public override void Generate(StringBuilder builder, CompilerContext context) { }
  public override void Setup(CompilerContext context) => context.AppendFooter(Holders);
}

public record FootnoteGroupHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator FootnoteGroupHolder(FootnoteGroup group) => new(Converter.Container2Holders(group));
}

public record FootnoteHolder(string Label, IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator FootnoteHolder(Footnote footnote) => new(footnote.Label ?? throw new InvalidOperationException("Footnote has no label"), Converter.Container2Holders(footnote));
  public static explicit operator FootnoteHolder(FootnoteLinkReferenceDefinition footnoteLink) => (FootnoteHolder) footnoteLink.Footnote;
  public override void Generate(StringBuilder builder, CompilerContext context) { }
  public override void Setup(CompilerContext context) => context.DeclareFootnote(Label, Holders);
}

public record TableHolder(IEnumerable<IHolder> Holders, string Specs) : ContainerHolder(Holders)
{
  private static string SpecsToLatex(List<TableColumnDefinition> definitions)
  {
    StringBuilder builder = new();
    definitions.ForEach(definition => builder.Append(definition.Alignment switch
    {
      TableColumnAlign.Left => 'l',
      TableColumnAlign.Center => 'c',
      TableColumnAlign.Right => 'r',
      _ => 'l'
    }));
    return builder.ToString();
  }
  public static explicit operator TableHolder(Table table) {
    string specs = SpecsToLatex(table.ColumnDefinitions);
    return new(Converter.Container2Holders(table), specs);
  }
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("booktabs");
    
    builder.AppendLine(@$"\begin{{tabular}}{{{Specs}}}");
    builder.AppendLine(@"\toprule");
    bool firstdone = false;
    foreach (IHolder child in Holders)
    {
      child.Run(builder, context);
      if (!firstdone)
      {
        builder.AppendLine(@"\midrule");
        firstdone = true;
      }
    }
    builder.AppendLine(@"\bottomrule");
    builder.AppendLine(@"\end{tabular}");
  }
}

public record TableRowHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator TableRowHolder(TableRow row) => new(Converter.Container2Holders(row));
  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    int count = 0;
    foreach (IHolder child in Holders)
    {
      if (count > 0)
        builder.Append(" & ");
      child.Run(builder, context);
      count++;
    }
    builder.Append(@" \\");
  }
}

public record TableCellHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator TableCellHolder(TableCell cell) => new(Converter.Container2Holders(cell));
}

public record EmptyHolder : IHolder
{
  public void Generate(StringBuilder builder, CompilerContext context) { }
  public void Setup(CompilerContext context) { }
}

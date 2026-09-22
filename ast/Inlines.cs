using System.Text;
using compiler;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.Emoji;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.SmartyPants;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax.Inlines;
using YamlDotNet.Serialization;

namespace ast;

public record LiteralInlineHolder(string Content) : IHolder
{
  public static explicit operator LiteralInlineHolder(LiteralInline literalInline) => new(literalInline.Content.ToString());
  public static explicit operator LiteralInlineHolder(HtmlInline html) => new(html.Tag);
  public static explicit operator LiteralInlineHolder(HtmlEntityInline htmlEntity) => new(htmlEntity.Transcoded.ToString());
  public static explicit operator LiteralInlineHolder(EmojiInline emoji) => new(emoji.Content.ToString());
  public void Generate(StringBuilder builder, CompilerContext context) => builder.Append(Content);
  public void Setup(CompilerContext context) { }
}

public record CodeInlineHolder(string Code) : IHolder
{
  public static explicit operator CodeInlineHolder(CodeInline codeInline) => new(codeInline.Content);
  static bool IsValidDelimiter(char c)
  {
    return !char.IsLetterOrDigit(c)
      && !char.IsWhiteSpace(c)
      && c is not '['
      && c is not '*';
  }
  static char FindLatexDelimiter(string code)
  {
    for (char c = '!'; c <= '~'; c++)
    {
      if (!IsValidDelimiter(c) || code.Contains(c))
        continue;
      return c;
    }
    throw new InvalidOperationException("Could not find a delimiter for inline listing");
  }

  public void Generate(StringBuilder builder, CompilerContext context)
  {
    char delimiter = FindLatexDelimiter(Code);
    builder.Append($@"\lstinline{delimiter}{Code}{delimiter}");
  }

  public void Setup(CompilerContext context) { }
}

public record AutolinkInlineHolder(string Url, bool IsEmail) : IHolder
{
  public static explicit operator AutolinkInlineHolder(AutolinkInline autolink) => new(autolink.Url, autolink.IsEmail);
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("hyperref");
    if (IsEmail)
      builder.Append($@"\href{{mailto:{Url}}}{{{Url}}}");
    else
      builder.Append($@"\url{{{Url}}}");
  }

  public void Setup(CompilerContext context) { }
}

public record LineBreakInlineHolder(bool Hard) : IHolder
{
  public static explicit operator LineBreakInlineHolder(LineBreakInline lineBreak) => new(lineBreak.IsHard);
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    if (Hard)
      builder.Append(@"\\");
    else
      builder.Append(' ');
  }

  public void Setup(CompilerContext context) { }
}

public record ContainerInlineHolder(IEnumerable<IHolder> Holders) : IHolder
{
  public virtual void Generate(StringBuilder builder, CompilerContext context)
  {
    foreach (IHolder child in Holders)
      child.Run(builder, context);
  }
  public virtual void Setup(CompilerContext context) { }
}

public record EmphasisInlineHolder(IEnumerable<IHolder> Holders, int Level) : ContainerInlineHolder(Holders)
{
  public static explicit operator EmphasisInlineHolder(EmphasisInline emphasis) => new(Converter.Container2Holders(emphasis), emphasis.DelimiterCount);
  private string GetCommand() => Level switch
  {
    1 => "emph",
    2 => "textbf",
    _ => throw new NotSupportedException($"Unsupported Emphasis level: {Level}")
  };

  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    builder.Append($@"\{GetCommand()}{{");
    base.Generate(builder, context);
    builder.Append('}');
  }
}

public record LinkInlineHolder(string Url, bool IsImage, IEnumerable<IHolder> Holders) : ContainerInlineHolder(Holders)
{
  public static explicit operator LinkInlineHolder(LinkInline linkInline) 
    => new(linkInline.GetDynamicUrl?.Invoke() ?? linkInline.Url ?? throw new InvalidOperationException("Link has no Url"), linkInline.IsImage, Converter.Container2Holders(linkInline));

  public override void Generate(StringBuilder builder, CompilerContext context)
  {
    if (IsImage)
    {
      context.AddPackage("graphicx");
      builder.Append(@"\includegraphics{");
      builder.Append(Url);
      builder.Append('}');
      return;
    }
    context.AddPackage("hyperref");
    builder.Append(@"\href{");
    builder.Append(Url);
    builder.Append("}{");
    base.Generate(builder, context);
    builder.Append('}');
  }
}

public record AbbreviationInlineHolder(string Label) : IHolder
{
  public static explicit operator AbbreviationInlineHolder(AbbreviationInline abbreviation) => new(abbreviation.Abbreviation.Label!);
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    if (!context.AbbreviationDeclared(Label, out string id))
      throw new KeyNotFoundException($"Abbreviation {Label} does not exist");
    builder.Append($@"\gls{{{id}}}");
  }
  public void Setup(CompilerContext context) { }
}

public record MathInlineHolder(string Math) : IHolder
{
  public static explicit operator MathInlineHolder(MathInline math) => new(math.Content.ToString());
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    builder.Append(@"\(");
    builder.Append(Math);
    builder.Append(@"\)");
  }
  public void Setup(CompilerContext context) { }
}

public record TaskListHolder(bool Checked) : IHolder
{
  public static explicit operator TaskListHolder(TaskList taskList) => new(taskList.Checked);
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("wasysym", "nointegrals");
    builder.Append(Checked ? @"\CheckedBox" : @"\Square");
  }

  public void Setup(CompilerContext context) { }
}

public record SmartyPantHolder(SmartyPantType Type) : IHolder
{
  public static explicit operator SmartyPantHolder(SmartyPant smarty) => new(smarty.Type);
  public void Generate(StringBuilder builder, CompilerContext context) => builder.Append(Type switch
  {
    SmartyPantType.LeftQuote => "`",
    SmartyPantType.RightQuote => "'",
    SmartyPantType.LeftDoubleQuote => "``",
    SmartyPantType.RightDoubleQuote => "''",
    SmartyPantType.Dash2 => "--",
    SmartyPantType.Dash3 => "---",
    SmartyPantType.Ellipsis => @"\ldots{}",
    SmartyPantType.LeftAngleQuote => @"\guillemotleft",
    SmartyPantType.RightAngleQuote => @"\guillemotright",
    _ => throw new NotSupportedException($"Invalid SmartyPant type {Type}")
  });
  public void Setup(CompilerContext context) { }
}

public record FootnoteLinkHolder(string Label, bool Ignore) : IHolder
{
  public static explicit operator FootnoteLinkHolder(FootnoteLink footnoteLink) => new(footnoteLink.Footnote.Label!, footnoteLink.IsBackLink);
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    if (Ignore)
      return;
    IEnumerable<IHolder> holders = context.GetFootnote(Label, out string id);
    builder.Append(@"\footnote{\label{");
    builder.Append(id);
    builder.Append('}');
    foreach (IHolder child in holders)
      child.Run(builder, context);
    builder.Append('}');
  }

  public void Setup(CompilerContext context) { }
}

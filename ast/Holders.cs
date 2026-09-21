using System.Security.Cryptography;
using System.Text;
using Markdig.Extensions.Abbreviations;
using Markdig.Extensions.Alerts;
using Markdig.Extensions.CustomContainers;
using Markdig.Extensions.Mathematics;
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
  public static explicit operator AbbrHolder(Abbreviation abbr) => new(abbr.Label!, abbr.Text.ToString());
  public void Generate(StringBuilder builder, CompilerContext context)
  {
    context.AddPackage("glossaries", "acronym");
    context.AddAfterPackages(@"\makeglossaries");
    string id = IHolder.MakeId(Label);
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

public record DocumentHolder(IEnumerable<IHolder> Holders) : ContainerHolder(Holders)
{
  public static explicit operator DocumentHolder(ContainerBlock block) => new(Converter.Container2Holders(block));
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

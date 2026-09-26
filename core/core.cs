using System.Collections;
using System.Text;
using ast;
using compiler;
using Markdig;
using Markdig.Syntax;
using MoonSharp.Interpreter;

namespace core;

public class OrderedSet<T> : ICollection<T>
{
  protected readonly List<T> Ordered = [];
  protected readonly HashSet<T> Values = [];
  public int Count => Values.Count;
  public bool IsReadOnly => false;
  public void Add(T item)
  {
    if (Contains(item))
      return;
    Ordered.Add(item);
    Values.Add(item);
  }
  public void Clear()
  {
    Ordered.Clear();
    Values.Clear();
  }
  public bool Contains(T item) => Values.Contains(item);
  public void CopyTo(T[] array, int arrayIndex) => Ordered.CopyTo(array, arrayIndex);
  public IEnumerator<T> GetEnumerator() => Ordered.GetEnumerator();
  public bool Remove(T item) => Ordered.Remove(item) && Values.Remove(item);
  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class MarkdownHelpers
{
  public static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .UseYamlFrontMatter()
    .UseRawLatex()
    .UseSmartyPants()
    .Build();
}

public static class LuaHelpers
{
  public static void IfPresent<T>(this Dictionary<object, object?> dict, object key, Action<T> action)
  {
    if (dict.TryGetValue(key, out var value))
    {
      if (value is T t)
        action(t);
      else if (value is IConvertible)
      {
        try {
          action((T)Convert.ChangeType(value, typeof(T)));
        } catch { }
      }

    }
  }
  public static void CreateLuaEnvironment(this Script script, CompilerContext context, StringBuilder output)
  {
    script.Globals["outputPush"] = (string str) => output.Append(str);
    script.Globals["outputPushln"] = (string str) => output.AppendLine(str);
    script.Globals["addPackage"] = (string package, string? properties) =>
    {
      if (properties == null)
        context.AddPackage(package);
      else
        context.AddPackage(package, properties);
    };
    script.Globals["addAfterPackage"] = (string directive) => context.AddAfterPackages(directive);
    script.Globals["addFooter"] = (string content) =>
    {
      MarkdownDocument document = Markdown.Parse(content, MarkdownHelpers.Pipeline);
      IEnumerable<IHolder> holders = Converter.Container2Holders(document);
      context.AppendFooter(holders);
    };
    script.Globals["addLanguage"] = (string lang) => context.Languages.Add(lang);
    script.Globals["setDocument"] = (Dictionary<object, object?> table) =>
    {
      table.IfPresent("class", (string clazz) => context.DocumentConfigs.Class = clazz);
      table.IfPresent("fontSize", (int fs) => context.DocumentConfigs.FontSize = fs);
      table.IfPresent("paper", (string paper) => context.DocumentConfigs.Paper = paper);
      table.IfPresent("orientation", (string orient) => context.DocumentConfigs.Orientation = orient);
      table.IfPresent("side", (string side) => context.DocumentConfigs.Side = side);
      table.IfPresent("column", (string column) => context.DocumentConfigs.Column = column);
      table.IfPresent("draft", (bool draft) => context.DocumentConfigs.Draft = draft);
      table.IfPresent("title", (string? title) => context.DocumentConfigs.Title = title);
      table.IfPresent("author", (string? author) => context.DocumentConfigs.Author = author);
      table.IfPresent("date", (string date) => context.DocumentConfigs.Date = date);
      table.IfPresent("paragraphIndent", (int paragraphIndent) => context.DocumentConfigs.ParagraphIndent = paragraphIndent);
      table.IfPresent("paragraphSkip", (int paragraphSkip) => context.DocumentConfigs.ParagraphSkip = paragraphSkip);
      table.IfPresent("lineSpread", (float lineSpread) => context.DocumentConfigs.LineSpread = lineSpread);
      table.IfPresent("sectionNumberDepth", (int sectionNumberDepth) => context.DocumentConfigs.SectionNumberDepth = sectionNumberDepth);
      table.IfPresent("tableOfContents", (bool tableOfContents) => context.DocumentConfigs.TableOfContents = tableOfContents);
      table.IfPresent("tableOfContentsDepth", (int tableOfContentsDepth) => context.DocumentConfigs.TableOfContentsDepth = tableOfContentsDepth);
      table.IfPresent("pageNumbering", (string pageNumbering) => context.DocumentConfigs.PageNumbering = pageNumbering);
      table.IfPresent("pageNumber", (int pageNumber) => context.DocumentConfigs.PageNumber = pageNumber);
      table.IfPresent("pageStyle", (string pageStyle) => context.DocumentConfigs.PageStyle = pageStyle);
      table.IfPresent("geometry", (string geometry) => context.DocumentConfigs.Geometry = geometry);
    };
    script.Globals["addFootnote"] = (string label, string content) =>
    {
      MarkdownDocument document = Markdown.Parse(content, MarkdownHelpers.Pipeline);
      IEnumerable<IHolder> holders = Converter.Container2Holders(document);
      context.DeclareFootnote(label, holders);
    };
    script.Globals["addCustomContainer"] = (string name, Dictionary<string, string> properties) => context.AddCustomContainer(name, properties);
  }
}

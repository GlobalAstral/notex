
using System.Text;

using PackageManager = core.OrderedSet<(string package, string? prop)>;
using PropertyManager = System.Collections.Generic.Dictionary<string, string>;
using CustomContainerManager = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using ast;
using core;

namespace compiler;
public class CompilerContext
{
  private PackageManager Packages {get;} = [];
  private OrderedSet<string> AfterPackages {get;} = [];
  private List<IHolder> Footers {get;} = [];
  public OrderedSet<string> Languages {get;} = [];
  public DocumentConfigs DocumentConfigs = new();
  
  private Dictionary<string, IEnumerable<IHolder>> DeclaredFootnotes {get;} = [];
  private CustomContainerManager CustomContainers {get;} = [];
  private OrderedSet<string> DeclaredAbbreviations {get;} = [];
  
  public void ApplyYaml(YamlHandler handler)
  {
    DocumentConfigs = handler.Document;

    foreach (string package in handler.Packages)
    {
      if (package.Length == 0)
        continue;
      string[] splits = package.Split(':');
      Packages.Add((splits[0], splits.Length > 1 ? splits[1] : null));
    }

    foreach (string after in handler.AfterPackages)
    {
      if (after.Length == 0)
        continue;
      AfterPackages.Add(after);
    }

    foreach (string lang in handler.Languages)
    {
      if (lang.Length == 0)
        continue;
      Languages.Add(lang);
    }
  }
  public void AddPackage(string package) => Packages.Add((package, null));
  public void AddPackage(string package, string properties) => Packages.Add((package, properties));
  public void AddAfterPackages(string directive) => AfterPackages.Add(directive);
  public void AddCustomContainer(string name, PropertyManager properties) => CustomContainers[name] = properties;
  public PropertyManager? GetCustomContainer(string name) => CustomContainers[name];
  public PropertyManager TryGetCustomContainer(string name, PropertyManager def)
  {
    if (!CustomContainers.TryGetValue(name, out var val))
    {
      val = def;
      CustomContainers[name] = val;
    }
    return val;
  }
  public bool AbbreviationDeclared(string Label, out string id)
  {
    id = IHolder.MakeId(Label);
    return DeclaredAbbreviations.Contains(id);
  }
  public void DeclareAbbreviation(string Label, out string id)
  {
    id = IHolder.MakeId(Label);
    DeclaredAbbreviations.Add(id);
  }
  public void AppendFooter(IEnumerable<IHolder> footer) => Footers.AddRange(footer);
  public bool FootnoteDeclared(string Label, out string id)
  {
    id = IHolder.MakeId(Label);
    return DeclaredFootnotes.ContainsKey(id);
  }
  public void DeclareFootnote(string Label, IEnumerable<IHolder> holders)
  {
    string id = IHolder.MakeId(Label);
    DeclaredFootnotes[id] = holders;
  }
  public IEnumerable<IHolder> GetFootnote(string Label, out string id)
  {
    if (!FootnoteDeclared(Label, out string val))
      throw new KeyNotFoundException($"Footnote {Label} does not exist");
    id = val;
    return DeclaredFootnotes[id];
  }

  public void ResolvePackages(StringBuilder builder)
  {
    foreach ((string package, string? prop) in Packages)
    {
      if (prop == null)
        builder.AppendLine($@"\usepackage{{{package}}}");
      else
        builder.AppendLine($@"\usepackage[{prop}]{{{package}}}");
    }
  }

  public void ResolveAfterPackages(StringBuilder builder)
  {
    foreach (string after in AfterPackages)
      builder.AppendLine(after);
  }

  public void ResolveFooters(StringBuilder builder)
  {
    foreach (IHolder child in Footers)
      child.Run(builder, this);
  }
}

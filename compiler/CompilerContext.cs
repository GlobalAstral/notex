
using System.Text;

using PackageManager = System.Collections.Generic.HashSet<(string package, string? prop)>;
using PropertyManager = System.Collections.Generic.Dictionary<string, string>;
using CustomContainerManager = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, string>>;
using ast;

namespace compiler;
public class CompilerContext
{
  private PackageManager Packages {get;} = [];
  private List<string> AfterPackages {get;} = [];
  private List<IHolder> Footers {get;} = [];
  
  private Dictionary<string, IEnumerable<IHolder>> DeclaredFootnotes {get;} = [];
  private CustomContainerManager CustomContainers {get;} = [];
  private HashSet<string> DeclaredAbbreviations {get;} = [];
  
  public void ApplyYaml(YamlHandler handler) => throw new NotImplementedException();
  public void AddPackage(string package) => Packages.Add((package, null));
  public void AddPackage(string package, string properties) => Packages.Add((package, properties));
  public void AddAfterPackages(string directive)
  {
    if (!AfterPackages.Contains(directive))
      AfterPackages.Add(directive);
  }
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

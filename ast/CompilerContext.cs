namespace ast;

using System.Text;

using PackageManager = HashSet<(string package, string? prop)>;
using PropertyManager = Dictionary<string, string>;
using CustomContainerManager = Dictionary<string, Dictionary<string, string>>;

public class CompilerContext
{
  private PackageManager Packages {get;} = [];
  private StringBuilder AfterPackages {get;} = new();
  private CustomContainerManager CustomContainers {get;} = [];
  public void ApplyYaml(YamlHandler handler) => throw new NotImplementedException();
  public void AddPackage(string package) => Packages.Add((package, null));
  public void AddPackage(string package, string properties) => Packages.Add((package, properties));
  public void AddAfterPackages(string directive) => AfterPackages.AppendLine(directive);
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
}

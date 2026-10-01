using MoonSharp.Interpreter;

namespace pluginExtension;

public interface IPluginNode
{
  DynValue ToLua(Script lua);
  void FromLua(DynValue value);
}

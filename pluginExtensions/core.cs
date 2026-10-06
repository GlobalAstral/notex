using MoonSharp.Interpreter;

namespace pluginExtension;

public interface IPluginNode
{
  DynValue ToLua(Script lua);
  void FromLua(Script lua, DynValue value);
}

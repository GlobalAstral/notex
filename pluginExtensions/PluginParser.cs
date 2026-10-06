using Markdig.Parsers;
using MoonSharp.Interpreter;

namespace pluginExtension;

public class PluginParser(Script lua, DynValue tryOpen) : BlockParser, IPluginNode
{
  public void FromLua(Script lua, DynValue value) { }

  public DynValue ToLua(Script lua) => DynValue.True;

  public override BlockState TryOpen(BlockProcessor processor)
  {
    PluginProcessor proc = new(processor);
    DynValue luaproc = proc.ToLua(lua);
    DynValue state = lua.Call(tryOpen, luaproc);
    double number = state.Type == DataType.Number ? state.Number : throw new ScriptRuntimeException("Parser function expected to return a BlockState");
    if (!Enum.IsDefined((BlockState)(int)number))
      throw new ArgumentException($"Invalid {nameof(BlockState)} value: {number}");
    return (BlockState)(int)number;
  }
}

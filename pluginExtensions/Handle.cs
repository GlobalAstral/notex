using System.Numerics;
using MoonSharp.Interpreter;

namespace pluginExtension;

public struct Handle : IPluginNode
{
  public enum Type : byte
  {
    RawTextBlock,
    LeafBlock,
    ContainerBlock,
    LeafInline,
    ContainerInline,
    Parser,
    Holder,
  }
  private static readonly int TypeBits = BitOperations.Log2((uint)(Enum.GetValues<Type>().Length - 1)) + 1;
  private static readonly int IntBits = 8 * sizeof(int);
  private static readonly int PayloadMask = (1 << (IntBits - TypeBits)) - 1;
  private static int current_id = 0;
  private Handle(int compoundID)
  {
    CompoundID = compoundID;
  }
  int CompoundID;
  public static Handle Create(Type type)
  {
    byte b = (byte)type;
    int current = current_id++;
    int max = (int) Math.Pow(2, IntBits - TypeBits);
    if (current >= max)
      throw new InvalidDataException($"Cannot have more than {max - 1} handles");
    return new Handle((b << TypeBits) | current);
  }

  public new readonly Type GetType() => (Type) (CompoundID >> (IntBits - TypeBits));
  private readonly int GetPayload() => CompoundID & PayloadMask;
  public override readonly string ToString() => $"{GetType()}:{GetPayload()}";

  public readonly bool Equals(Handle other) => CompoundID == other.CompoundID;
  public readonly bool Equals(int other) => CompoundID == other;
  public override readonly bool Equals(object? obj) => (obj is Handle other && Equals(other)) || (obj is int o && Equals(o));
  public override readonly int GetHashCode() => CompoundID;
  public static bool operator ==(Handle left, Handle right) => left.Equals(right);
  public static bool operator !=(Handle left, Handle right) => !(left == right);
  public static bool operator ==(Handle left, int right) => left.Equals(right);
  public static bool operator !=(Handle left, int right) => !left.Equals(right);
  public static bool operator ==(int left, Handle right) => right.Equals(left);
  public static bool operator !=(int left, Handle right) => !right.Equals(left);

  public readonly DynValue ToLua(Script lua)
  {
    int temp = CompoundID;
    Table table = new(lua);
    table["handle"] = () => DynValue.NewNumber(temp);
    table["snapshot"] = (Table self) => From(lua, self).Get().ToLua(lua);
    table["load"] = (Table self, DynValue value) => From(lua, self).Get().FromLua(lua, value);
    return DynValue.NewTable(table);
  }

  public void FromLua(Script lua, DynValue value)
  {
    if (value.Type != DataType.Table)
      throw new InvalidCastException($"LuaValue {value} cannot be converted to a Table");
    Table table = value.Table;
    DynValue i = table.Get("handle");
    DynValue r = lua.Call(i);
    if (r.Type != DataType.Number)
      throw new InvalidCastException($"LuaValue {r} cannot be converted to a Number");
    int integer = (int) r.Number;
    CompoundID = integer;
  }
  public Handle(Script lua, Table dyn) => FromLua(lua, DynValue.NewTable(dyn));
  public static Handle From(Script lua, Table table) => new(lua, table);
  public readonly IPluginNode Get() => PluginRegistry.Get(this);
}

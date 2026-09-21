using System.Security.Cryptography;
using System.Text;
using Markdig.Helpers;
using Markdig.Syntax;

namespace ast;

public interface IHolder
{
  public static string MakeId(string Label)
  {
    Span<byte> hash = stackalloc byte[32];
    SHA256.HashData(Encoding.UTF8.GetBytes(Label), hash);
    return $"ID{Convert.ToHexStringLower(hash)}";
  }
  static void InsideEnv(StringBuilder builder, CompilerContext context, string env, Dictionary<string, string> props, Action<StringBuilder, CompilerContext> action)
  {
    if (props.Count > 0)
      builder.AppendLine($@"\begin{{{env}}}[{string.Join(',', props.Select(pair => $"{pair.Key}={pair.Value}"))}]");
    else
      builder.AppendLine($@"\begin{{{env}}}");

    action(builder, context);

    builder.AppendLine($@"\end{{{env}}}");
  }
  static void InsideEnv(StringBuilder builder, CompilerContext context, string env, Action<StringBuilder, CompilerContext> action) => InsideEnv(builder, context, env, [], action);
  static void InsideEnv(StringBuilder builder, CompilerContext context, string env, Dictionary<string, string> props, string append) => InsideEnv(builder, context, env, props, (builder, context) => builder.AppendLine(append));
  static void InsideEnv(StringBuilder builder, CompilerContext context, string env, string append) => InsideEnv(builder, context, env, (builder, context) => builder.AppendLine(append));
  
  static void ForeachChild(ContainerBlock block, Action<Block> action)
  {
    foreach (Block child in block)
      action(child);
  }

  public static string CollapseLines(StringLine[] lines) => string.Join('\n', lines.Select(line => line.Slice.ToString()));

  void Setup(CompilerContext context);
  void Generate(StringBuilder builder, CompilerContext context);

  public void Run(StringBuilder builder, CompilerContext context)
  {
    Setup(context);
    Generate(builder, context);
  }
}

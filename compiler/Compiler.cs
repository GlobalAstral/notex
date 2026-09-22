
using System.Text;
using ast;

namespace compiler;

public static class Compiler
{
  public static string Compile(IHolder document)
  {
    StringBuilder output = new();
    CompilerContext context = new();

    StringBuilder builder = new();

    document.Run(builder, context);

    output.AppendLine(@"\documentclass[11pt,a4paper]{article}");

    context.ResolvePackages(output);
    context.ResolveAfterPackages(output);

    output.AppendLine(builder.ToString());

    return output.ToString();
  }
}

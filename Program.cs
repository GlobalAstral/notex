
using System.Text;
using ast;
using compiler;
using core;
using Markdig;
using Markdig.Syntax;
using MoonSharp.Interpreter;

static void Process(string filepath, bool debug, StringBuilder output, CompilerContext context)
{
  if (Path.GetExtension(filepath) != ".md")
    throw new ArgumentException("Filename is not a .md file");

  string texfile = Path.ChangeExtension(filepath, ".tex");

  string markdown = File.ReadAllText(filepath).Replace(Environment.NewLine, "\n");

  MarkdownDocument document = Markdown.Parse(markdown, MarkdownHelpers.Pipeline);

  if (debug)
  {
    foreach (var block in document)
      Console.WriteLine(block.GetType().FullName);
  }

  IHolder holder = new Converter(document).Convert();

  string tex = Compiler.Compile(holder, output, context);

  File.WriteAllText(texfile, tex);
}

static void RunPlugins(Script lua)
{
  if (!Directory.Exists("plugins"))
    return;
  foreach (string path in Directory.GetFiles("plugins"))
  {
    if (Path.GetExtension(path) != ".lua")
      continue;
    lua.Globals["__FILE__"] = path;
    string content = File.ReadAllText(path).Replace(Environment.NewLine, "\n");
    lua.DoString(content);
  }
}


List<string> arguments = [.. args];

bool debug = arguments.Remove("--debug");

string[] files = [ .. args ];

if (files.Length < 1)
  throw new ArgumentException("Command line argument expected: filename");

Script lua = new(CoreModules.Preset_HardSandbox);

StringBuilder output = new();
CompilerContext context = new();

lua.CreateLuaEnvironment(context, output);


RunPlugins(lua);

context.SortHandlers();

context.RunSetup();

foreach (string filepath in arguments)
  Process(filepath, debug, output, context);

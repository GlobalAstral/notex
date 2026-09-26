
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

  string markdown = File.ReadAllText(filepath);

  

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

List<string> arguments = [.. args];

bool debug = arguments.Remove("--debug");

string[] files = [ .. args ];

if (files.Length < 1)
  throw new ArgumentException("Command line argument expected: filename");

StringBuilder output = new();
CompilerContext context = new();

Script lua = new();

lua.CreateLuaEnvironment(context, output); //TODO 



foreach (string filepath in arguments)
  Process(filepath, debug, output, context);

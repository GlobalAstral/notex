
using ast;
using compiler;
using Markdig;
using Markdig.Syntax;

static void Process(string filepath, bool debug)
{
  if (Path.GetExtension(filepath) != ".md")
    throw new ArgumentException("Filename is not a .md file");

  string texfile = Path.ChangeExtension(filepath, ".tex");

  string markdown = File.ReadAllText(filepath);

  var pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .UseYamlFrontMatter()
    .UseSmartyPants()
    .Build();

  MarkdownDocument document = Markdown.Parse(markdown, pipeline);

  if (debug)
  {
    foreach (var block in document)
      Console.WriteLine(block.GetType().FullName);
  }

  IHolder holder = new Converter(document).Convert();

  string tex = Compiler.Compile(holder);

  File.WriteAllText(texfile, tex);
}

List<string> arguments = [.. args];

bool debug = arguments.Remove("--debug");

string[] files = [ .. args ];

if (files.Length < 1)
  throw new ArgumentException("Command line argument expected: filename");

foreach (string filepath in arguments)
  Process(filepath, debug);

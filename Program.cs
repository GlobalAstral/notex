
using ast;
using compiler;
using Markdig;
using Markdig.Extensions.Abbreviations;
using Markdig.Syntax;

// string filepath = (string?)args.GetValue(0) ?? throw new ArgumentException("Command line argument expected: filename");
string filepath = "test.md";

if (Path.GetExtension(filepath) != ".md")
  throw new ArgumentException("Filename is not a .md file");

string texfile = Path.ChangeExtension(filepath, ".tex");

string markdown = File.ReadAllText(filepath);

var pipeline = new MarkdownPipelineBuilder()
  .UseFootnotes()
  .UseAdvancedExtensions()
  .UseSmartyPants()
  .Build();

MarkdownDocument document = Markdown.Parse(markdown, pipeline);

IHolder holder = new Converter(document).Convert();

string tex = Compiler.Compile(holder);

File.WriteAllText(texfile, tex);

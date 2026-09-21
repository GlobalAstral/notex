
using Markdig;
using Markdig.Syntax;

string filepath = (string?)args.GetValue(0) ?? throw new ArgumentException("Command line argument expected: filename");

if (Path.GetExtension(filepath) != ".md")
  throw new ArgumentException("Filename is not a .md file");

string texfile = Path.ChangeExtension(filepath, ".tex");

string markdown = File.ReadAllText(filepath);

var pipeline = new MarkdownPipelineBuilder()
  .UseAdvancedExtensions()
  .Build();

MarkdownDocument document = Markdown.Parse(markdown, pipeline);

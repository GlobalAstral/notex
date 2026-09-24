
using System.Globalization;
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

    DocumentConfigs cfg = context.DocumentConfigs;

    output.AppendLine(context.DocumentConfigs.GetDocumentClass());

    context.ResolvePackages(output);
    context.ResolveAfterPackages(output);
    
    output.AppendLine($@"\setlength{{\parindent}}{{{cfg.ParagraphIndent}pt}}");
    output.AppendLine($@"\setlength{{\parskip}}{{{cfg.ParagraphSkip}pt}}");
    output.AppendLine($@"\linespread{{{cfg.LineSpread.ToString(CultureInfo.InvariantCulture)}}}");

    output.AppendLine($@"\setcounter{{secnumdepth}}{{{cfg.SectionNumberDepth}}}");
    output.AppendLine($@"\setcounter{{tocdepth}}{{{cfg.TableOfContentsDepth}}}");

    output.AppendLine($@"\pagenumbering{{{cfg.PageNumbering}}}");
    output.AppendLine($@"\setcounter{{page}}{{{cfg.PageNumber}}}");
    output.AppendLine($@"\pagestyle{{{cfg.PageStyle}}}");

    if (cfg.Title != null)
      output.AppendLine($@"\title{{{cfg.Title}}}");
    if (cfg.Author != null)
      output.AppendLine($@"\author{{{cfg.Author}}}");
    
    output.AppendLine($@"\date{{{cfg.Date}}}");

    output.AppendLine(@"\begin{document}");

    if (cfg.Title != null)
      output.AppendLine(@"\maketitle");
    if (cfg.TableOfContents)
      output.AppendLine(@"\tableofcontents");

    output.AppendLine(builder.ToString());

    output.AppendLine(@"\end{document}");

    return output.ToString();
  }
}

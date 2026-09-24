using core;

namespace ast;

public class DocumentConfigs
{
  public string Class {get; set;} = "article";
  public int FontSize {get; set;} = 11;
  public string Paper {get; set;} = "a4paper";
  public string Orientation {get; set;} = "portrait";
  public string Side {get; set;} = "oneside";
  public string Column {get; set;} = "onecolumn";
  public bool Draft {get; set;} = false;

  public string? Title {get; set;} = null;
  public string? Author {get; set;} = null;
  public string Date {get; set;} = @"\today";

  public int ParagraphIndent {get; set;} = 0;
  public int ParagraphSkip {get; set;} = 6;
  public float LineSpread {get; set;} = 1.2f;

  public int SectionNumberDepth {get; set;} = 5;
  public bool TableOfContents {get; set;} = true;
  public int TableOfContentsDepth {get; set;} = 2;
  public string PageNumbering {get; set;} = "arabic";
  public int PageNumber {get; set;} = 1;
  public string PageStyle {get; set;} = "plain";

  public string GetDocumentClass() => $@"\documentclass[{FontSize}pt,{Paper},{Orientation},{Side},{Column},{(Draft ? "draft" : "final")}]{{{Class}}}";
}

public class YamlHandler
{
  public OrderedSet<string> Packages {get; set;} = [];
  public OrderedSet<string> AfterPackages {get; set;} = [];
  public DocumentConfigs Document {get; set;} = new();
}

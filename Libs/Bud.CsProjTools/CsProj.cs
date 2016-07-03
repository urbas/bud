using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bud.CsProjTools {
  public static class CsProj {
    private const string Indentation = "  ";
    public static readonly IEnumerable<KeyValuePair<string, string>> NoAttributes = Enumerable.Empty<KeyValuePair<string, string>>();
    public static readonly IEnumerable<CsProjElement> NoElements = Enumerable.Empty<CsProjElement>();

    public static string Generate(params CsProjElement[] elements) {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>").Append('\n');
      stringBuilder.Append(@"<Project ToolsVersion=""14.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">").Append('\n');
      foreach (var element in elements) {
        Append(stringBuilder, element, Indentation);
        stringBuilder.Append('\n');
      }
      stringBuilder.Append(@"</Project>");
      return stringBuilder.ToString();
    }

    private static void Append(StringBuilder stringBuilder, CsProjElement element, string indentantion) {
      stringBuilder.Append(indentantion)
        .Append($@"<{element.Name}");
      foreach (var attribute in element.Attributes) {
        stringBuilder.Append($@" {attribute.Key}=""{attribute.Value}""");
      }
      if (element.Content.HasValue || element.Children.Any()) {
        stringBuilder.Append(">");
        if (element.Children.Any()) {
          stringBuilder.Append('\n');
          var nextIndentation = indentantion + Indentation;
          foreach (var child in element.Children) {
            Append(stringBuilder, child, nextIndentation);
            stringBuilder.Append('\n');
          }
          stringBuilder.Append(indentantion);
        }
        if (element.Content.HasValue) {
          stringBuilder.Append(element.Content.Value);
        }
        stringBuilder.Append($@"</{element.Name}>");
      } else {
        stringBuilder.Append(" />");
      }
    }

    public static CsProjElement Import(string project, Option<string> condition = default(Option<string>))
      => new CsProjElement("Import",
                           AsAttributes("Project", project)
                             .Concat(ToConditionAttribute(condition)),
                           Enumerable.Empty<CsProjElement>());

    public static CsProjElement PropertyGroup(params CsProjElement[] properties)
      => PropertyGroup(Option.None<string>(), properties);

    public static CsProjElement PropertyGroup(Option<string> condition,
                                              params CsProjElement[] properties)
      => new CsProjElement("PropertyGroup",
                           ToConditionAttribute(condition),
                           properties);

    public static CsProjElement Property(string key, string value,
                                         Option<string> condition = default(Option<string>))
      => new CsProjElement(key, ToConditionAttribute(condition), NoElements, value);

    public static IEnumerable<KeyValuePair<string, string>> AsAttributes(string key, string project)
      => new[] {new KeyValuePair<string, string>(key, project)};

    public static IEnumerable<CsProjElement> AsElements(CsProjElement element)
      => new[] {element};

    private static IEnumerable<KeyValuePair<string, string>> ToConditionAttribute(Option<string> condition)
      => condition.Map(c => AsAttributes("Condition", c))
                  .GetOrElse(NoAttributes);

    public static CsProjElement ItemGroup(params CsProjElement[] elements)
      => new CsProjElement("ItemGroup", CsProj.NoAttributes, elements);

    public static CsProjElement Reference(string assemblyName, Option<string> hintPath = default(Option<string>))
      => Item("Reference", assemblyName, hintPath);

    public static CsProjElement Item(string itemName, string includeName, Option<string> hintPath = default(Option<string>))
      => new CsProjElement(itemName,
                           CsProj.AsAttributes("Include", includeName),
                           hintPath.Map(p => CsProj.AsElements(new CsProjElement("HintPath", CsProj.NoAttributes, CsProj.NoElements, p)))
                                   .GetOrElse(CsProj.NoElements));
  }

  public class CsProjElement {
    public string Name { get; }
    public Option<string> Content { get; }
    public IEnumerable<CsProjElement> Children { get; }
    public IEnumerable<KeyValuePair<string, string>> Attributes { get; }

    public CsProjElement(string name,
                         IEnumerable<KeyValuePair<string, string>> attributes,
                         IEnumerable<CsProjElement> children,
                         Option<string> content = default(Option<string>)) {
      Attributes = attributes.ToList();
      Name = name;
      Children = children;
      Content = content;
    }
  }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bud.CsProjTools {
  public static class CsProj {
    public const string Indentation = "  ";
    public static readonly IEnumerable<Tuple<string, string>> NoAttributes = Enumerable.Empty<Tuple<string, string>>();
    public static readonly IEnumerable<CsProjElement> NoElements = Enumerable.Empty<CsProjElement>();

    public static string Generate(params CsProjElement[] elements) {
      var stringBuilder = new StringBuilder();
      stringBuilder.Append(@"<?xml version=""1.0"" encoding=""utf-8""?>").Append('\n');
      stringBuilder.Append(@"<Project ToolsVersion=""14.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">").Append('\n');
      AppendElements(stringBuilder, elements, Indentation);
      stringBuilder.Append(@"</Project>");
      return stringBuilder.ToString();
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

    public static CsProjElement Property(string key,
                                         string value,
                                         Option<string> condition = default(Option<string>))
      => new CsProjElement(key, ToConditionAttribute(condition), NoElements, value);

    public static IEnumerable<Tuple<string, string>> AsAttributes(string key, string project)
      => new[] {new Tuple<string, string>(key, project)};

    public static IEnumerable<CsProjElement> AsElements(params CsProjElement[] elements)
      => elements;

    private static IEnumerable<Tuple<string, string>> ToConditionAttribute(Option<string> condition)
      => condition.Map(c => AsAttributes("Condition", c)).GetOrElse(NoAttributes);

    public static CsProjElement ItemGroup(params CsProjElement[] elements)
      => new CsProjElement("ItemGroup", NoAttributes, elements);

    public static CsProjElement Reference(string assemblyName, Option<string> hintPath = default(Option<string>))
      => Item("Reference", assemblyName, hintPath);

    public static CsProjElement Item(string itemName, string includeName, Option<string> hintPath = default(Option<string>))
      => new CsProjElement(itemName,
                           AsAttributes("Include", includeName),
                           hintPath.Map(p => AsElements(new CsProjElement("HintPath", NoAttributes, NoElements, p)))
                                   .GetOrElse(NoElements));

    private static void AppendElement(StringBuilder stringBuilder, CsProjElement element, string indentation) {
      stringBuilder.Append(indentation).Append($@"<{element.Name}");
      AppendAttributes(stringBuilder, element.Attributes);
      if (element.Content.HasValue || element.Children.Any()) {
        stringBuilder.Append(">");
        AppendChildren(stringBuilder, element, indentation);
        if (element.Content.HasValue) {
          stringBuilder.Append(element.Content.Value);
        }
        stringBuilder.Append($@"</{element.Name}>");
      } else {
        stringBuilder.Append(" />");
      }
    }

    private static void AppendAttributes(StringBuilder stringBuilder,
                                         IEnumerable<Tuple<string, string>> attributes) {
      foreach (var attribute in attributes) {
        stringBuilder.Append($@" {attribute.Item1}=""{attribute.Item2}""");
      }
    }

    private static void AppendChildren(StringBuilder stringBuilder,
                                       CsProjElement element,
                                       string indentation) {
      if (element.Children.Any()) {
        stringBuilder.Append('\n');
        AppendElements(stringBuilder, element.Children, indentation + Indentation);
        stringBuilder.Append(indentation);
      }
    }

    private static void AppendElements(StringBuilder stringBuilder,
                                       IEnumerable<CsProjElement> elements,
                                       string indentation) {
      foreach (var child in elements) {
        AppendElement(stringBuilder, child, indentation);
        stringBuilder.Append('\n');
      }
    }
  }
}
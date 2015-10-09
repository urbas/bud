using System.IO;
using System.Linq;
using Bud.IO;
using static System.IO.Path;
using static Bud.Configs;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);
    public static readonly Key<string> ProjectId = nameof(ProjectId);
    public static readonly Key<IFiles> Sources = nameof(Sources);
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);

    public static Configs Project(string projectDir, string projectId = null)
      => Empty.InitConst(Sources, Files.Empty)
              .InitConst(ProjectDir, projectDir)
              .Init(ProjectId, c => projectId ?? GetFileName(ProjectDir[c]))
              .Init(FilesObservatory, c => new FilesObservatory());

    public static Configs SourceDir(string subDir = null, string fileFilter = "*", SearchOption searchOption = SearchOption.AllDirectories) {
      return Empty.Modify(Sources, (configs, existingSources) => {
        var sourceDir = subDir == null ? ProjectDir[configs] : Combine(ProjectDir[configs], subDir);
        return existingSources.ExtendWith(new FilesInDir(FilesObservatory[configs], sourceDir, fileFilter, searchOption));
      });
    }

    public static Configs SourceFiles(params string[] relativeFilePaths)
      => Empty.Modify(Sources, (configs, existingSources) => {
        var projectDir = ProjectDir[configs];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        return existingSources.ExtendWith(new ListedFiles(FilesObservatory[configs], absolutePaths));
      });
  }
}
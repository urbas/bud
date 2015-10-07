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

    public static Configs Project(string projectDir, string projectId = null, IFilesObservatory filesObservatory = null)
      => NewConfigs.InitConst(Sources, Files.Empty)
                 .InitConst(ProjectId, projectId ?? GetFileName(projectDir))
                 .InitConst(ProjectDir, projectDir)
                 .InitConst(FilesObservatory, filesObservatory ?? new FilesObservatory());

    public static Configs SourceDir(string subDir = null, string fileFilter = "*") {
      return NewConfigs.Modify(Sources, (tasks, existingSources) => {
        var sourceDir = subDir == null ? ProjectDir[tasks] : Combine(ProjectDir[tasks], subDir);
        return existingSources.ExtendWith(FilesObservatory[tasks], sourceDir, fileFilter);
      });
    }

    public static Configs SourceFiles(params string[] relativeFilePaths)
      => NewConfigs.Modify(Sources, (tasks, existingSources) => {
        var projectDir = ProjectDir[tasks];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        return existingSources.ExtendWith(new ListedFiles(FilesObservatory[tasks], absolutePaths));
      });
  }
}
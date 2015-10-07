using System.Collections.Generic;
using System.Linq;
using Bud.IO;
using Bud.Tasking;
using Bud.Tasking.ApiV1;
using static System.IO.Path;
using static Bud.Tasking.ApiV1.Tasks;

namespace Bud {
  public static class Build {
    public static readonly Key<string> ProjectDir = nameof(ProjectDir);
    public static readonly Key<string> ProjectId = nameof(ProjectId);
    public static readonly Key<IFiles> SourceFiles = nameof(SourceFiles);
    public static readonly Key<IFilesObservatory> FilesObservatory = nameof(FilesObservatory);

    public static Tasks Project(string projectDir, string projectId = null, IFilesObservatory filesObservatory = null)
      => NewTasks.InitConst(SourceFiles, Files.Empty)
                 .InitConst(ProjectId, projectId ?? GetFileName(projectDir))
                 .InitConst(ProjectDir, projectDir)
                 .InitConst(FilesObservatory, filesObservatory ?? new FilesObservatory());

    public static Tasks SourcesInSubDir(string subDir = null, string fileFilter = "*") {
      return NewTasks.Modify(SourceFiles, (tasks, existingSources) => {
        var sourceDir = subDir == null ? ProjectDir[tasks] : Combine(ProjectDir[tasks], subDir);
        return existingSources.ExtendWith(FilesObservatory[tasks], sourceDir, fileFilter);
      });
    }

    public static Tasks Sources(params string[] relativeFilePaths)
      => NewTasks.Modify(SourceFiles, (tasks, existingSources) => {
        var projectDir = ProjectDir[tasks];
        var absolutePaths = relativeFilePaths.Select(relativeFilePath => Combine(projectDir, relativeFilePath));
        return existingSources.ExtendWith(new ListedFiles(FilesObservatory[tasks], absolutePaths));
      });
  }
}
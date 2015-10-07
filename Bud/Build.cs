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
    public static readonly Key<IFileSystemObserverFactory> FileSystemObserverFactory = nameof(FileSystemObserverFactory);

    public static Tasks Project(string projectDir, string projectId = null, IFileSystemObserverFactory fileSystemObserverFactory = null)
      => NewTasks.InitConst(SourceFiles, Files.Empty)
                 .InitConst(ProjectId, projectId ?? GetFileName(projectDir))
                 .InitConst(ProjectDir, projectDir)
                 .InitConst(FileSystemObserverFactory, fileSystemObserverFactory ?? new FileSystemObserverFactory());

    public static Tasks SourcesInDir(string subfolder = null, string fileFilter = "*") {
      return NewTasks.Modify(SourceFiles, (tasks, existingSources) => {
        var sourceDir = subfolder == null ? ProjectDir[tasks] : Combine(ProjectDir[tasks], subfolder);
        return existingSources.ExtendWith(FileSystemObserverFactory[tasks], sourceDir, fileFilter);
      });
    }

    public static Tasks Sources(params string[] sourceFiles)
      => NewTasks.Modify(SourceFiles, (tasks, existingSources) => existingSources.ExtendWith(new ListedFiles(FileSystemObserverFactory[tasks], sourceFiles)));
  }
}
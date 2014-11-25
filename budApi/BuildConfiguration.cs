using System;

namespace Bud
{
	public class BuildConfiguration
	{
    public string ProjectBaseDir { private set; get; }

    public BuildConfiguration(string projectBaseDir) {
      ProjectBaseDir = projectBaseDir;
    }
	}
}
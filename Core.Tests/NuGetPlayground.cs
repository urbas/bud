using System;
using System.Linq;
using NuGet;
using System.Collections.Generic;
using NUnit.Framework;

namespace Bud {
  public class NuGetPlayground {
    [Test]
    public void FindPackagesById_MUST_return_the_package_from_the_main_nuget_repository() {
      //ID of the package to be looked up
      string packageID = "EntityFramework";

      //Connect to the official package repository
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");

      //Get the list of all NuGet packages with ID 'EntityFramework'       
      List<IPackage> packages = repo.FindPackagesById(packageID).ToList();

      //Filter the list of packages that are not Release (Stable) versions
      packages = packages.Where(item => (item.IsReleaseVersion() == false)).ToList();

      //Iterate through the list and print the full name of the pre-release packages to console
      foreach (IPackage p in packages) {
        Console.WriteLine(p.GetFullName());
      }
    }


    [Test]
    public void GetPackage_MUST_put_it_into_the_right_place() {
      //ID of the package to be looked up
      string packageID = "EntityFramework";

      //Connect to the official package repository
      IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("http://packages.nuget.org/api/v2");

      //Initialize the package manager
      string path = ".";
      PackageManager packageManager = new PackageManager(repo, path);

      //Download and unzip the package
      packageManager.InstallPackage(packageID, SemanticVersion.Parse("5.0.0"));
    }
  }
}


using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using Newtonsoft.Json;
using NuGet;

namespace Bud.Dependencies {
  public class Package : IDependency, IPackage {
    [JsonProperty(PropertyName = "Version")] private readonly SemanticVersion version;
    public readonly ImmutableList<AssemblyReference> Assemblies;
    [JsonProperty(PropertyName = "Dependencies", NullValueHandling = NullValueHandling.Ignore)] private readonly ImmutableList<PackageDependencyInfo> dependencies;

    [JsonIgnore]
    public PackageVersions HostPackageVersions { get; private set; }

    [JsonIgnore]
    public SemanticVersion Version {
      get { return version; }
    }

    [JsonIgnore]
    public ImmutableList<PackageDependencyInfo> Dependencies {
      get { return dependencies ?? ImmutableList<PackageDependencyInfo>.Empty; }
    }

    [JsonIgnore]
    public string Id {
      get { return HostPackageVersions.Id; }
    }

    [JsonConstructor]
    public Package(SemanticVersion version, IEnumerable<AssemblyReference> assemblies, IEnumerable<PackageDependencyInfo> dependencies) {
      this.version = version;
      Assemblies = assemblies == null ? ImmutableList<AssemblyReference>.Empty : assemblies.Select(assemblyReference => assemblyReference.WithHostPackage(this)).ToImmutableList();
      this.dependencies = dependencies == null || dependencies.IsEmpty() ? null : dependencies.ToImmutableList();
    }

    public Package(IPackage package) :
      this(package.Version,
           package.AssemblyReferences.Select(assemblyReference => new AssemblyReference(assemblyReference)),
           package.DependencySets.SelectMany(dependencySet => dependencySet.Dependencies.Select(dependency => new PackageDependencyInfo(dependency, dependencySet.TargetFramework, dependencySet.SupportedFrameworks)))) {}

    public IPackage AsPackage(IConfig config) => this;

    public override string ToString() => Version != null ? string.Format("{0}@{1}", Id, Version) : Id;

    [JsonIgnore]
    public string Title {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public IEnumerable<string> Authors {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public IEnumerable<string> Owners {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public Uri IconUrl {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public Uri LicenseUrl {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public Uri ProjectUrl {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public bool RequireLicenseAcceptance {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public bool DevelopmentDependency {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public string Description {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public string Summary {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public string ReleaseNotes {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public string Language {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public string Tags {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public string Copyright {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public ICollection<PackageReferenceSet> PackageAssemblyReferences {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public IEnumerable<PackageDependencySet> DependencySets {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public Version MinClientVersion {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public Uri ReportAbuseUrl {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public int DownloadCount {
      get { throw new NotImplementedException(); }
    }

    public IEnumerable<IPackageFile> GetFiles() {
      throw new NotImplementedException();
    }

    public IEnumerable<FrameworkName> GetSupportedFrameworks() {
      throw new NotImplementedException();
    }

    public Stream GetStream() {
      throw new NotImplementedException();
    }

    [JsonIgnore]
    public bool IsAbsoluteLatestVersion {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public bool IsLatestVersion {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public bool Listed {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public DateTimeOffset? Published {
      get { throw new NotImplementedException(); }
    }

    [JsonIgnore]
    public IEnumerable<IPackageAssemblyReference> AssemblyReferences {
      get { return Assemblies; }
    }

    internal Package WithHostPackageVersions(PackageVersions hostPackageVersions) {
      this.HostPackageVersions = hostPackageVersions;
      return this;
    }
  }
}
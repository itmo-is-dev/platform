using Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;
using Microsoft.Build.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace Itmo.Dev.Platform.Options.MSBuild.Tools;

public sealed class CustomAssemblyLoadContext(
    string targetFramework,
    string assemblyPath,
    string[] sharedFrameworkPaths,
    string projectAssetsFilePath,
    TaskLoggingHelper log) : AssemblyLoadContext(isCollectible: true), IDisposable
{
    private readonly AssemblyDependencyResolver _dependencyResolver = new(assemblyPath);
    private readonly ProjectAssetsModel _projectAssets = ProjectAssetsModel.FromFile(projectAssetsFilePath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        log.LogMessage("Loading {0}", assemblyName.FullName);

        return TryLoadFromCurrentDomain(assemblyName)
               ?? TryLoadFromSharedFrameworks(assemblyName)
               ?? TryLoadFromReferencePaths(assemblyName)
               ?? TryLoadFromResolver(assemblyName);
    }

    private Assembly? TryLoadFromCurrentDomain(AssemblyName assemblyName)
    {
        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName()));

        if (assembly is not null)
        {
            log.LogMessage("Loaded from current domain = '{0}'", assembly.GetName());
        }

        return assembly;
    }

    private Assembly? TryLoadFromSharedFrameworks(AssemblyName assemblyName)
    {
        foreach (string sharedFrameworkPath in sharedFrameworkPaths)
        {
            var path = Path.Combine(sharedFrameworkPath, $"{assemblyName.Name}.dll");

            if (File.Exists(path))
            {
                log.LogMessage("Loaded from shared framework = '{0}'", sharedFrameworkPath);
                return CustomLoadFromAssemblyPath(path);
            }
        }

        return null;
    }

    private Assembly? TryLoadFromReferencePaths(AssemblyName assemblyName)
    {
        if (_projectAssets.TryGetAssemblyDescriptor(targetFramework, assemblyName, out var descriptor) is false)
        {
            log.LogMessage("Failed to find descriptor for {0}", assemblyName.Name);
            return null;
        }

        if (_projectAssets.Libraries.TryGetValue(descriptor.LibraryName, out var library) is false)
        {
            log.LogMessage("Failed to find library {0}", descriptor.LibraryName);
            return null;
        }

        if (library.Type is not ProjectAssetsLibraryType.Package)
        {
            log.LogMessage("Invalid library type = {0} for {1}", library.Type, assemblyName);
            return null;
        }

        if (_projectAssets.TryGetPackageDirectoryPath(library.Path, out var libraryFullPath) is false)
        {
            log.LogMessage("Failed to get package directory path for {0}", descriptor.LibraryName);
            return null;
        }

        var candidateAssemblyPath = Path.Combine(libraryFullPath, descriptor.PathInPacakge);
        log.LogMessage("Candidate = {0}", candidateAssemblyPath);

        return File.Exists(candidateAssemblyPath) ? CustomLoadFromAssemblyPath(candidateAssemblyPath) : null;
    }

    private Assembly? TryLoadFromResolver(AssemblyName assemblyName)
    {
        var path = _dependencyResolver.ResolveAssemblyToPath(assemblyName);

        if (string.IsNullOrEmpty(path) is true)
            return null;

        log.LogMessage("From resolver = '{0}'", path);
        return CustomLoadFromAssemblyPath(path);
    }

    private Assembly CustomLoadFromAssemblyPath(string path, [CallerMemberName] string callerName = "")
    {
        try
        {
            return LoadFromAssemblyPath(path);
        }
        catch
        {
            log.LogWarning("Failed to load assembly at '{0}' using method '{1}'", path, callerName);
            throw;
        }
    }

    public void Dispose()
    {
        Unload();
    }
}

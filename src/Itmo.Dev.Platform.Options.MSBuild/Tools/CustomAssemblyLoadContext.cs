using Itmo.Dev.Platform.Options.MSBuild.Extensions;
using Itmo.Dev.Platform.Options.MSBuild.Models.ProjectAssets;
using Microsoft.Build.Utilities;
using System.Collections.Concurrent;
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

    private readonly ConcurrentDictionary<string, Assembly> _assemblyCache = [];

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_assemblyCache.TryGetValue(assemblyName.FullName, out var assembly))
            return assembly;

        using var _ = log.UseDebugScope(nameof(CustomAssemblyLoadContext));

        log.LogDebugMessage("Loading {0}", assemblyName.FullName);

        assembly = TryLoadFromCurrentDomain(assemblyName)
                   ?? TryLoadFromSharedFrameworks(assemblyName)
                   ?? TryLoadFromReferencePaths(assemblyName)
                   ?? TryLoadFromResolver(assemblyName);

        if (assembly is not null)
        {
            _assemblyCache[assemblyName.FullName] = assembly;
        }

        return assembly;
    }

    private Assembly? TryLoadFromCurrentDomain(AssemblyName assemblyName)
    {
        using var _ = log.UseDebugScope(nameof(TryLoadFromCurrentDomain));

        var assembly = AppDomain.CurrentDomain
            .GetAssemblies()
            .FirstOrDefault(assembly => AssemblyName.ReferenceMatchesDefinition(assemblyName, assembly.GetName()));

        if (assembly is not null)
        {
            log.LogDebugMessage("Loaded from current domain = '{0}'", assembly.GetName());
        }

        return assembly;
    }

    private Assembly? TryLoadFromSharedFrameworks(AssemblyName assemblyName)
    {
        using var _ = log.UseDebugScope(nameof(TryLoadFromSharedFrameworks));

        foreach (string sharedFrameworkPath in sharedFrameworkPaths)
        {
            var path = Path.Combine(sharedFrameworkPath, $"{assemblyName.Name}.dll");

            if (File.Exists(path))
            {
                log.LogDebugMessage("Loaded from shared framework = '{0}'", sharedFrameworkPath);
                return CustomLoadFromAssemblyPath(path);
            }
        }

        return null;
    }

    private Assembly? TryLoadFromReferencePaths(AssemblyName assemblyName)
    {
        using var _ = log.UseDebugScope(nameof(TryLoadFromReferencePaths));

        if (_projectAssets.TryGetAssemblyDescriptor(targetFramework, assemblyName, out var descriptor) is false)
        {
            log.LogDebugMessage("Failed to find descriptor for {0}", assemblyName.Name);
            return null;
        }

        if (_projectAssets.Libraries.TryGetValue(descriptor.LibraryName, out var library) is false)
        {
            log.LogDebugMessage("Failed to find library {0}", descriptor.LibraryName);
            return null;
        }

        if (library.Type is not ProjectAssetsLibraryType.Package)
        {
            return null;
        }

        if (_projectAssets.TryGetPackageDirectoryPath(library.Path, out var libraryFullPath) is false)
        {
            log.LogDebugMessage("Failed to get package directory path for {0}", descriptor.LibraryName);
            return null;
        }

        var candidateAssemblyPath = Path.Combine(libraryFullPath, descriptor.PathInPacakge);
        log.LogDebugMessage("Candidate = {0}", candidateAssemblyPath);

        return File.Exists(candidateAssemblyPath) ? CustomLoadFromAssemblyPath(candidateAssemblyPath) : null;
    }

    private Assembly? TryLoadFromResolver(AssemblyName assemblyName)
    {
        using var _ = log.UseDebugScope(nameof(TryLoadFromResolver));

        var path = _dependencyResolver.ResolveAssemblyToPath(assemblyName);

        if (string.IsNullOrEmpty(path) is true)
            return null;

        log.LogDebugMessage("From resolver = '{0}'", path);
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

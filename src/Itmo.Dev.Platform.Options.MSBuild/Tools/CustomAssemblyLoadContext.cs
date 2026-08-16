using Microsoft.Build.Utilities;
using System.Reflection;
using System.Runtime.Loader;

namespace Itmo.Dev.Platform.Options.MSBuild.Tools;

public sealed class CustomAssemblyLoadContext(
    string assemblyPath,
    string[] sharedFrameworkPaths,
    string[] referencePaths,
    TaskLoggingHelper log) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _dependencyResolver = new(assemblyPath);

    private readonly IReadOnlyDictionary<string, string> _referencePaths = referencePaths
        .GroupBy(Path.GetFileNameWithoutExtension, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(grouping => grouping.Key ?? string.Empty, grouping => grouping.First());

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
                return LoadFromAssemblyPath(path);
            }
        }

        return null;
    }

    private Assembly? TryLoadFromReferencePaths(AssemblyName assemblyName)
    {
        if (_referencePaths.TryGetValue(assemblyName.Name ?? string.Empty, out var path) is false)
            return null;

        log.LogMessage("Loaded from reference paths = '{0}'", path);
        return LoadFromAssemblyPath(path);
    }

    private Assembly? TryLoadFromResolver(AssemblyName assemblyName)
    {
        var path = _dependencyResolver.ResolveAssemblyToPath(assemblyName);

        if (string.IsNullOrEmpty(path) is true)
            return null;

        log.LogMessage("From resolver = '{0}'", path);
        return LoadFromAssemblyPath(path);
    }
}

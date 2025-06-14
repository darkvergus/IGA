using System.Reflection;
using System.Runtime.Loader;

namespace Host.Core;

internal sealed class PluginLoadContext(string pluginPath) : AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver resolver = new(pluginPath);

    protected override Assembly? Load(AssemblyName name)
    {
        string? path = resolver.ResolveAssemblyToPath(name);
        return path is null ? null : LoadFromAssemblyPath(path);
    }
}
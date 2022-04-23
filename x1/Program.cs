using System.Reflection;
using System.Runtime.Loader;

public class Program
{
    public static Type type;
    public static void Load()
    {
        Console.WriteLine($"Runtime: {Environment.Version}");

        var dll = Path.Combine(AppContext.BaseDirectory, "Plugin/x2.dll");

        Console.WriteLine("+++ LOAD");
        var ctx = new PluginLoadContext(dll, isCollectible: true);
        type = ctx.LoadFromAssemblyPath(dll).GetType("x2.Class1")!;
        Console.WriteLine("--- LOAD");
        ctx = null;
        DoGC("1");
    }
    public static void Init()
    {
        DoGC("3");
        Task.Run(() =>
        {
            Console.WriteLine("+++ INIT");
            var obj = Activator.CreateInstance(type)!;
            Console.WriteLine("--- INIT");
            GC.KeepAlive(obj);
        }).Wait();
    }
    public static void DoGC(string tag)
    {
        Console.WriteLine("+++ GC " + tag);
        for (int i = 0; i < 10; ++i)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            GC.WaitForPendingFinalizers();
        }
        Console.WriteLine("--- GC " + tag);
    }
    public static void Main()
    {
        Load();
        DoGC("2");
        Init();
    }
    private class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath, bool isCollectible)
            : base(Path.GetFileNameWithoutExtension(pluginPath), isCollectible)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
            this.Unloading += e =>
            {
                Console.WriteLine("PluginLoadContext is unloaded");
            };
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                Console.WriteLine($"loading '{assemblyName.Name}' to plugin");
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
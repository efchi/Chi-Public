using System.Reflection;

namespace Chi
{
    public static class Paths
    {
        public static readonly string SourcesPath =
            Path.Combine(
                Directory.GetParent(                            // sln
                Directory.GetParent(                            // bin
                Directory.GetParent(                            // Debug
                Directory.GetParent(                            // net6.0
                    Assembly.GetEntryAssembly()!.Location       // .dll
                )!.FullName)!.FullName)!.FullName)!.FullName,
                ".sources");

        public static readonly string StartupPath = 
            Path.Combine(SourcesPath, ".startup");

        public static readonly string TestPath =
            Path.Combine(SourcesPath, ".test");
    }
}

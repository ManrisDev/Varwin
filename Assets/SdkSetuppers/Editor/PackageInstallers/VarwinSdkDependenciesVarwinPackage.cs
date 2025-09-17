#if UNITY_EDITOR
using System.IO;

namespace Varwin.PackageInstallers
{
    public class VarwinSdkDependenciesVarwinPackage : IVarwinPackage
    {
        public static string PackageName = "com.varwin.sdk";

        public string PackageIdentified => $"{PackageName}:1.0.0";

        public bool IsPackageInstalled(string manifest, string packagesLock)
        {
            var folderPath = Path.Combine(PackagesInstaller.ProjectPackages, PackageName);
            return Directory.Exists(folderPath) && File.Exists(Path.Combine(folderPath, "package.json"));
        }
    }
}
#endif
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Varwin.PackageInstallers;

public static class PackagesInstaller
{
    private static string _path;
    public static string ProjectPath => _path ??= new DirectoryInfo(Application.dataPath).Parent?.FullName.Replace("\\", "/");
    public static string ProjectPackages => $"{ProjectPath}/Packages";
    public static string LocalPackages => $"{ProjectPath}/Assets/SdkSetuppers/Packages";
    public static bool InstallRequired => InstallRequiredPackagesList.Count > 0;

    public static bool IsProcessing;

    private static AddRequest _currentAddRequest;
    private static IVarwinPackage _current;
    private static string _packageManifest;
    private static string _packageLock;

    private static List<IVarwinPackage> InstallRequiredPackagesList
    {
        get
        {
            UpdateRequiredPackages();
            return _installRequiredPackagesList;
        }
        set => _installRequiredPackagesList = value;
    }

    public static List<IVarwinPackage> SdkRequiredPackages = new()
    {
        new VarwinSdkDependenciesVarwinPackage(),
    };

    private static List<IVarwinPackage> _installRequiredPackagesList;

    private static void MoveLocalPackagesToProjectFolder()
    {
        if (!Directory.Exists(LocalPackages))
        {
            Debug.LogError("Packages folder doesn't exists. Check that Varwin SDK installed correctly.");
            return;
        }

        var files = Directory.GetFiles(LocalPackages, "*.*", SearchOption.AllDirectories);

        try
        {
            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".meta")
                {
                    continue;
                }

                var packageFolderRelativePath = Path.GetRelativePath(LocalPackages, file);
                var copyToPath = Path.Combine(ProjectPackages, packageFolderRelativePath);

                if (File.Exists(copyToPath))
                {
                    File.Delete(copyToPath);
                }

                var directoryName = Path.GetDirectoryName(copyToPath);
                if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

                File.Move(file, copyToPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    public static void UpdateRequiredPackages()
    {
        UpdatePackagesInfo();
        InstallRequiredPackagesList = SdkRequiredPackages.Where(x => !x.IsPackageInstalled(_packageManifest, _packageLock)).ToList();
    }

    public static async void EmbedInstallAllPackages()
    {
        if (!InstallRequired || IsProcessing)
            return;

        try
        {
            IsProcessing = true;

            EditorApplication.LockReloadAssemblies();
            MoveLocalPackagesToProjectFolder();
            UpdatePackagesInfo();

            var openXR = new VarwinOpenXRPackage();
            bool openXrInstallRequired = openXR.IsPackageIncluded() && !openXR.IsPackageInstalled(_packageManifest, _packageLock);

            if (openXrInstallRequired)
            {
                if (SdkRequiredPackages.FirstOrDefault(x => x.GetType() == typeof(VarwinOpenXRPackage)) == null)
                {
                    SdkRequiredPackages.Add(new VarwinOpenXRPackage());
                }

                var openXrRequest = Client.Add(openXR.PackageIdentified);

                while (!openXrRequest.IsCompleted)
                {
                    await Task.Yield();
                }

                if (openXrRequest.Status != StatusCode.Success)
                {
                    Debug.LogError($"Can't install package {openXR.PackageIdentified} with error {openXrRequest.Error.message}");
                }

                openXR.AfterSetupAction();
            }

            Client.Resolve();
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            throw;
        }
        finally
        {
            IsProcessing = false;
            EditorApplication.UnlockReloadAssemblies();
        }
    }

    public static (string packageManifes, string packageLock) GetInstalledPackages()
    {
        UpdatePackagesInfo();
        return (_packageManifest, _packageLock);
    }

    private static void UpdatePackagesInfo()
    {
        var packagesPath = Path.Combine(new DirectoryInfo(Application.dataPath).Parent?.FullName.Replace("\\", "/"), "Packages");

        _packageManifest = File.ReadAllText(Path.Combine(packagesPath, "manifest.json"));
        _packageLock = File.ReadAllText(Path.Combine(packagesPath, "packages-lock.json"));
    }
}
#endif
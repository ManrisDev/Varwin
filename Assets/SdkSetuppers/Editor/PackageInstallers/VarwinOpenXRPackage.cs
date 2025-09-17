#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class VarwinOpenXRPackage : IVarwinPackage
{
    private const string VarwinOpenXRGlobalDefine = "VARWIN_OPENXR";
    private const string VarwinOpenXRPackageIdentifier = "com.varwin.xr.openxr";
    private const string VarwinOpenXRPackageInstallIdentifier = "file:com.varwin.xr.openxr.tar.gz";

    private static AddRequest _addRequest;
    private static ListRequest _listRequest;
    public string PackageIdentified => VarwinOpenXRPackageInstallIdentifier;

    private void SetupGlobalDefine()
    {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        
        if(!defines.Contains(VarwinOpenXRGlobalDefine))
        {
            defines += $"; {VarwinOpenXRGlobalDefine}";
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
        }
    }

    public void AfterSetupAction() => SetupGlobalDefine();

    public bool IsPackageInstalled(string manifest, string packagesLock) => manifest.Contains(VarwinOpenXRPackageIdentifier);

    public bool IsPackageIncluded()
    {
        bool fileExists = File.Exists(Path.Combine(PackagesInstaller.ProjectPackages, "com.varwin.xr.openxr.tar.gz"));
        Debug.Log($"Open xr package included at path {Path.Combine(PackagesInstaller.ProjectPackages, "com.varwin.xr.openxr.tar.gz")}: {fileExists}");

        return fileExists;
    }
}
#endif
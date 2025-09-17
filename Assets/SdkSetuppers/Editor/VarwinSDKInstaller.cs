#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin
{
    public static class VarwinSDKInstaller
    {
        public static bool IsProcessing;
        private static readonly string InternalSdkPackagePath = "Assets/InternalPackage/VarwinSDK.unitypackage";
        private static bool _installFailed;

        public static void InstallSDK()
        {
            if (!Installed() && !_installFailed)
            {
                try
                {
                    IsProcessing = true;
                    AssetDatabase.ImportPackage(InternalSdkPackagePath, true);
                    SubscribeMethods();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw;
                }
            }
        }

        private static void SubscribeMethods()
        {
            AssetDatabase.importPackageCompleted += OnPackageImported;
            AssetDatabase.importPackageCancelled += OnPackageImportCancelled;
            AssetDatabase.importPackageFailed += OnPackageImportFailed;
        }

        private static void UnsubscribeMethods()
        {
            AssetDatabase.importPackageCompleted -= OnPackageImported;
            AssetDatabase.importPackageCancelled -= OnPackageImportCancelled;
            AssetDatabase.importPackageFailed -= OnPackageImportFailed;
        }

        private static void OnPackageImported(string packageName)
        {
            IsProcessing = false;
            File.Delete(Path.Combine(PackagesInstaller.ProjectPath, InternalSdkPackagePath));
            UnsubscribeMethods();
        }

        private static void OnPackageImportCancelled(string packageName)
        {
            IsProcessing = false;

            Debug.LogError($"Import Internal package cancelled. Please install the varwin sdk package located at {InternalSdkPackagePath} manually");

            _installFailed = true;
            UnsubscribeMethods();
        }

        private static void OnPackageImportFailed(string packageName, string errormessage)
        {
            IsProcessing = false;

            Debug.LogError($"Import Internal package failed with error message {errormessage}.\n" +
                           $"Please install the varwin sdk package located at {InternalSdkPackagePath} manually");

            _installFailed = true;
            UnsubscribeMethods();
        }

        public static bool Installed() => Directory.Exists(Path.Combine(PackagesInstaller.ProjectPath, "Assets/Varwin"));
    }   
}
#endif
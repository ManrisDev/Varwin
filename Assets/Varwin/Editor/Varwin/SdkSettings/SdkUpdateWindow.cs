using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Varwin.Data;
using Version = System.Version;

namespace Varwin.Editor
{
    [InitializeOnLoad]
    public class SdkUpdateWindow : EditorWindow
    {
        private const string SessionStateKey = "Varwin.SDK.SdkUpdateWindow.Shown";
        
        private static string RemoteVersion;
        private static bool IsUpdateAvailable;
        private static bool IsForceOpen;
        private static UnityWebRequest RemoteRequest;
        
        private const string DisableUpdateAutoCheckPrefsKey = "DisableSdkUpdateAutoCheck";
        private const string RemoteVersionJsonPath = "https://dist.varwin.com/releases/latest/info.json";
        private const string DownloadLink = "https://dist.varwin.com/releases/latest/VarwinSDK.unitypackage";

        private static string LocalVersion => VarwinVersionInfo.VersionNumber;
        private static string StoredErrorText;
       
        private static bool IsDisableAutoCheck
        {
            get
            {
                if (!EditorPrefs.HasKey(DisableUpdateAutoCheckPrefsKey))
                {
                    EditorPrefs.SetBool(DisableUpdateAutoCheckPrefsKey, false);
                }
                
                return EditorPrefs.GetBool(DisableUpdateAutoCheckPrefsKey);
            }
            set
            {
                EditorPrefs.SetBool(DisableUpdateAutoCheckPrefsKey, value);
            }
        }

        private void Update()
        {
            if (string.IsNullOrEmpty(RemoteVersion))
            {
                GetRemoteVersion();  
            }
        }

        private void OnGUI()
        {
            if (IsUpdateAvailable)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(GetUpdateAvailableMessage(), MessageType.Warning);                    
                EditorGUILayout.HelpBox(SdkTexts.SdkDownloadHelpMessage, MessageType.Info);
                
                EditorGUILayout.Space();
                if (GUILayout.Button(SdkTexts.DownloadSdkButton))
                {
                    Application.OpenURL(DownloadLink);
                }
            }
            else
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(GetNotNeedUpdateMessage(), MessageType.Info);
            }
            
            EditorGUILayout.Space();
            IsDisableAutoCheck = EditorGUILayout.ToggleLeft(SdkTexts.DisableAutoCheckToggle, IsDisableAutoCheck);
        }

        private void OnDestroy()
        {
            IsForceOpen = false;
        }

        private static void OnEditorUpdate()
        {
            bool shown = SessionState.GetBool(SessionStateKey, false);
            if (shown && !IsForceOpen)
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }

            if (EditorApplication.isPlaying)
            {
                return;
            }
            
            if (!IsForceOpen && IsDisableAutoCheck)
            {
                EditorApplication.update -= OnEditorUpdate;
                return;
            }
            
            GetRemoteVersion();

            if (IsUpdateAvailable && !shown || IsForceOpen)
            {
                IsForceOpen = false;
                SessionState.SetBool(SessionStateKey, true);
                var window = GetWindow<SdkUpdateWindow>(SdkTexts.SdkUpdateWindowTitle, true);
                window.minSize = new Vector2(350, 190);
                window.maxSize = new Vector2(350, 190);
            }
        }

        private static void GetRemoteVersion()
        {
            IsUpdateAvailable = false;

            if (RemoteRequest == null)
            {
                RemoteRequest = UnityWebRequest.Get(RemoteVersionJsonPath);
                RemoteRequest.SendWebRequest();
            }
            
            if (!RemoteRequest.isDone)
            {
                return;
            }
            
            if (!RemoteRequest.isNetworkError)
            {
                try
                {
                    var versionObj = RemoteRequest.downloadHandler.text.JsonDeserialize<RemoteVersionInfo>();
                    if (versionObj != null)
                    {
                        if (!string.IsNullOrWhiteSpace(versionObj.version))
                        {
                            RemoteVersion = versionObj.version;
                        }
                    }
                }
                catch (Exception e)
                {
                    if (StoredErrorText != RemoteRequest.downloadHandler.text)
                    {
                        Debug.LogWarning(e.Message);
                        StoredErrorText = RemoteRequest.downloadHandler.text;
                    }
                }
            }
            else
            {
                RemoteVersion = "Error!";
            }
            
            if (Version.TryParse(LocalVersion, out Version localVersion))
            {
                if (Version.TryParse(RemoteVersion, out Version remoteVersion))
                {
                    IsUpdateAvailable = localVersion.CompareTo(remoteVersion) < 0;
                }
            }
        }
        
        private static string GetUpdateAvailableMessage()
        {
            return $"{SdkTexts.UpdateAvailableMessage}\n{GetVersionsText()}";
        }

        private static string GetNotNeedUpdateMessage()
        {
            return $"{SdkTexts.NotNeedUpdateMessage}\n{GetVersionsText()}";
        }

        private static string GetVersionsText()
        {
            string local = !string.IsNullOrWhiteSpace(LocalVersion) ? LocalVersion : RemoteVersion;
            string remote = !string.IsNullOrWhiteSpace(RemoteVersion) ? RemoteVersion : "waiting...";
            return string.Format(SdkTexts.VersionsFormat, local, remote);
        }
        
        static SdkUpdateWindow()
        {
            IsForceOpen = false;
            EditorApplication.update += OnEditorUpdate;            
        }

        public static void OpenWindow()
        {
            IsForceOpen = true;
            EditorApplication.update += OnEditorUpdate;
            StoredErrorText = null;
        }

        private class RemoteVersionInfo : IJsonSerializable
        {
            public string version;
        }        
    }
}
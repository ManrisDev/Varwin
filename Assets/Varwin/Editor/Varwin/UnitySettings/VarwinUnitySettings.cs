using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if VARWIN_OPENXR
using Varwin.OpenXR;
#endif

namespace Varwin.Editor
{
    public static class VarwinUnitySettings
    {
        public const string AcceptingRecommendedSettingsKey = "Varwin.Settings.AcceptingRecommendedSettings";
        public const string Ignore = "Varwin.ignore.";
        public const string UseRecommended = "Use recommended ({0})";
        public const string CurrentValue = " (current = {0})";
        private const long AndroidAPIValue = 33554432;
        
        public static string Defines
        {
            get => PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
            set => PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, value);
        }

        public readonly static List<UnitySettingsOption> Options = new List<UnitySettingsOption>()
        {
            // Android 
            
            new UnitySettingsOption(
                "Build Target",
                () => EditorUserBuildSettings.activeBuildTarget,
                x => EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, x),
#if UNITY_EDITOR_WIN
                BuildTarget.StandaloneWindows64),
#else
                BuildTarget.StandaloneLinux64),
#endif                
            
            new UnitySettingsOption(
                "Android Texture Compression",
                () => EditorUserBuildSettings.androidBuildSubtarget,
                x => EditorUserBuildSettings.androidBuildSubtarget = x,
                MobileTextureSubtarget.Generic),
            
            // Rendering Settings
            
            new UnitySettingsOption(
                "Display Resolution Dialog",
                () => PlayerSettings.displayResolutionDialog,
                x => PlayerSettings.displayResolutionDialog = x,
                ResolutionDialogSetting.Disabled),
            
            new UnitySettingsOption(
                "GPU skinning",
                () => PlayerSettings.gpuSkinning,
                x => PlayerSettings.gpuSkinning = x,
                true),
            
            new UnitySettingsOption(
                "Visible in background",
                () => PlayerSettings.visibleInBackground,
                x => PlayerSettings.visibleInBackground = x,
                true),
            
            new UnitySettingsOption(
                "Color Space (requires reloading scene)",
                () => PlayerSettings.colorSpace,
                x => PlayerSettings.colorSpace = x,
                ColorSpace.Linear),

            // Other Settings
            
            new UnitySettingsOption(
                "API Compatibility Level (Standalone)",
                () => PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone),
                x => PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, x),
                ApiCompatibilityLevel.NET_4_6),

            new UnitySettingsOption(
                "API Compatibility Level (Android)",
                () => PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Android),
                x => PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, x),
                ApiCompatibilityLevel.NET_4_6),
            
            new UnitySettingsOption(
                "Scripting Runtime Version",
                () => PlayerSettings.scriptingRuntimeVersion,
                x => PlayerSettings.scriptingRuntimeVersion = x,
                ScriptingRuntimeVersion.Latest),
            
            new UnitySettingsOption(
                "Dynamic Batching",
                () => GetDynamicBatching(),
                x => SetDynamicBatching(x),
                true),
            
            new UnitySettingsOption(
                "Strip Unused Mesh Components",
                () => PlayerSettings.stripUnusedMeshComponents,
                x => PlayerSettings.stripUnusedMeshComponents = x,
                false),
                
            new UnitySettingsOption(
                "Legacy Clamp Blend Shape Weights",
                () => GetPlayerSettingProperty("legacyClampBlendShapeWeights").boolValue,
                x => GetPlayerSettingProperty("legacyClampBlendShapeWeights").boolValue = x,
                true),
            
            new UnitySettingsOption(
                "Xbox One GPU Variability",
                () => GetPlayerSettingProperty("XboxOneEnableGPUVariability").boolValue,
                x => GetPlayerSettingProperty("XboxOneEnableGPUVariability").boolValue = x,
                false),
            
            new UnitySettingsOption(
                "Mobile MT Rendering",
                () => PlayerSettings.MTRendering,
                x => PlayerSettings.MTRendering = x,
                false),
            
            new UnitySettingsOption(
                "Android VR Settings",
                () => GetAndroidVrSettings(),
                x => SetupAndroidVrSettings(),
                true),
            
            new UnitySettingsOption(
                "Android Graphics API",
                () => GetAndroidBuildTargetGraphicsApi(),
                x => SetAndroidBuildTargetGraphicsApi(),
                true),
            
            new UnitySettingsOption(
                "Vertex Channel Compression Mask",
                () =>  GetPlayerSettingProperty("VertexChannelCompressionMask").intValue,
                x => GetPlayerSettingProperty("VertexChannelCompressionMask").intValue = x,
                214),
            
            new UnitySettingsOption(
                "iOS Render Extra Frame On Pause",
                () =>  GetPlayerSettingProperty("iOSRenderExtraFrameOnPause").boolValue,
                x => GetPlayerSettingProperty("iOSRenderExtraFrameOnPause").boolValue = x,
                true),
            
            // XR Settings
            new UnitySettingsOption(
                "Virtual Reality Supported",
                () => PlayerSettings.virtualRealitySupported,
                x =>
                {
                    PlayerSettings.virtualRealitySupported = x;
                    UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(BuildTargetGroup.Standalone, new string[]{ "None", "Varwin OpenXR"});
                },
                true),            
            new UnitySettingsOption(
                "360 Stereo Capture",
                () => PlayerSettings.enable360StereoCapture,
                x => PlayerSettings.enable360StereoCapture = x,
                false),
#if VARWIN_OPENXR
            new UnitySettingsOption(
                "Use all OpenXR Interaction profiles",
                () => IsUsedAllOpenXRProfiles(),
                x => SetUsedAllOpenXRProfiles(x),
                true),
            new UnitySettingsOption(
                "Use multipass in VR",
                () => IsMultipassVR(),
                x => SetMultipassVR(x),
                true),
#endif
        };
        
        public static readonly Dictionary<int, string> Tags = new Dictionary<int, string>()
        {
            {0, "TeleportArea"},
            {1, "NotTeleport"},
            {2, "Beam"},
            {3, "Effects"}
        };

        public static readonly Dictionary<int, string> Layers = new Dictionary<int, string>()
        {
            {8, "Rotators"},
            {9, "VRControllers"},
            {10, "Location"},
            {11, "LiquidContainer"},
            {12, "LiquidEraser"},
            {13, "Zones"},
            {14, "PostProcessing"},
        };

        public static readonly string[] AdditionalExtensionsToInclude = {"txt", "xml", "fnt", "cd", "asmdef", "rsp"};

        private static SerializedObject _playerSettingsObject;
        private static SerializedObject _physicsManagerObject;
        private static SerializedObject _tagManagerObject;
        private static byte[] _requiredGraphicsSettings;
        private static byte[] _originalGraphicsSettings;

        private static SerializedObject PlayerSettingsObject
        {
            get
            {
                if (_playerSettingsObject == null)
                {
                    _playerSettingsObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
                }
                
                return _playerSettingsObject;
            }
        }

        private static SerializedObject PhysicsManagerObject
        {
            get
            {
                if (_physicsManagerObject == null)
                {
                    _physicsManagerObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/DynamicsManager.asset")[0]);
                }
                
                return _physicsManagerObject;
            }
        }

#if VARWIN_OPENXR
        private static Type[] _openXRInteractionProfiles = new[]
        {
            typeof(GoogleDaydreamControllerProfile),
            typeof(MicrosoftMixedRealityMotionControllerProfile),
            typeof(OculusTouchControllerProfile),
            typeof(ValveIndexControllerProfile),
            typeof(HTCViveControllerProfile),
            typeof(KhronosSimpleControllerProfile)
        };
#endif
        
        private static SerializedObject TagManagerObject
        {
            get
            {
                if (_tagManagerObject == null)
                {
                    _tagManagerObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                }
                
                return _tagManagerObject;
            }
        }
        
        private static byte[] RequiredGraphicsSettings
        {
            get
            {
                if (_requiredGraphicsSettings == null)
                {
                    _requiredGraphicsSettings = File.ReadAllBytes("Assets/Varwin/Editor/Varwin/UnitySettings/GraphicsSettings.asset");
                }

                return _requiredGraphicsSettings;
            }
        }

        private static byte[] OriginalGraphicsSettings
        {
            get
            {
                if (_originalGraphicsSettings == null)
                {
                    _originalGraphicsSettings = File.ReadAllBytes("ProjectSettings/GraphicsSettings.asset");
                }

                return _originalGraphicsSettings;
            }
        }

        private static SerializedProperty TagsProperty => TagManagerObject.FindProperty("tags");

        private static SerializedProperty LayersProperty => TagManagerObject.FindProperty("layers");

        private static SerializedProperty BatchingProperty => PlayerSettingsObject.FindProperty("m_BuildTargetBatching");

        public static bool TagsAreValid()
        {
            foreach (var tag in Tags)
            {
                if (tag.Key < TagsProperty.arraySize)
                {
                    SerializedProperty t = TagsProperty.GetArrayElementAtIndex(tag.Key);
                    if (!t.stringValue.Equals(tag.Value))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void SetupTags()
        {
            if (TagsAreValid())
            {
                return;
            }

            foreach (var tag in Tags)
            {
                while (tag.Key >= TagsProperty.arraySize)
                {
                    TagsProperty.InsertArrayElementAtIndex(tag.Key);
                }
                SerializedProperty n = TagsProperty.GetArrayElementAtIndex(tag.Key);
                n.stringValue = tag.Value;
            }
        }

        public static string GetLayer(int layerIndex)
        {
            SerializedProperty sp = LayersProperty.GetArrayElementAtIndex(layerIndex);
            return sp?.stringValue;
        }

        public static void SetLayer(int layerIndex, string layerName)
        {
            SerializedProperty sp = LayersProperty.GetArrayElementAtIndex(layerIndex);
            if (sp != null)
            {
                sp.stringValue = layerName;
            }
        }

        public static void Save()
        {
            TagManagerObject.ApplyModifiedProperties();
            PlayerSettingsObject.ApplyModifiedProperties();
            PhysicsManagerObject.ApplyModifiedProperties();
        }

        public static bool GetDynamicBatching()
        {
            for (int i = 0; i < BatchingProperty.arraySize; ++i)
            {
                SerializedProperty batching = BatchingProperty.GetArrayElementAtIndex(i);
                SerializedProperty dynamicBatching = batching.FindPropertyRelative("m_DynamicBatching");
                if (!dynamicBatching.boolValue)
                {
                    return false;
                }
            }

            return true;
        }
        
        public static void SetDynamicBatching(bool value)
        {
            for (int i = 0; i < BatchingProperty.arraySize; ++i)
            {
                SerializedProperty batching = BatchingProperty.GetArrayElementAtIndex(i);
                SerializedProperty dynamicBatching = batching.FindPropertyRelative("m_DynamicBatching");
                dynamicBatching.boolValue = value;
            }
        }

        public static SerializedProperty GetPlayerSettingProperty(string name)
        {
            return PlayerSettingsObject.FindProperty(name);
        }

        public static bool GetAndroidVrSettings()
        {
             SerializedProperty buildTargetVrSettings = GetPlayerSettingProperty("m_BuildTargetVRSettings");
             for (int i = 0; i < buildTargetVrSettings.arraySize; ++i)
             {
                 SerializedProperty element = buildTargetVrSettings.GetArrayElementAtIndex(i);
                 SerializedProperty buildTarget = element.FindPropertyRelative("m_BuildTarget");

                 if (buildTarget.stringValue == "Android")
                 {
                     SerializedProperty enabled = element.FindPropertyRelative("m_Enabled");
                     return enabled.boolValue;
                 }
             }

             return false;
        }

        private static bool IsSymbolsDefined(string symbol)
        {
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out var symbols);
            return symbols.Any(a => a == symbol);
        }

        private static void AppendDefineSymbols(string symbol)
        {
            PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out var symbols);
            if (symbols.Any(a => a == symbol))
            {
                return;
            }
            
            var resultSymbols = new List<string>(symbols) {symbol};
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, resultSymbols.ToArray());
        }

#if VARWIN_OPENXR
        private static bool IsUsedAllOpenXRProfiles()
        {
#if UNITY_EDITOR_WIN
            var profileTypeNames = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Windows)?.ProfileTypeNames;
#else
            var profileTypeNames = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Linux)?.ProfileTypeNames;
#endif

            return _openXRInteractionProfiles.All(profileType => profileTypeNames?.Contains(profileType.FullName) ?? false);
        }
        
        private static void SetUsedAllOpenXRProfiles(bool isEnabled)
        {
#if UNITY_EDITOR_WIN
            var settings = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Windows);
#else
            var settings = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Linux);
#endif
            if (!settings)
            {
                Debug.LogError("Settings for XR Subsystem not found!");
                return;
            }
            
            var typeNames = _openXRInteractionProfiles.Select(a => a.FullName);
            if (!isEnabled)
            {
                settings.ProfileTypeNames.RemoveAll(a => typeNames.Contains(a));
            }
            else
            {
                foreach (var typeName in typeNames)
                {
                    if (settings.ProfileTypeNames.Contains(typeName))
                    {
                        continue;
                    }

                    settings.ProfileTypeNames.Add(typeName);
                }
            }
                                
        }
        
        private static bool IsMultipassVR()
        {
#if UNITY_EDITOR_WIN
            var renderMode = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Windows)?.RenderMode;
#else
            var renderMode = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Linux)?.RenderMode;
#endif

            return renderMode == VarwinOpenXRRenderMode.MultiPass;
        }
        
        private static void SetMultipassVR(bool isEnabled)
        {
#if UNITY_EDITOR_WIN
            var settings = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Windows);
#else
            var settings = VarwinOpenXRSettings.Instance?.GetSettings(TargetPlatform.Linux);
#endif
            if (!settings)
            {
                Debug.LogError("Settings for XR Subsystem not found!");
                return;
            }

            settings.RenderMode = isEnabled ? VarwinOpenXRRenderMode.MultiPass : VarwinOpenXRRenderMode.SinglePass;
        }
#endif
        
        public static void SetupAndroidVrSettings()
        {
            int index = -1;
            SerializedProperty buildTargetVrSettings = GetPlayerSettingProperty("m_BuildTargetVRSettings");
            
            for (int i = 0; i < buildTargetVrSettings.arraySize; ++i)
            {
                SerializedProperty element = buildTargetVrSettings.GetArrayElementAtIndex(i);
                SerializedProperty elementBuildTarget = element.FindPropertyRelative("m_BuildTarget");

                if (elementBuildTarget.stringValue == "Android")
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                index = buildTargetVrSettings.arraySize;
                buildTargetVrSettings.arraySize++;
            }
            
            SerializedProperty android = buildTargetVrSettings.GetArrayElementAtIndex(index);
            
            SerializedProperty buildTarget = android.FindPropertyRelative("m_BuildTarget");
            buildTarget.stringValue = "Android";
            
            SerializedProperty enabled = android.FindPropertyRelative("m_Enabled");
            enabled.boolValue = true;

            bool useOpenVr = false;
            SerializedProperty devices = android.FindPropertyRelative("m_Devices");
            for (int i = 0; i < devices.arraySize; ++i)
            {
                if (devices.GetArrayElementAtIndex(i).stringValue == "OpenVR")
                {
                    useOpenVr = true;
                    break;
                }
            }

            if (!useOpenVr)
            {
                devices.arraySize++;
                devices.GetArrayElementAtIndex(devices.arraySize - 1).stringValue = "OpenVR";
            }
        }

        public static bool GetAndroidBuildTargetGraphicsApi()
        {
            SerializedProperty buildTargetVrSettings = GetPlayerSettingProperty("m_BuildTargetGraphicsAPIs");
            for (int i = 0; i < buildTargetVrSettings.arraySize; ++i)
            {
                SerializedProperty element = buildTargetVrSettings.GetArrayElementAtIndex(i);
                SerializedProperty buildTarget = element.FindPropertyRelative("m_BuildTarget");

                if (buildTarget.stringValue == "AndroidPlayer")
                {
                    return false;
                }
            }

            return true;
        }

        public static void SetAndroidBuildTargetGraphicsApi()
        {
            SerializedProperty buildTargetVrSettings = GetPlayerSettingProperty("m_BuildTargetGraphicsAPIs");
            for (int i = 0; i < buildTargetVrSettings.arraySize; ++i)
            {
                SerializedProperty element = buildTargetVrSettings.GetArrayElementAtIndex(i);
                SerializedProperty buildTarget = element.FindPropertyRelative("m_BuildTarget");

                if (buildTarget.stringValue == "AndroidPlayer")
                {
                    buildTarget.stringValue = "LinuxStandaloneSupport";
                    SerializedProperty api = element.FindPropertyRelative("m_APIs");
                    const int low = (int)(AndroidAPIValue & uint.MaxValue);
                    const int high = (int)(AndroidAPIValue >> 32);
                    api.GetArrayElementAtIndex(0).intValue = high;
                    api.GetArrayElementAtIndex(1).intValue = low;
                }
            }
        }

        public static bool IsGraphicsSettingsValid()
        {
            return Enumerable.SequenceEqual(RequiredGraphicsSettings, OriginalGraphicsSettings);
        }

        public static void OverwriteGraphicsSettings()
        {
            File.WriteAllBytes("ProjectSettings/GraphicsSettings.asset",RequiredGraphicsSettings);
            Clear();
        }

        public static void Clear()
        {
            if (_requiredGraphicsSettings != null)
            {
                Array.Clear(_requiredGraphicsSettings, 0, _requiredGraphicsSettings.Length);
                _requiredGraphicsSettings = null;
            }
            if (_originalGraphicsSettings != null)
            {
                Array.Clear(_originalGraphicsSettings, 0, _originalGraphicsSettings.Length);
                _originalGraphicsSettings = null;
            }
        }
    }
}

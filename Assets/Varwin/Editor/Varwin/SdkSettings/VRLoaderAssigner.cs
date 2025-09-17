using UnityEditor;
using System.Linq;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;

namespace Varwin.Editor
{
    public static class VRLoaderAssigner
    {
        private const string OpenXRLoaderName = "Varwin Open XR Loader";

        [InitializeOnEnterPlayMode]
        private static void SetOpenVRLoader()
        {
#if VARWIN_OPENXR            
            var varwinTools = UnityEditor.Editor.FindObjectOfType<VarwinTools>();
            if (!varwinTools)
            {
                return;
            }

            var isVrMode = false;
            var monoBehaviours = varwinTools.GetComponents<MonoBehaviour>();
            var adapterLoader = monoBehaviours.FirstOrDefault(x => x.GetType().Name.Contains("AdapterLoader"));

            if (!adapterLoader)
            {
                return;
            }

            var adapterLoaderType = adapterLoader.GetType();
            var platformModeFieldInfo = adapterLoaderType.GetField("PlatformMode");
            var platformModeValue = (PlatformMode)platformModeFieldInfo.GetValue(adapterLoader);

            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (settings.Manager is null)
            {
                return;
            }

            var loaderAssigned = IsOpenVrLoaderAssigned();
            if (platformModeValue == PlatformMode.Vr && !loaderAssigned)
            {
                XRPackageMetadataStore.AssignLoader(settings.Manager, OpenXRLoaderName, BuildTargetGroup.Standalone);
            }
            else if(platformModeValue != PlatformMode.Vr && loaderAssigned)
            {
                XRPackageMetadataStore.RemoveLoader(settings.Manager, OpenXRLoaderName, BuildTargetGroup.Standalone);
            }
#endif
        }

        private static bool IsOpenVrLoaderAssigned()
        {
            var settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);

            return settings.Manager is not null && settings.Manager.activeLoaders.Any(x => x.name == OpenXRLoaderName);
        }
    }
}
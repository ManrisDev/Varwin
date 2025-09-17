using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Varwin
{
    [UsedImplicitly]
    public static class CameraManager
    {
        private static Camera _desktopPlayerCamera;
        
        private static Camera _desktopEditorCamera;
        
        private static Camera _vrCamera;

        /// <summary>
        /// DesktopPlayerRig Camera
        /// </summary>
        public static Camera DesktopPlayerCamera
        {
            get => _desktopPlayerCamera;
            set
            {
                if (_desktopPlayerCamera)
                {
                    return;
                }
                _desktopPlayerCamera = value;
            }
        }
        
        /// <summary>
        /// Desktop Editor Camera
        /// </summary>
        public static Camera DesktopEditorCamera
        {
            get => _desktopEditorCamera;
            set
            {
                if (_desktopEditorCamera)
                {
                    return;
                }
                _desktopEditorCamera = value;
            }
        }
        
        /// <summary>
        /// STVRPlayer VRCamera 
        /// </summary>
        public static Camera VrCamera
        {
            get => _vrCamera;
            set
            {
                if (_vrCamera)
                {
                    return;
                }
                _vrCamera = value;
            }
        }

        /// <summary>
        /// Current Camera
        /// </summary>
        public static Camera CurrentCamera
        {
            get
            {
                if (ProjectData.PlatformMode == PlatformMode.Desktop)
                {
                    return ProjectData.IsPlayMode ? DesktopPlayerCamera : DesktopEditorCamera;
                }
                return VrCamera;
            }
        }
    }
}
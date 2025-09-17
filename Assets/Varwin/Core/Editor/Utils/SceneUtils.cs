using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class SceneUtils
    {
        public static void MoveCameraToEditorView(Camera camera)
        {
            if (camera != null)
            {
                var sceneViewCamera = SceneView.lastActiveSceneView.camera;

                if (sceneViewCamera != null)
                {
                    camera.transform.position = sceneViewCamera.transform.position;
                    camera.transform.rotation = sceneViewCamera.transform.rotation;
                }
            }
        }
    }
}
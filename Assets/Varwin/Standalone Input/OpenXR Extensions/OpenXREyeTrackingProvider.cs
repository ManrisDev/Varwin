#if VARWIN_OPENXR

using UnityEngine;
using Varwin.XR;

namespace OpenXR.Extensions
{
    public class OpenXREyeTrackingProvider : IEyePoseProvider
    {
        public Component AddComponent(GameObject targetObject, bool isLeft)
        {
            var trackedComponent = targetObject.AddComponent<EyeTrackingPoseDriver>();
            trackedComponent.IsLeft = isLeft;
            return trackedComponent;
        }
    }
}

#endif
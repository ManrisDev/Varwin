using UnityEngine;
using Varwin.DesktopInput;
using Varwin.PlatformAdapter;
using Varwin.Raycasters;

namespace Varwin.DesktopPlayer
{
    public class DesktopPlayerRaycaster : DefaultVarwinRaycaster<DesktopInteractableObject>
    {
        public const float DesktopMinRaycastDistance = 1.3f;

        protected override float ObjectInteractDistance => DistancePointer.CustomSettings
            ? Mathf.Max(DistancePointer.CustomSettings.Distance, DesktopMinRaycastDistance)
            : DesktopMinRaycastDistance;
    }
}
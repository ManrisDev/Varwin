using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Система сдвига камеры относительно пола.
    /// </summary>
    public class VarwinXRCameraOffset : MonoBehaviour
    {
        /// <summary>
        /// Сдвиг от пола.
        /// </summary>
        public float FloorOffset = 1.36144f;
        
        /// <summary>
        /// Объект сдвига камеры.
        /// </summary>
        public Transform CameraOffsetObject;

        /// <summary>
        /// Сдвиг при отсутствии floor.
        /// </summary>
        private void Start()
        {
            List<XRInputSubsystem> inputSubsystems = new();
            var targetCameraOffset = FloorOffset;
            SubsystemManager.GetInstances(inputSubsystems);
            if (inputSubsystems.Count == 0)
            {
                targetCameraOffset = 0;
            }

            foreach (var inputSubsystem in inputSubsystems)
            {
                var trackingOriginModes = inputSubsystem.GetTrackingOriginMode();
                if (trackingOriginModes == TrackingOriginModeFlags.Floor)
                {
                    targetCameraOffset = 0;
                }
            }

            CameraOffsetObject.localPosition = new Vector3(CameraOffsetObject.localPosition.x, targetCameraOffset, CameraOffsetObject.localPosition.z);
        }
    }
}
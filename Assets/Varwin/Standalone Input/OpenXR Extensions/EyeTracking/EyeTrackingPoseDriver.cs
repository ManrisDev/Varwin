using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace OpenXR.Extensions
{
    public class EyeTrackingPoseDriver : MonoBehaviour
    {
        private List<InputDevice> _findedInputDevices = new();
        public bool IsLeft;
        private InputDevice _head;

        private void Start()
        {
            _head = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye);
        }

        private void Update()
        {
            if (_findedInputDevices.Count == 0)
            {
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, _findedInputDevices);
                return;
            }

            var eyeTracker = _findedInputDevices[0];
            if (!eyeTracker.TryGetFeatureValue(CommonUsages.eyesData, out var eyeData))
            {
                return;
            }

            if (!_head.TryGetFeatureValue(CommonUsages.centerEyePosition, out var centerPosition) ||
                !_head.TryGetFeatureValue(CommonUsages.centerEyeRotation, out var centerRotation))
            {
                return;
            }

            var rotation = Quaternion.identity;
            var position = Vector3.zero;

            if (IsLeft && (!eyeData.TryGetLeftEyePosition(out position) || !eyeData.TryGetLeftEyeRotation(out rotation)))
            {
                return;
            }
            
            if (!IsLeft && (!eyeData.TryGetRightEyePosition(out position) || !eyeData.TryGetRightEyeRotation(out rotation)))
            {
                return;
            }

            transform.localRotation = Quaternion.Inverse(centerRotation) * rotation;
            transform.localPosition = Quaternion.Inverse(centerRotation) * (position - centerPosition);
        }
    }
}
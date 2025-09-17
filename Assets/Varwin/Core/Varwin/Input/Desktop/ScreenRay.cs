using UnityEngine;

namespace Varwin
{
    /// <summary>
    /// Rotate transform by ScreenPad.Input (for desktop teleport)
    /// </summary>
    public class ScreenRay : MonoBehaviour
    {
        public float XRotationScale = 40;
        public float YRotationScale = 40;
        private void Update()
        {
            GameObject o = gameObject;
            Quaternion rotation = o.transform.rotation;
            rotation.eulerAngles = new Vector3(-ScreenPadBase.Input.y * YRotationScale, ScreenPadBase.Input.x * XRotationScale, rotation.eulerAngles.z);
            o.transform.localRotation = rotation;
        }
    }
}
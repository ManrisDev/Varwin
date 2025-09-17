using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Контроллер трекинга глаз.
    /// </summary>
    public class EyeTrackingManager : MonoBehaviour
    {
        /// <summary>
        /// Трансформ левого глаза.
        /// </summary>
        public Transform LeftEyeTransform;
        
        /// <summary>
        /// Трансформ правого глаза.
        /// </summary>
        public Transform RightEyeTransform;

        /// <summary>
        /// Добавление управляющих компонентов.
        /// </summary>
        private void Awake()
        {
            if (IEyePoseProvider.Instance == null)
            {
                return;
            }

            IEyePoseProvider.Instance.AddComponent(LeftEyeTransform.gameObject, true);
            IEyePoseProvider.Instance.AddComponent(RightEyeTransform.gameObject, false);
        }
    }
}
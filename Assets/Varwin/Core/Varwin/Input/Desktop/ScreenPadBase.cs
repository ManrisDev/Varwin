using UnityEngine;

namespace Varwin
{
    public abstract class ScreenPadBase : MonoBehaviour
    {
        /// <summary>
        /// Offset relative to the center of the screen (0; 0 - is center of screen)
        /// </summary>
        public static Vector2 Input = Vector2.zero;
    }
}
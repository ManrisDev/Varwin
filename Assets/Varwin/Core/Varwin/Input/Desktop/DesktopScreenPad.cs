using UnityEngine;

namespace Varwin
{
    public class DesktopScreenPad : ScreenPadBase
    {
        private void Update()
        {
            float inputX = MouseInput.MousePosition.x / Screen.width * 2 - 1;
            float inputY = MouseInput.MousePosition.y / Screen.height * 2 - 1;
            Input = new Vector2(inputX, inputY);
        }
    }
}
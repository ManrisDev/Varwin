using UnityEngine;

namespace Varwin
{
    public static class MouseInput
    {
        public static Vector3 MousePosition => Cursor.lockState == CursorLockMode.None ? Input.mousePosition : new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }    
}


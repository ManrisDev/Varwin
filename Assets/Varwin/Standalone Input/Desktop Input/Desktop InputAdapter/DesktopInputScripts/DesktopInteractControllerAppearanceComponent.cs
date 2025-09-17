using UnityEngine;

namespace Varwin.DesktopInput
{
    public class DesktopInteractControllerAppearanceComponent : MonoBehaviour
    {
        public bool hideControllerOnGrab;

        public void Destroy()
        {
            Destroy(this);
        }
    }
}
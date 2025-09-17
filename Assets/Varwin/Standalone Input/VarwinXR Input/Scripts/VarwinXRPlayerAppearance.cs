using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRPlayerAppearance : PlayerAppearance
    {
        public VarwinXRPlayerAppearance()
        {
            ControllerAppearance = new ComponentWrapFactory<InteractControllerAppearance,
                VarwinXRInteractControllerAppearance, VarwinXRInteractControllerAppearanceDUMMY>();
        }

        private class VarwinXRInteractControllerAppearance : InteractControllerAppearance,
            IInitializable<VarwinXRInteractControllerAppearanceDUMMY>
        {
            private VarwinXRInteractControllerAppearanceDUMMY _objectAppearanceDummy;

            public override bool HideControllerOnGrab
            {
                set { }
            }

            public override void DestroyComponent()
            {
                //ToDo if needed...
            }

            public void Init(VarwinXRInteractControllerAppearanceDUMMY interactableObject)
            {
                _objectAppearanceDummy = interactableObject;
            }
        }
    }

    public class VarwinXRInteractControllerAppearanceDUMMY : MonoBehaviour
    {
    }
}
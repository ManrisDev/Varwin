using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopPlayerAppearance : PlayerAppearance
    {
        public DesktopPlayerAppearance()
        {
            ControllerAppearance = new ComponentWrapFactory<InteractControllerAppearance,
                DesktopInteractControllerAppearance,DesktopInteractControllerAppearanceComponent>();
        }

        private class DesktopInteractControllerAppearance : InteractControllerAppearance, IInitializable<DesktopInteractControllerAppearanceComponent >
        {
            private DesktopInteractControllerAppearanceComponent _controllerAppearance;
            public override bool HideControllerOnGrab
            {
                set => _controllerAppearance.hideControllerOnGrab = value;
            }

            public override void DestroyComponent()
            {
                _controllerAppearance.Destroy();
            }
            
            public void Init(DesktopInteractControllerAppearanceComponent monoBehaviour)
            {
                _controllerAppearance = monoBehaviour;
            }
        }
    }
}
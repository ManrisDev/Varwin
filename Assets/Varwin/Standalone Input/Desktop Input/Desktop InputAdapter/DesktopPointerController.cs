using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopPointerController : PointerController
    {
        public DesktopPointerController()
        {
            Managers = 
                new ComponentWrapFactory<PointerManager, DesktopPointerManager, DesktopPointerControllerComponent>();
        }
        
        public override bool IsMenuOpened
        {
            set { }
        }

        public class DesktopPointerManager : PointerManager, IInitializable<DesktopPointerControllerComponent>
        {
            private bool _isMenuOpened;
            private bool _showUiPointer;

            public override bool IsMenuOpened
            {
                get => _isMenuOpened;
                set => _isMenuOpened = false;
            }

            public override bool ShowUIPointer
            {
                get => _showUiPointer;
                set => _showUiPointer = false;
            }
            
            public void Init(DesktopPointerControllerComponent monoBehaviour)
            {
                InputAdapter.Instance.PointerController.SetRightManager(this);
                InputAdapter.Instance.PointerController.SetLeftManager(this);
            }
        }
    }
}

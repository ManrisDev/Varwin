using Varwin.Public;

namespace Varwin
{
    public class PlatformModeSwitchController
    {
        private readonly ISwitchPlatformModeSubscriber _switchMode;
        public PlatformModeSwitchController(ISwitchPlatformModeSubscriber switchPlatformModeSubscriber)
        {
            _switchMode = switchPlatformModeSubscriber;
        }

        public void SwitchPlatformMode(PlatformMode newPlatformMode, PlatformMode oldPlatformMode)
        {
            _switchMode?.OnSwitchPlatformMode(newPlatformMode, oldPlatformMode);
        }
    }
}

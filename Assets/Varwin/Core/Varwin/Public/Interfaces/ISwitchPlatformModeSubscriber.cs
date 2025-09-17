namespace Varwin.Public
{
    public interface ISwitchPlatformModeSubscriber
    {
        void OnSwitchPlatformMode(PlatformMode newPlatformMode, PlatformMode oldPlatformMode);
    }
}

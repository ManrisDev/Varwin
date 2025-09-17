using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class SpectatorAdapter : SDKAdapter
    {
        public SpectatorAdapter() : base(
            new DesktopPlayerAppearance(),
            new DesktopControllerInput(),
            new DesktopObjectInteraction(),
            new DesktopControllerInteraction(),
            new SpectatorPlayerController(),
            new DesktopPointerController(),
            new ComponentFactory<PointableObject, UniversalPointableObject>(),
            new ComponentWrapFactory<Tooltip, UniversalTooltip, TooltipObject>())
        {
        }
    }
}
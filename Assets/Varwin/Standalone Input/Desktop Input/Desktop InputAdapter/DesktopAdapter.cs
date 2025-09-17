using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopAdapter : SDKAdapter
    {
        public DesktopAdapter() : base(
            new DesktopPlayerAppearance(),
            new DesktopControllerInput(),
            new DesktopObjectInteraction(),
            new DesktopControllerInteraction(),
            new DesktopPlayerController(),
            new DesktopPointerController(),
            new ComponentFactory<PointableObject, UniversalPointableObject>(),
            new ComponentWrapFactory<Tooltip, UniversalTooltip, TooltipObject>())
        {
        }
    }
}

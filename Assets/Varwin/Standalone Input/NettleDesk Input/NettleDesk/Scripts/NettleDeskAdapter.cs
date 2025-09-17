using Varwin.DesktopInput;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    public class NettleDeskAdapter : SDKAdapter
    {
        public NettleDeskAdapter() : base(
            new DesktopPlayerAppearance(),
            new NettleDeskControllerInput(),
            new NettleDeskObjectInteraction(),
            new NettleDeskControllerInteraction(),
            new NettleDeskPlayerController(),
            new NettleDeskPointerController(),
            new ComponentFactory<PointableObject, UniversalPointableObject>(),
            new ComponentWrapFactory<Tooltip, UniversalTooltip, TooltipObject>())
        {
        }
    }
}

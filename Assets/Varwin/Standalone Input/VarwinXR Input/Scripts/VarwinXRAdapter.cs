using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRAdapter : SDKAdapter
    {
        public VarwinXRAdapter() : 
            base(new VarwinXRPlayerAppearance(), 
                new VarwinXRControllerInput(), 
                new VarwinXRObjectInteraction(), 
                new VarwinXRControllerInteraction(), 
                new VarwinXRPlayerController(), 
                new VarwinXRPointerController(),
                new ComponentFactory<PointableObject, UniversalPointableObject>(),
                new ComponentWrapFactory<Tooltip, UniversalTooltip, TooltipObject>())
        {
        }
    }
}
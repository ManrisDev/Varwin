using UnityEngine;
using Varwin.UI;
using Varwin.PlatformAdapter;

public class TooltipTransformAdapterBase : MonoBehaviour
{
    protected ControllerTooltipManager.TooltipControllers ControllerType;

    public void Init(ControllerTooltipManager.TooltipControllers typeController)
    {
        ControllerType = typeController;
    }

    public virtual Transform GetButtonTransform(ControllerInteraction.ControllerElements controllerElement)
    {
        return null;
    }
}

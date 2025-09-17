using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRTooltipAdapter : TooltipTransformAdapterBase
    {
        private VarwinXRController _varwinXRController;
        
        public VarwinXRControllerModel ControllerModel => _varwinXRController.ControllerModel;

        private void Awake()
        {
            _varwinXRController = GetComponent<VarwinXRController>();
        }

        public override Transform GetButtonTransform(ControllerInteraction.ControllerElements controllerElement)
        {
            if (!ControllerModel)
            {
                return null;
            }

            var findedControl = ControllerModel.GetControlWithType(controllerElement);
            return !findedControl ? null : findedControl.transform;
        }
    }
}
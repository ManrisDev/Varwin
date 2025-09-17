using System;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRPointerController : PointerController
    {
        public VarwinXRPointerController()
        {
            Managers = new ComponentWrapFactory<PointerManager, VarwinXRPointerManager, VarwinXRPointerControllerComponent>();
        }
        
        public override bool IsMenuOpened
        {
            set
            {
                if (Left == null || Right == null)
                {
                    return;
                }
                
                Left.IsMenuOpened = value;
                Right.IsMenuOpened = value;
            }
        }
        
        public class VarwinXRPointerManager : PointerManager, IInitializable<VarwinXRPointerControllerComponent>
        {
            private VarwinXRPointerControllerComponent _controllerComponent;

            public override bool IsMenuOpened
            {
                get => _controllerComponent.IsMenuOpened;
                set => _controllerComponent.IsMenuOpened = value;
            }

            public override bool ShowUIPointer
            {
                get => _controllerComponent.ShowUIPointer;
                set => _controllerComponent.ShowUIPointer = value;
            }

            public void Init(VarwinXRPointerControllerComponent interactableObject)
            {
                _controllerComponent = interactableObject;

                switch (_controllerComponent.Hand)
                {
                    case ControllerInteraction.ControllerHand.Left:
                        InputAdapter.Instance.PointerController.SetLeftManager(this);

                        break;
                    case ControllerInteraction.ControllerHand.Right:
                        InputAdapter.Instance.PointerController.SetRightManager(this);

                        break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
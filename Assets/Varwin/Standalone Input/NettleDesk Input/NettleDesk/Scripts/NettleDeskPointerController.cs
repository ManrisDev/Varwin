using System;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    public class NettleDeskPointerController : PointerController
    {
        public NettleDeskPointerController()
        {
            Managers = new ComponentWrapFactory<PointerManager, NettleDeskPointerManager, NettleDeskPointerControllerComponent>();
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
        
        public class NettleDeskPointerManager : PointerManager, IInitializable<NettleDeskPointerControllerComponent>
        {
            private NettleDeskPointerControllerComponent _controllerComponent;

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

            public void Init(NettleDeskPointerControllerComponent interactableObject)
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
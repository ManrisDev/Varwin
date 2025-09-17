using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class DesktopControllerInput : ControllerInput
    {
        public override event ControllerInteractionEventHandler ControllerEnabled;
        public override event ControllerInteractionEventHandler ControllerDisabled;
        public override event ControllerInteractionEventHandler TriggerPressed;
        public override event ControllerInteractionEventHandler TriggerReleased;
        public override event ControllerInteractionEventHandler TouchpadReleased;
        public override event ControllerInteractionEventHandler TouchpadPressed;
        public override event ControllerInteractionEventHandler ButtonTwoPressed;
        public override event ControllerInteractionEventHandler ButtonTwoReleased;
        public override event ControllerInteractionEventHandler GripPressed;
        
        private readonly List<ControllerEvents> _controllerEvents = new();

        public DesktopControllerInput()
        {
            ControllerEventFactory = 
                new ComponentWrapFactory<ControllerEvents, DesktopControllerEvents, DesktopControllerEventComponent>();
        }
        
        public override void AddController(ControllerEvents events)
        {
            if (_controllerEvents.Contains(events))
            {
                return;
            }
            
            _controllerEvents.Add(events);

            events.ControllerEnabled += (sender, args) => { ControllerEnabled?.Invoke(sender, args); };

            events.ControllerDisabled += (sender, args) => { ControllerDisabled?.Invoke(sender, args); };

            events.TriggerPressed += (sender, args) => { TriggerPressed?.Invoke(sender, args); };
            
            events.TriggerReleased += (sender, args) => { TriggerReleased?.Invoke(sender, args); };

            events.TouchpadReleased += (sender, args) => { TouchpadReleased?.Invoke(sender, args); };

            events.TouchpadPressed += (sender, args) => { TouchpadPressed?.Invoke(sender, args); };

            events.ButtonTwoPressed += (sender, args) => { ButtonTwoPressed?.Invoke(sender, args); };

            events.ButtonTwoReleased += (sender, args) => { ButtonTwoReleased?.Invoke(sender, args); };

            events.GripPressed += (sender, args) => { GripPressed?.Invoke(sender, args); };
        }

        public override ControllerEvents GetController(ControllerInteraction.ControllerHand hand)
        {
            return _controllerEvents?.FirstOrDefault(a=>a.Hand == hand);
        }

        private class DesktopControllerEvents : ControllerEvents, IInitializable<DesktopControllerEventComponent>
        {
            public override ControllerInteraction.ControllerHand Hand => ControllerInteraction.ControllerHand.Right;
            public override event ControllerInteractionEventHandler ControllerEnabled;
            public override event ControllerInteractionEventHandler ControllerDisabled;
            public override event ControllerInteractionEventHandler TriggerPressed;
            public override event ControllerInteractionEventHandler TriggerReleased;
            public override event ControllerInteractionEventHandler TouchpadReleased;
            public override event ControllerInteractionEventHandler TouchpadPressed;
            public override event ControllerInteractionEventHandler ButtonTwoPressed;
            public override event ControllerInteractionEventHandler ButtonTwoReleased;
            public override event ControllerInteractionEventHandler GripPressed;
            
            public override GameObject gameObject => _eventComponent.gameObject;
            public override Transform transform => _eventComponent.transform;

            private DesktopControllerEventComponent _eventComponent;
            
            public override bool IsTouchpadPressed() => _eventComponent.IsTouchpadPressed();

            public override bool IsTouchpadReleased()=> _eventComponent.IsTouchpadReleased();

            public override bool IsTriggerPressed()=> _eventComponent.IsTriggerPressed();

            public override bool IsTriggerReleased() => _eventComponent.IsTriggerReleased();

            public override bool IsButtonPressed(ButtonAlias gripPress) => _eventComponent.IsButtonPressed(gripPress);

            public override bool GetBoolInputActionState(string actionStateName) => false;

            public override void OnGripReleased(ControllerInteractionEventArgs controllerInteractionEventArgs)
            {
                _eventComponent.OnGripReleased(controllerInteractionEventArgs);
            }

            public override GameObject GetController() => _eventComponent.GetController();

            public void Init(DesktopControllerEventComponent monoBehaviour)
            {
                _eventComponent = monoBehaviour;

                _eventComponent.ControllerEnabled += (sender, args) => { ControllerEnabled?.Invoke(sender, args); };
                _eventComponent.LeftMouseButtonPressed += (sender, args) => { TriggerPressed?.Invoke(sender, args); };
                _eventComponent.LeftMouseButtonReleased += (sender, args) => { TriggerReleased?.Invoke(sender, args); };
                _eventComponent.TeleportPressed += (sender, args) => { TouchpadPressed?.Invoke(sender, args); };
                _eventComponent.TeleportReleased += (sender, args) => { TouchpadReleased?.Invoke(sender, args); };
                _eventComponent.RightMouseButtonPressed += (sender, args) => { GripPressed?.Invoke(sender, args); };
                
                InputAdapter.Instance.ControllerInput.AddController(this);
            }
        }
    }
}

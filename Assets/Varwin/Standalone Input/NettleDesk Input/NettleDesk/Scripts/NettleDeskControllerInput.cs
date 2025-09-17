using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    public class NettleDeskControllerInput : ControllerInput
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
        public override event ControllerInteractionEventHandler GripReleased;
        
        private readonly List<ControllerEvents> _controllerEvents = new();
        
        public NettleDeskControllerInput()
        {
            ControllerEventFactory = 
                new ComponentWrapFactory<ControllerEvents, NettleDeskControllerEvents, NettleDeskControllerEventComponentBase>();
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
            
            events.GripReleased += (sender, args) => { GripReleased?.Invoke(sender, args); };
        }

        public override ControllerEvents GetController(ControllerInteraction.ControllerHand hand)
        {
            return _controllerEvents?.FirstOrDefault(a => a.Hand == hand);
        }

        private class NettleDeskControllerEvents : ControllerEvents, IInitializable<NettleDeskControllerEventComponentBase>
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
            public override event ControllerInteractionEventHandler GripReleased;
            
            public override GameObject gameObject => _eventComponent.gameObject;
            public override Transform transform => _eventComponent.transform;

            private NettleDeskControllerEventComponentBase _eventComponent;
            
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

            public void Init(NettleDeskControllerEventComponentBase monoBehaviour)
            {
                _eventComponent = monoBehaviour;

                _eventComponent.ControllerEnabled += (sender, args) => { ControllerEnabled?.Invoke(sender, args); };
                _eventComponent.ControlledDisabled += (sender, args) => { ControllerDisabled?.Invoke(sender, args); };
                _eventComponent.TriggerPressed += (sender, args) => { TriggerPressed?.Invoke(sender, args); };
                _eventComponent.TriggerReleased += (sender, args) => { TriggerReleased?.Invoke(sender, args); };
                _eventComponent.TeleportPressed += (sender, args) => { TouchpadPressed?.Invoke(sender, args); };
                _eventComponent.TeleportReleased += (sender, args) => { TouchpadReleased?.Invoke(sender, args); };
                _eventComponent.GripPressed += (sender, args) => { GripPressed?.Invoke(sender, args); };
                _eventComponent.GripReleased += (sender, args) => { GripReleased?.Invoke(sender, args); };
                
                InputAdapter.Instance.ControllerInput.AddController(this);
            }
        }
    }
}

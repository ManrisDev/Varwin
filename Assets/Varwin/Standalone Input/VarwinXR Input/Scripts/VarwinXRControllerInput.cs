using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRControllerInput : ControllerInput
    {
        public override event ControllerInteractionEventHandler ControllerEnabled;
        public override event ControllerInteractionEventHandler ControllerDisabled;
        public override event ControllerInteractionEventHandler TriggerPressed;
        public override event ControllerInteractionEventHandler TriggerReleased;
        public override event ControllerInteractionEventHandler TouchpadReleased;
        public override event ControllerInteractionEventHandler TouchpadPressed;
        public override event ControllerInteractionEventHandler ButtonOnePressed;
        public override event ControllerInteractionEventHandler ButtonOneReleased;
        public override event ControllerInteractionEventHandler ButtonTwoPressed;
        public override event ControllerInteractionEventHandler ButtonTwoReleased;
        public override event ControllerInteractionEventHandler GripPressed;
        public override event ControllerInteractionEventHandler GripReleased;
        public override event ControllerInteractionEventHandler TouchpadButtonPressed;
        public override event ControllerInteractionEventHandler TouchpadButtonReleased;

        public event ControllerInteractionEventHandler TurnLeftPressed;
        public event ControllerInteractionEventHandler TurnRightPressed;

        private VarwinXRInteractableObject _interactableObject;

        private readonly List<ControllerEvents> _controllerEvents = new List<ControllerEvents>();

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

            events.TouchpadButtonPressed += (sender, args) => { TouchpadButtonPressed?.Invoke(sender, args); };
            events.TouchpadButtonReleased += (sender, args) => { TouchpadButtonReleased?.Invoke(sender, args); };
            
            events.ButtonOnePressed += (sender, args) => { ButtonOnePressed?.Invoke(sender, args); };
            events.ButtonOneReleased += (sender, args) => { ButtonOneReleased?.Invoke(sender, args); };

            events.ButtonTwoPressed += (sender, args) => { ButtonTwoPressed?.Invoke(sender, args); };
            events.ButtonTwoReleased += (sender, args) => { ButtonTwoReleased?.Invoke(sender, args); };

            events.GripPressed += (sender, args) => { GripPressed?.Invoke(sender, args); };
            events.GripReleased += (sender, args) => { GripReleased?.Invoke(sender, args); };

            ((VarwinXRControllerEvents) events).TurnLeftPressed += (sender, args) => { TurnLeftPressed?.Invoke(sender, args); };
            ((VarwinXRControllerEvents) events).TurnRightPressed += (sender, args) => { TurnRightPressed?.Invoke(sender, args); };
        }

        public override ControllerEvents GetController(ControllerInteraction.ControllerHand hand)
        {
            return _controllerEvents?.FirstOrDefault(a => a.Hand == hand);
        }

        public VarwinXRControllerInput()
        {
            ControllerEventFactory =
                new ComponentWrapFactory<ControllerEvents, VarwinXRControllerEvents, VarwinXRControllerEventComponent>();
        }

        private class VarwinXRControllerEvents : ControllerEvents, IInitializable<VarwinXRControllerEventComponent>
        {
            private VarwinXRControllerEventComponent _eventComponent = null;
            public override GameObject gameObject => _eventComponent.gameObject;
            private bool _inputActionState;

            public override Transform transform => _eventComponent.transform;
            public override ControllerInteraction.ControllerHand Hand => _eventComponent.IsLeft ? ControllerInteraction.ControllerHand.Left : ControllerInteraction.ControllerHand.Right;
            public override event ControllerInteractionEventHandler ControllerEnabled;
            public override event ControllerInteractionEventHandler ControllerDisabled;
            public override event ControllerInteractionEventHandler TriggerPressed;
            public override event ControllerInteractionEventHandler TriggerReleased;
            public override event ControllerInteractionEventHandler TouchpadReleased;
            public override event ControllerInteractionEventHandler TouchpadPressed;
            public override event ControllerInteractionEventHandler TouchpadButtonPressed;
            public override event ControllerInteractionEventHandler TouchpadButtonReleased;
            public override event ControllerInteractionEventHandler ButtonOnePressed;
            public override event ControllerInteractionEventHandler ButtonOneReleased;
            public override event ControllerInteractionEventHandler ButtonTwoPressed;
            public override event ControllerInteractionEventHandler ButtonTwoReleased;
            public override event ControllerInteractionEventHandler GripPressed;
            public override event ControllerInteractionEventHandler GripReleased;

            public event ControllerInteractionEventHandler TurnLeftPressed;
            public event ControllerInteractionEventHandler TurnRightPressed;

            public override float GetGripValue() => _eventComponent.GripValue;
            public override float GetTriggerValue() => _eventComponent.TriggerValue;
            public override Vector2 GetTrackpadValue() => _eventComponent.Primary2DAxisValue;
            public override bool IsTouchpadPressed() => _eventComponent.IsThumbstickPressed();
            public override bool IsTouchpadReleased() => _eventComponent.IsThumbstickReleased();
            public override bool IsTriggerPressed() => _eventComponent.IsTriggerPressed();
            public override bool IsTriggerReleased() => _eventComponent.IsTriggerReleased();
            public override bool IsButtonPressed(ButtonAlias gripPress) => _eventComponent.IsButtonPressed(gripPress);

            public override bool GetBoolInputActionState(string actionStateName) => _inputActionState;
            public override bool IsEnabled() => _eventComponent.IsInitialized();

            public override void OnGripReleased(ControllerInteractionEventArgs controllerInteractionEventArgs)
            {
                _eventComponent.OnGripReleased(controllerInteractionEventArgs);
            }

            public override GameObject GetController() => _eventComponent.gameObject.gameObject;

            ControllerInteractionEventArgs GetControllerArguments(VarwinXRControllerEventComponent sender)
            {
                var args = new ControllerInteractionEventArgs()
                {
                    controllerReference = new PlayerController.ControllerReferenceArgs()
                    {
                        hand = !sender.IsLeft ? ControllerInteraction.ControllerHand.Right : ControllerInteraction.ControllerHand.Left
                    }
                };

                return args;
            }
            
            public void Init(VarwinXRControllerEventComponent interactableObject)
            {
                _eventComponent = interactableObject;
                
                _eventComponent.TriggerPressed += (sender) => { TriggerPressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.TriggerReleased += (sender) => { TriggerReleased?.Invoke(sender, GetControllerArguments(sender));  };

                _eventComponent.ThumbstickReleased += (sender) => { TouchpadReleased?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ThumbstickPressed += (sender) => { TouchpadPressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ButtonOnePressed += (sender) => { ButtonOnePressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ButtonOneReleased += (sender) => { ButtonOneReleased?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ButtonTwoPressed += (sender) => { ButtonTwoPressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ButtonTwoReleased += (sender) => { ButtonTwoReleased?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.Initialized += sender => { ControllerEnabled?.Invoke(sender, GetControllerArguments(sender)); };
                
                _eventComponent.Deinitialized += sender => { ControllerDisabled?.Invoke(sender, GetControllerArguments(sender)); };
                
                _eventComponent.GripPressed += (sender) =>
                {
                    GripPressed?.Invoke(sender, GetControllerArguments(sender));
                    _inputActionState = !_inputActionState;
                };
                _eventComponent.GripReleased += (sender) =>
                {
                    if (!sender.HasGrabbedObject)
                    {
                        _inputActionState = false;
                    }
                    
                    GripReleased?.Invoke(sender, GetControllerArguments(sender));
                };

                _eventComponent.TurnLeftPressed += (sender) => { TurnLeftPressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.TurnRightPressed += (sender) => { TurnRightPressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ThumbstickButtonPressed += (sender) => { TouchpadButtonPressed?.Invoke(sender, GetControllerArguments(sender)); };

                _eventComponent.ThumbstickButtonReleased += (sender) => { TouchpadButtonReleased?.Invoke(sender, GetControllerArguments(sender)); };

                InputAdapter.Instance.ControllerInput.AddController(this);
            }
        }
    }
}
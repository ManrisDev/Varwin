using System;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.NettleDesk
{
    public abstract class NettleDeskControllerEventComponentBase : MonoBehaviour
    {
        public event ControllerInput.ControllerInteractionEventHandler ControllerEnabled;
        public event ControllerInput.ControllerInteractionEventHandler ControlledDisabled;
        public event ControllerInput.ControllerInteractionEventHandler TriggerPressed;
        public event ControllerInput.ControllerInteractionEventHandler TriggerReleased;
        public event ControllerInput.ControllerInteractionEventHandler TeleportPressed;
        public event ControllerInput.ControllerInteractionEventHandler TeleportReleased;
        public event ControllerInput.ControllerInteractionEventHandler GripPressed;
        public event ControllerInput.ControllerInteractionEventHandler GripReleased;

        public abstract bool IsTouchpadPressed();
        public abstract bool IsTouchpadReleased();
        public abstract bool IsTriggerPressed();
        public abstract bool IsTriggerReleased();
        public abstract bool IsButtonPressed(ControllerInput.ButtonAlias gripPress);
        public abstract void OnGripReleased(ControllerInput.ControllerInteractionEventArgs controllerInteractionEventArgs);
        public abstract GameObject GetController();

        protected void InvokeControllerEnabled(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            ControllerEnabled?.Invoke(sender, args);
        }

        protected void InvokeControllerDisabled(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            ControlledDisabled?.Invoke(sender, args);
        }
        
        protected void InvokeTriggerPressed(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            TriggerPressed?.Invoke(sender, args);
        }

        protected void InvokeTriggerReleased(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            TriggerReleased?.Invoke(sender, args);
        }
        
        protected void InvokeTeleportPressed(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            TeleportPressed?.Invoke(sender, args);
        }

        protected void InvokeTeleportReleased(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            TeleportReleased?.Invoke(sender, args);
        }

        protected void InvokeGripPressed(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            GripPressed?.Invoke(sender, args);
        }
        
        protected void InvokeGripReleased(object sender, ControllerInput.ControllerInteractionEventArgs args)
        {
            GripReleased?.Invoke(sender, args);
        }
    }
}
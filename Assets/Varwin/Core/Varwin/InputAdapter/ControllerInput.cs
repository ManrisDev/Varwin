using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public abstract class ControllerInput
    {
        public IMonoComponent<ControllerEvents> ControllerEventFactory;

        public enum ButtonAlias
        {
            /// <summary>
            /// No button specified.
            /// </summary>
            Undefined,

            /// <summary>
            /// The trigger is squeezed past the current hairline threshold.
            /// </summary>
            TriggerHairline,

            /// <summary>
            /// The trigger is squeezed a small amount.
            /// </summary>
            TriggerTouch,

            /// <summary>
            /// The trigger is squeezed about half way in.
            /// </summary>
            TriggerPress,

            /// <summary>
            /// The trigger is squeezed all the way down.
            /// </summary>
            TriggerClick,

            /// <summary>
            /// The grip is squeezed past the current hairline threshold.
            /// </summary>
            GripHairline,

            /// <summary>
            /// The grip button is touched.
            /// </summary>
            GripTouch,

            /// <summary>
            /// The grip button is pressed.
            /// </summary>
            GripPress,

            /// <summary>
            /// The grip button is pressed all the way down.
            /// </summary>
            GripClick,

            /// <summary>
            /// The touchpad is touched (without pressing down to click).
            /// </summary>
            TouchpadTouch,

            /// <summary>
            /// The touchpad is pressed (to the point of hearing a click).
            /// </summary>
            TouchpadPress,

            /// <summary>
            /// The touchpad two is touched (without pressing down to click).
            /// </summary>
            TouchpadTwoTouch,

            /// <summary>
            /// The touchpad two is pressed (to the point of hearing a click).
            /// </summary>
            TouchpadTwoPress,

            /// <summary>
            /// The button one is touched.
            /// </summary>
            ButtonOneTouch,

            /// <summary>
            /// The button one is pressed.
            /// </summary>
            ButtonOnePress,

            /// <summary>
            /// The button two is touched.
            /// </summary>
            ButtonTwoTouch,

            /// <summary>
            /// The button two is pressed.
            /// </summary>
            ButtonTwoPress,

            /// <summary>
            /// The start menu is pressed.
            /// </summary>
            StartMenuPress,

            /// <summary>
            /// The touchpad sense touch is active.
            /// </summary>
            TouchpadSense,

            /// <summary>
            /// The trigger sense touch is active.
            /// </summary>
            TriggerSense,

            /// <summary>
            /// The middle finger sense touch is active.
            /// </summary>
            MiddleFingerSense,

            /// <summary>
            /// The ring finger sense touch is active.
            /// </summary>
            RingFingerSense,

            /// <summary>
            /// The pinky finger sense touch is active.
            /// </summary>
            PinkyFingerSense,

            /// <summary>
            /// The grip sense axis touch is active.
            /// </summary>
            GripSense,

            /// <summary>
            /// The grip sense axis is pressed.
            /// </summary>
            GripSensePress
        }
        
        public abstract event ControllerInteractionEventHandler ControllerEnabled;
        public abstract event ControllerInteractionEventHandler ControllerDisabled;
        public abstract event ControllerInteractionEventHandler TriggerPressed;
        public abstract event ControllerInteractionEventHandler TriggerReleased;
        public abstract event ControllerInteractionEventHandler TouchpadReleased;
        public abstract event ControllerInteractionEventHandler TouchpadPressed;
        public virtual event ControllerInteractionEventHandler ButtonOnePressed;
        public virtual event ControllerInteractionEventHandler ButtonOneReleased;
        public abstract event ControllerInteractionEventHandler ButtonTwoPressed;
        public abstract event ControllerInteractionEventHandler ButtonTwoReleased;
        public abstract event ControllerInteractionEventHandler GripPressed;
        public virtual event ControllerInteractionEventHandler GripReleased;
        public virtual event ControllerInteractionEventHandler TouchpadButtonPressed;
        public virtual event ControllerInteractionEventHandler TouchpadButtonReleased;
        
        public abstract class ControllerEvents
        {
            public abstract GameObject gameObject { get; }
            public abstract Transform transform { get; }
            public abstract ControllerInteraction.ControllerHand Hand { get; }
            public abstract event ControllerInteractionEventHandler ControllerEnabled;
            public abstract event ControllerInteractionEventHandler ControllerDisabled;
            public abstract event ControllerInteractionEventHandler TriggerPressed;
            public abstract event ControllerInteractionEventHandler TriggerReleased;
            public abstract event ControllerInteractionEventHandler TouchpadReleased;
            public abstract event ControllerInteractionEventHandler TouchpadPressed;
            public virtual event ControllerInteractionEventHandler TouchpadButtonPressed;
            public virtual event ControllerInteractionEventHandler TouchpadButtonReleased;
            public virtual event ControllerInteractionEventHandler ButtonOnePressed;
            public virtual event ControllerInteractionEventHandler ButtonOneReleased;
            public abstract event ControllerInteractionEventHandler ButtonTwoPressed;
            public abstract event ControllerInteractionEventHandler ButtonTwoReleased;
            public abstract event ControllerInteractionEventHandler GripPressed;
            public virtual event ControllerInteractionEventHandler GripReleased;
                
            public abstract bool IsTouchpadPressed();
            public abstract bool IsTouchpadReleased();
            public abstract bool IsTriggerPressed();
            
            public abstract bool IsTriggerReleased();
            
            public abstract bool IsButtonPressed(ButtonAlias gripPress);

            public abstract bool GetBoolInputActionState(string actionStateName);

            public abstract void OnGripReleased(ControllerInteractionEventArgs controllerInteractionEventArgs);
            
            public virtual float GetTriggerValue() => 0;
            
            public virtual float GetGripValue() => 0;

            public virtual bool IsEnabled() => false;
            
            public virtual Vector2 GetTrackpadValue() => Vector2.zero;

            public abstract GameObject GetController();
        }

        public delegate void ControllerInteractionEventHandler(object sender, ControllerInteractionEventArgs args = new ControllerInteractionEventArgs());

        public struct ControllerInteractionEventArgs
        {
            public PlayerController.ControllerReferenceArgs controllerReference;
            public float buttonPressure;
            public Vector2 touchpadAxis;
            public float touchpadAngle;
            public Vector2 touchpadTwoAxis;
            public float touchpadTwoAngle;
        }

        public abstract void AddController(ControllerEvents events);
        public abstract ControllerEvents GetController(ControllerInteraction.ControllerHand hand);
    }
}

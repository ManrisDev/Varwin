using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.UI
{
    public class ControllerTooltipManager : MonoBehaviour
    {
        public enum TooltipButtons
        {
            Trigger,
            Grip,
            Touchpad,
            ButtonOne,
            ButtonTwo,
            StartMenu
        }

        public enum TooltipControllers
        {
            Left,
            Right,
            Both
        }

        private class TooltipDescriptor
        {
            public string Text;
            public TooltipControllers Controller;
            public TooltipButtons Button;
            public Transform ButtonTransform;
            public GameObject TooltipObject;
            public Tooltip Tooltip;
            public Highlighter Highlighter;
        }
        
        public ControllerInput.ControllerEvents LeftControllerEvents, RightControllerEvents;
        private TooltipTransformAdapterBase _leftTooltipAdapter, _rightTooltipAdapter;

        public GameObject LeftController, RightController;

        public GameObject TooltipTemplate;

        private bool _leftControllerReady, _rightControllerReady;

        private bool _showTooltip = true;

        private Dictionary<TooltipButtons, TooltipDescriptor> _tooltipObjectsLeft, _tooltipObjectsRight;

        private TooltipButtons[] _allButtons;
        
        void Start()
        {
            LeftControllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(LeftController);
            RightControllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(RightController);

            if (LeftControllerEvents == null || RightController == null)
            {
                Debug.LogError	("Missing ControllerEvents!");
                return;
            }

            _leftControllerReady = LeftControllerEvents.IsEnabled();
            _rightControllerReady = RightControllerEvents.IsEnabled();
            _leftTooltipAdapter = LeftController.GetComponent<TooltipTransformAdapterBase>();
            _rightTooltipAdapter = RightController.GetComponent<TooltipTransformAdapterBase>();
            
            if (_leftTooltipAdapter == null || _rightTooltipAdapter == null)
            {
                Debug.LogError	("Missing TooltipTransformAdapterBase!");
                return;
            }
            
            _leftTooltipAdapter.Init(TooltipControllers.Left);
            _rightTooltipAdapter.Init(TooltipControllers.Right);

            StartCoroutine(ControllerEventsSubscriptionSafe());

            _tooltipObjectsLeft = new Dictionary<TooltipButtons, TooltipDescriptor>();
            _tooltipObjectsRight = new Dictionary<TooltipButtons, TooltipDescriptor>();

            _allButtons = Enum.GetValues(typeof(TooltipButtons)).Cast<TooltipButtons>().ToArray();
        }

        private void Update()
        {
            foreach (var button in _allButtons)
            {
                if (_tooltipObjectsLeft.ContainsKey(button) && !_tooltipObjectsLeft[button].ButtonTransform)
                {
                    RefreshTooltip(_tooltipObjectsLeft[button]);
                }
                
                if (_tooltipObjectsRight.ContainsKey(button) && !_tooltipObjectsRight[button].ButtonTransform)
                {
                    RefreshTooltip(_tooltipObjectsRight[button]);
                }
            }
        }

        public void SetTooltip(
            string text,
            TooltipControllers controller,
            TooltipButtons button,
            bool buttonGlow,
            bool vibration = true)
        {
            StartCoroutine(TooltipCoroutine(text,
                controller,
                button,
                buttonGlow,
                vibration));
        }

        public void HideTooltip(TooltipControllers controller, TooltipButtons button)
        {
            SetTooltip("",
                controller,
                button,
                false);
        }

        private void RefreshTooltip(TooltipDescriptor tooltipDescriptor)
        {
            if (tooltipDescriptor == null || string.IsNullOrEmpty(tooltipDescriptor.Text))
            {
                return;
            }
            
            if(tooltipDescriptor.Highlighter)
            {
                Destroy(tooltipDescriptor.Highlighter);
            }
            
            if(tooltipDescriptor.TooltipObject)
            {
                Destroy(tooltipDescriptor.TooltipObject);
            }

            if (tooltipDescriptor.Controller == TooltipControllers.Left)
            {
                _tooltipObjectsLeft.Remove(tooltipDescriptor.Button);
            }
            else
            {
                _tooltipObjectsRight.Remove(tooltipDescriptor.Button);
            }

            SetTooltip(tooltipDescriptor.Text, tooltipDescriptor.Controller, tooltipDescriptor.Button, true);
            
        }
        
        private IEnumerator TooltipCoroutine(
            string text,
            TooltipControllers controller,
            TooltipButtons button,
            bool buttonGlow,
            bool vibration = true)
        {
            while (!UpdateTooltip(text,
                controller,
                button,
                buttonGlow,
                vibration))
            {
                yield return new WaitForEndOfFrame();
            }
        }

        private bool UpdateTooltip(
            string text,
            TooltipControllers controller,
            TooltipButtons button,
            bool buttonGlow,
            bool vibration = true)
        {
            if ((controller == TooltipControllers.Left && !_leftControllerReady)
                || (controller == TooltipControllers.Right && !_rightControllerReady))
            {
                return false;
            }

            GameObject tooltipObject;
            Tooltip tooltip;

            Transform buttonAttachTransform = null;
            Highlighter highlighter = null;
            
            if (controller == TooltipControllers.Left && !_tooltipObjectsLeft.ContainsKey(button)
                || controller == TooltipControllers.Right && !_tooltipObjectsRight.ContainsKey(button))
            {
                buttonAttachTransform = GetButtonTransform(controller, button);

                if (!buttonAttachTransform)
                {
                    return false;
                }

                tooltipObject = Instantiate(TooltipTemplate, GetTooltipInstantiationTransform(controller, button));

                tooltip = InputAdapter.Instance.Tooltip.GetFromChildren(tooltipObject, true);

                tooltip.drawLineTo = buttonAttachTransform;

                highlighter = HighlightAdapter.Instance.AddHighlighter(buttonAttachTransform.parent.gameObject);

                highlighter.SetConfig(HighlightAdapter.Instance.Configs.ControllerTooltip);
                
                highlighter.IsEnabled = false;

                var tooltipDescriptor = new TooltipDescriptor
                {
                    Text = text,
                    Controller = controller,
                    Button = button,
                    ButtonTransform = buttonAttachTransform,
                    TooltipObject = tooltipObject,
                    Tooltip = tooltip,
                    Highlighter = highlighter
                };
                
                if (controller == TooltipControllers.Left)
                {
                    _tooltipObjectsLeft.Add(button, tooltipDescriptor);
                }
                else
                {
                    _tooltipObjectsRight.Add(button, tooltipDescriptor);
                }
                
            }
            else
            {
                if (controller == TooltipControllers.Left)
                {
                    _tooltipObjectsLeft[button].Text = text;
                    
                    tooltip = _tooltipObjectsLeft[button].Tooltip;
                    tooltipObject = _tooltipObjectsLeft[button].TooltipObject;
                }
                else
                {
                    _tooltipObjectsRight[button].Text = text;
                    
                    tooltip = _tooltipObjectsRight[button].Tooltip;
                    tooltipObject = _tooltipObjectsRight[button].TooltipObject; 
                }
            }

            tooltip.displayText = text;

            tooltip.ResetTooltip();

            if (!highlighter)
            {
                buttonAttachTransform = GetButtonTransform(controller, button);

                if (!buttonAttachTransform)
                {
                    return false;
                }
                
                highlighter = buttonAttachTransform.GetComponent<Highlighter>();

                if (!highlighter)
                {
                    highlighter = buttonAttachTransform.GetComponentInParent<Highlighter>();
                }
                
                if (highlighter)
                {
                    highlighter.IsEnabled = false;
                }
            }

            if (text.Trim().Length != 0 || buttonGlow)
            {
                tooltipObject.SetActive(true);

                if (highlighter)
                {
                    highlighter.IsEnabled = buttonGlow;
                }

                if (vibration)
                {
                    Vibrate(controller);
                }
            }
            else
            {
                tooltipObject.SetActive(false);
            }

            return true;
        }

        private Transform GetButtonTransform(TooltipControllers controller, TooltipButtons button)
        {
            ControllerInteraction.ControllerElements findElement;

            TooltipTransformAdapterBase adapter = null;

            Transform returnedTransform = null;

            if (controller == TooltipControllers.Left)
            {
                adapter = _leftTooltipAdapter;
            }
            else if (controller == TooltipControllers.Right)
            {
                adapter = _rightTooltipAdapter;
            }

            if (!adapter) return null;


            switch (button)
            {
                case TooltipButtons.Trigger:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.Trigger);
                    break;
                case TooltipButtons.Grip:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.GripLeft);
                    break;
                case TooltipButtons.Touchpad:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.Touchpad);
                    break;
                case TooltipButtons.ButtonOne:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.ButtonOne);
                    break;
                case TooltipButtons.ButtonTwo:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.ButtonTwo);
                    break;
                case TooltipButtons.StartMenu:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.StartMenu);
                    break;
                default:
                    returnedTransform = adapter.GetButtonTransform(ControllerInteraction.ControllerElements.Touchpad);
                    break;
            }

            return returnedTransform;
        }

        public Transform GetTooltipInstantiationTransform(TooltipControllers controller, TooltipButtons button)
        {
            Transform tooltipTransform;

            if (controller == TooltipControllers.Left)
            {
                tooltipTransform = FindDeepChild(LeftControllerEvents.transform, "TooltipPositions");
            }
            else
            {
                tooltipTransform = FindDeepChild(RightControllerEvents.transform, "TooltipPositions");
            }

            switch (button)
            {
                case TooltipButtons.Trigger:
                    tooltipTransform = tooltipTransform.Find("Trigger");

                    break;
                case TooltipButtons.Grip:
                    tooltipTransform = tooltipTransform.Find("Grip");

                    break;
                case TooltipButtons.Touchpad:
                    tooltipTransform = tooltipTransform.Find("Touchpad");

                    break;
                case TooltipButtons.ButtonOne:
                    tooltipTransform = tooltipTransform.Find("ButtonOne");

                    break;
                case TooltipButtons.ButtonTwo:
                    tooltipTransform = tooltipTransform.Find("ButtonTwo");

                    break;
                case TooltipButtons.StartMenu:
                    tooltipTransform = tooltipTransform.Find("StartMenu");

                    break;
            }

            return tooltipTransform;
        }

        private void SetLeftControllerReady(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            _leftControllerReady = true;
        }

        private void SetRightControllerReady(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            _rightControllerReady = true;
        }


        IEnumerator ControllerEventsSubscriptionSafe()
        {
            while (LeftControllerEvents == null || RightControllerEvents == null)
            {
                yield return null;
            }

            LeftControllerEvents.ControllerEnabled += SetLeftControllerReady;
            RightControllerEvents.ControllerEnabled += SetRightControllerReady;
        }

        public void Vibrate(
            TooltipControllers controller,
            float strength = 0.05f,
            float duration = 0.1f,
            float interval = 0.005f)
        {
            if (controller == TooltipControllers.Left || controller == TooltipControllers.Both)
            {
                var hand = InputAdapter.Instance.PlayerController.Nodes.LeftHand;

                hand.Controller.TriggerHapticPulse(strength, duration, interval);
            }

            if (controller == TooltipControllers.Right || controller == TooltipControllers.Both)
            {
                var hand = InputAdapter.Instance.PlayerController.Nodes.RightHand;

                hand.Controller.TriggerHapticPulse(strength, duration, interval);
            }
        }

        private static Transform FindDeepChild(Transform source, string childName)
        {
            if (source.name == childName)
            {
                return source;
            }
        
            foreach (Transform child in source)
            {
                Transform result = FindDeepChild(child, childName);
                if (result)
                {
                    return result;
                }
            }
            return null;
        }
    }
}

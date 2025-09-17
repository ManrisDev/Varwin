using System.Collections.Generic;
using UnityEngine.XR;
using Varwin.PlatformAdapter;

namespace Varwin
{
    public static class DeviceHelper
    {
        public static Dictionary<ControllerInteraction.ControllerElements, string> OculusButtonNames = new()
        {
            {ControllerInteraction.ControllerElements.Trigger, "trigger"},
            {ControllerInteraction.ControllerElements.GripLeft, "button_grip"},
            {ControllerInteraction.ControllerElements.GripRight, "button_grip"},
            {ControllerInteraction.ControllerElements.Touchpad, "thumbstick"},
            {ControllerInteraction.ControllerElements.ButtonOne, "button_x"},
            {ControllerInteraction.ControllerElements.ButtonTwo, "button_y"},
            {ControllerInteraction.ControllerElements.StartMenu, "button_enter"}
        };

        public static Dictionary<ControllerInteraction.ControllerElements, string> ViveButtonNames = new()
        {
            {ControllerInteraction.ControllerElements.Trigger, "trigger"},
            {ControllerInteraction.ControllerElements.GripLeft, "lgrip"},
            {ControllerInteraction.ControllerElements.GripRight, "rgrip"},
            {ControllerInteraction.ControllerElements.Touchpad, "trackpad"},
            // TODO: find name
            {ControllerInteraction.ControllerElements.ButtonOne, ""},
            {ControllerInteraction.ControllerElements.ButtonTwo, "button"},
            //TODO: find name
            {ControllerInteraction.ControllerElements.StartMenu, ""}
        };

        
        public static Dictionary<ControllerInteraction.ControllerElements, string> KnucklesButtonNames = new()
        {
            {ControllerInteraction.ControllerElements.Trigger, "trigger"},
            {ControllerInteraction.ControllerElements.GripLeft, "squeeze"},
            {ControllerInteraction.ControllerElements.GripRight, "squeeze"},
            {ControllerInteraction.ControllerElements.Touchpad, "thumbstick"},
            {ControllerInteraction.ControllerElements.ButtonOne, "button_b"},
            {ControllerInteraction.ControllerElements.ButtonTwo, "button_a"},
            {ControllerInteraction.ControllerElements.StartMenu, "sys_button"}
        };

                
        public static Dictionary<ControllerInteraction.ControllerElements, string> DefaultButtonNames = new()
        {
            {ControllerInteraction.ControllerElements.Trigger, "trigger"},
            {ControllerInteraction.ControllerElements.GripLeft, "handgrip"},
            {ControllerInteraction.ControllerElements.GripRight, "handgrip"},
            {ControllerInteraction.ControllerElements.Touchpad, "trackpad"},
            {ControllerInteraction.ControllerElements.ButtonOne, ""},
            {ControllerInteraction.ControllerElements.ButtonTwo, "menu_button"},
            {ControllerInteraction.ControllerElements.StartMenu, ""}
        };
        
        public static Dictionary<ControllerInteraction.ControllerElements, string> ViveFocus3ButtonNames = new()
        {
            {ControllerInteraction.ControllerElements.Trigger, "trigger"},
            {ControllerInteraction.ControllerElements.GripLeft, "grip"},
            {ControllerInteraction.ControllerElements.GripRight, "grip"},
            {ControllerInteraction.ControllerElements.Touchpad, "joystick"},
            {ControllerInteraction.ControllerElements.ButtonOne, "button"},
            {ControllerInteraction.ControllerElements.ButtonTwo, "buttonB"},
            {ControllerInteraction.ControllerElements.StartMenu, "buttonC"}
        };

        public static DeviceModel CurrentModel
        {
            get
            {
                if (IsOculus)
                {
                    return DeviceModel.Oculus;
                }

                if (IsVive)
                {
                    return DeviceModel.Vive;
                }

                if (IsWmr)
                {
                    return DeviceModel.WMR;
                }

                return DeviceModel.Unknown;
            }
        }

        public static string GetElementName(ControllerInteraction.ControllerElements controllerElements)
        {
            var elementName = "";

            if (IsVive && !IsViveFocus3)
            {
                elementName = ViveButtonNames[controllerElements];
            }
            else if (IsOculus)
            {
                elementName = OculusButtonNames[controllerElements];
            }
            else if (IsKnuckles)
            {
                elementName = KnucklesButtonNames[controllerElements];
            }
            else if (IsViveFocus3)
            {
                elementName = ViveFocus3ButtonNames[controllerElements];
            }
            else
            {
                elementName = DefaultButtonNames[controllerElements];
            }

            return elementName;
        }

        public static string CurrentDeviceName { get; private set; }

        static DeviceHelper()
        {
            UpdateCurrentDeviceName();
            InputDevices.deviceConnected += _=> UpdateCurrentDeviceName();
            InputDevices.deviceDisconnected += _=> UpdateCurrentDeviceName();
            InputDevices.deviceConfigChanged += _=> UpdateCurrentDeviceName();
        }

        private static void UpdateCurrentDeviceName()
        {
            CurrentDeviceName = InputDevices.GetDeviceAtXRNode(XRNode.Head).name;
        }

        private static bool IsDevice(string name)
        {
            return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(CurrentDeviceName) && CurrentDeviceName.ToLower().Contains(name.ToLower());
        }
        
        public static bool IsOculus => IsDevice("oculus");
        
        public static bool IsIndex => IsDevice("index");
        
        public static bool IsPimax => IsDevice("pimax");

        public static bool IsKnuckles => IsIndex || IsPimax;
        
        public static bool IsVive => IsDevice("vive");
        
        public static bool IsWmr => IsDevice("wmr") || IsDevice("windows");

        public static bool IsViveFocus3 => IsDevice("vive") && IsDevice("focus3");
    }

    public enum DeviceModel
    {
        Oculus,
        Vive,
        Index,
        WMR,
        Unknown
    }
}
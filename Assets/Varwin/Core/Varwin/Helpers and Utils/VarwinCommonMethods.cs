using System;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    public static class VarwinCommonMethods
    {
        public static GameObject GetGrabbedObject(ControllerInteraction.ControllerHand handType)
        {
            var hand = InputAdapter.Instance.PlayerController?.Nodes?.GetControllerReference(handType)?.GameObject;
            if (hand == null)
            {
                return null;
            }

            var controller = InputAdapter.Instance.ControllerInteraction?.Controller?.GetFrom(hand);

            return controller?.GetGrabbedObject();
        }

        public static GameObject GetGrabbedObject()
        {
            return GetGrabbedObject(ControllerInteraction.ControllerHand.Right) ??
                   GetGrabbedObject(ControllerInteraction.ControllerHand.Left);
        }

        public static event Action UpdateGizmos;
        public static event Action<Wrapper> UpdateColliders;

        public static void UpdateTransformGizmos()
        {
            UpdateGizmos?.Invoke();
        }

        public static void UpdateCollidersOnModel(Wrapper wrapper)
        {
            UpdateColliders?.Invoke(wrapper);
        }
    }
}
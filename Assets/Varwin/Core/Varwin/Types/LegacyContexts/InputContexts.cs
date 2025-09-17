using System;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    [Obsolete("This class is obsolete and will be removed. Use UseInteractionContext instead")]
    public class UsingContext
    {
        [Obsolete("This field is obsolete. Use the HandGameObject field")]
        public GameObject GameObject;
        public GameObject HandGameObject;
        public ControllerInteraction.ControllerHand Hand;
    }

    [Obsolete("This class is obsolete and will be removed. Use GrabInteractionContext instead")]
    public class GrabingContext
    {
        [Obsolete("This field is obsolete. Use the HandGameObject field")]
        public GameObject GameObject;
        public GameObject HandGameObject;
        public ControllerInteraction.ControllerHand Hand;
    }
}

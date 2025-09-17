using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin.Public
{
    [Obsolete("Use GrabSettings component instead", false)]
    public class VarwinAttachPoint : MonoBehaviour
    {
        public Transform LeftGrabPoint;
        public Transform RightGrabPoint;

        public void Init(Transform leftGrabPoint, Transform rightGrabPoint)
        {
            LeftGrabPoint = leftGrabPoint;
            RightGrabPoint = rightGrabPoint;
        }
    }
}

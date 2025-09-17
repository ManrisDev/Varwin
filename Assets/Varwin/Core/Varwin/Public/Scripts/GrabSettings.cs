using System;
using UnityEngine;

[Serializable]
public class GrabSettings : MonoBehaviour
{
    public GrabTypes GrabType;

    [Serializable]
    public enum GrabTypes
    {
        ControllerSpecific,
        Hold,
        Toggle
    }

    public ObjectAttachPoint[] AttachPoints;

    public bool DetachCursor;
    
    [Serializable]
    public class ObjectAttachPoint
    {
        public Transform LeftGrabPoint;
        public Transform RightGrabPoint;

        public void Init(Transform leftGrabPoint, Transform rightGrabPoint)
        {
            LeftGrabPoint = leftGrabPoint;
            RightGrabPoint = rightGrabPoint;
        }
    }

    public void Init(Transform left, Transform right)
    {
        GrabType = GrabTypes.ControllerSpecific;
        
        AttachPoints = new ObjectAttachPoint[1];
        AttachPoints[0] = new ObjectAttachPoint();
        AttachPoints[0].Init(left, right);
    }

    public void Init(ObjectAttachPoint[] objectAttachPoints)
    {
        AttachPoints = objectAttachPoints;
    }
}

using UnityEngine;

namespace Varwin.Public
{
    public interface IGrabStartInteractionAware : IVarwinInputAware
    {
        void OnGrabStart(GrabInteractionContext context);
    }

    public interface IGrabEndInteractionAware : IVarwinInputAware
    {
        void OnGrabEnd(GrabInteractionContext context);
    }

    public interface IGrabbingAware : IVarwinInputAware
    {
        void OnHandTransformChanged(Vector3 angularVelocity, Vector3 velocity);
    }

    public interface IGrabPointAware
    {
        Transform GetLeftGrabPoint();
        Transform GetRightGrabPoint();
    }
}
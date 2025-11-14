using UnityEngine;
using Varwin;
using Varwin.Public;
using Varwin.Types.RoomInteractableObject_f2d59f2c999d426cb16b4245588fb336;

public class GrabableObject : MonoBehaviour, IGrabStartInteractionAware
{
    private RoomInteractableObject RoomInteractableObject;

    private void Awake()
    {
        RoomInteractableObject = GetComponentInParent<RoomInteractableObject>();
    }

    public void OnGrabStart(GrabInteractionContext context)
    {
        RoomInteractableObject.ObjectGrabbed(gameObject.name);
    }
}

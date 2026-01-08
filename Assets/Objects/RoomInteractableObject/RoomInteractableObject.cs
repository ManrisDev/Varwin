using UnityEngine;
using Varwin;
using Varwin.Public;

namespace Varwin.Types.RoomInteractableObject_f2d59f2c999d426cb16b4245588fb336
{
    [VarwinComponent(English: "Room Interactable Object", Russian: "Room Interactable Object")]
    public class RoomInteractableObject : VarwinObject
    {
        public delegate void EventHandler(string objectName);

        // Событие, которое будет вызвано при нажатии на объект.
        [LogicEvent(English: "The object has grabed")]
        public event EventHandler Event;

        public void ObjectGrabbed(string objectName)
        {
            Event?.Invoke(objectName);
        }
    }
}

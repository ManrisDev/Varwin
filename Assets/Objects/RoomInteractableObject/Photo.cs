using UnityEngine;
using Varwin;
using Varwin.Public;

public class Photo : MonoBehaviour, IGrabStartInteractionAware
{
    public delegate void EventHandler();

    // Событие, которое будет вызвано при нажатии на объект.
    [LogicEvent(English: "The photo has started to be used")]
    public event EventHandler Event;

    public void OnGrabStart(GrabInteractionContext context)
    {
        Event?.Invoke();
    }
}

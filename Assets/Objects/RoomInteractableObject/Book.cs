using UnityEngine;
using Varwin;
using Varwin.Public;

public class Book : MonoBehaviour, IGrabStartInteractionAware
{
    public delegate void EventHandler();

    // Событие, которое будет вызвано при нажатии на объект.
    [LogicEvent(English: "The book has started to be used")]
    public event EventHandler Event;

    public void OnGrabStart(GrabInteractionContext context)
    {
        Event?.Invoke();
    }
}

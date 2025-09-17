using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    /// <summary>
    /// Класс для передачи контекста взаимодействия с объектом во время прикосновения (touch) к объекту.
    /// </summary>
    public class TouchInteractionContext : InteractionContext
    {
        public TouchInteractionContext(GameObject interactHand, ControllerInteraction.ControllerHand handType) : base(interactHand, handType)
        {
        }
    }
}
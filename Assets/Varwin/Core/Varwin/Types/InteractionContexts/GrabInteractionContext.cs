using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    /// <summary>
    /// Класс для передачи контекста взаимодействия с объектом во время граба.
    /// </summary>
    public class GrabInteractionContext : InteractionContext
    {
        public GrabInteractionContext(GameObject interactHand, ControllerInteraction.ControllerHand hand) : base(interactHand, hand)
        {
        }
    }
}
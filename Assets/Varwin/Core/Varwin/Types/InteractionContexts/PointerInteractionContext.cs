using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    /// <summary>
    /// Контекст взаимодействия поинтера с Pointable Object'ами.
    /// </summary>
    public class PointerInteractionContext : InteractionContext
    {
        public PointerInteractionContext(GameObject interactHand, ControllerInteraction.ControllerHand hand) : base(interactHand, hand)
        {
        }
    }
}
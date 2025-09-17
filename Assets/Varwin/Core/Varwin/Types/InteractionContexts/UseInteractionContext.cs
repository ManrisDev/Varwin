using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    /// <summary>
    /// Класс для передачи контекста взаимодействия с объектом во время использования (use) объекта.
    /// </summary>
    public class UseInteractionContext : InteractionContext
    {
        public UseInteractionContext(GameObject interactHand, ControllerInteraction.ControllerHand handType) : base(interactHand, handType)
        {
        }
    }
}
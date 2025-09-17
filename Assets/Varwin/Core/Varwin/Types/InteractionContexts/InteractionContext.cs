using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin
{
    /// <summary>
    /// Базовый класс для передачи контекста взаимодействия с объектами.
    /// </summary>
    public abstract class InteractionContext
    {
        /// <summary>
        /// GameObject руки, которая производит взаимодействие. Для Destop/AR мода это будет объект-заглушка.
        /// Для XR это будет рут GameObject руки из рига игрока.
        /// </summary>
        public readonly GameObject InteractHand;

        /// <summary>
        /// Тип руки, которая взаимодействует с объектом. Для Desktop/AR это всегда будет Right Hand.
        /// </summary>
        public readonly ControllerInteraction.ControllerHand Hand;

        /// <summary>
        /// Компонент ControllerSelf контроллера, который производит взаимодействие.
        /// </summary>
        public readonly ControllerInteraction.ControllerSelf ControllerSelf;

        public InteractionContext(GameObject interactHand, ControllerInteraction.ControllerHand hand)
        {
            InteractHand = interactHand;
            Hand = hand;

            ControllerSelf = hand == ControllerInteraction.ControllerHand.Right
                ? InputAdapter.Instance?.PlayerController.Nodes.RightHand.Controller
                : InputAdapter.Instance?.PlayerController.Nodes.LeftHand.Controller;
        }

        public override string ToString()
        {
            return $"CONTEXT:\n handType: {Hand}\n handObject: {InteractHand}\n controllerSeld: {ControllerSelf}";
        }
    }
}
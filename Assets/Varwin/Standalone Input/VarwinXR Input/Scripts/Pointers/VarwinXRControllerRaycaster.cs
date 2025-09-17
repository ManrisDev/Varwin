using Varwin.PlatformAdapter;
using Varwin.Raycasters;

namespace Varwin.XR
{
    /// <summary>
    /// Компонент обработки луча-указки.
    /// </summary>
    public class VarwinXRControllerRaycaster : DefaultVarwinRaycaster<VarwinXRInteractableObject>
    {
        /// <summary>
        /// Контроллер.
        /// </summary>
        public VarwinXRController Controller;

        /// <summary>
        /// Оверрайд для дистанции взаимодействия с объектом.
        /// </summary>
        protected override float ObjectInteractDistance => DistancePointer.RaycastDistance;

        /// <summary>
        /// Указатель до объекта.
        /// </summary>
        public VarwinXRDistancePointer DistancePointer;

        /// <summary>
        /// Переопределение руки, к которой принадлежит рейкастер.
        /// </summary>
        protected override ControllerInteraction.ControllerHand Hand => DistancePointer.Controller.IsLeft
            ? ControllerInteraction.ControllerHand.Left
            : ControllerInteraction.ControllerHand.Right;
    }
}
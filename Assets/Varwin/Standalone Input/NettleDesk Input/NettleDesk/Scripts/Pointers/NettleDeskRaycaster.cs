using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Raycasters;

namespace Varwin.NettleDesk
{
    /// <summary>
    /// Рейкастер для платформы NettleDesk.
    /// </summary>
    public class NettleDeskRaycaster : DefaultVarwinRaycaster<NettleDeskInteractableObject>
    {
        /// <summary>
        /// Кандидат на взятие в руку.
        /// </summary>
        public NettleDeskInteractableObject HoveredInteractableObject { get; private set; }

        /// <summary>
        /// <inheritdoc cref="DefaultVarwinRaycaster{TInteractable}.RaycastAndOverlap"/>
        /// </summary>
        public override void RaycastAndOverlap(Vector3 origin, Vector3 direction, Vector3 overlapPosition, float overlapRadius, float? raycastDistanceOverride = null,
            float? interactDistanceOverride = null)
        {
            base.RaycastAndOverlap(origin, direction, overlapPosition, overlapRadius, raycastDistanceOverride, interactDistanceOverride);
            HoveredInteractableObject = NearInteractableObject;
        }

        /// <summary>
        /// Переопределение руки, к которой принадлежит рейкастейр.
        /// </summary>
        protected override ControllerInteraction.ControllerHand Hand => _controllerHand;

        /// <summary>
        /// Рука, к которой принадлежит рейкастейр.
        /// </summary>
        [SerializeField] private ControllerInteraction.ControllerHand _controllerHand = ControllerInteraction.ControllerHand.Right;
    }
}
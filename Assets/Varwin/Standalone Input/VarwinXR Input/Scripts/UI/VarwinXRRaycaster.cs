using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Varwin.XR
{
    /// <summary>
    /// Компонент, который необходимо добавить на канвас для работы взаимодействия.
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class VarwinXRRaycaster : GraphicRaycaster
    {
        /// <summary>
        /// Канвас.
        /// </summary>
        private Canvas _canvas;
        
        /// <summary>
        /// Камера для обработки событий.
        /// </summary>
        private Camera _eventCamera;

        /// <summary>
        /// Камера для обработки событий.
        /// </summary>
        public override Camera eventCamera => _eventCamera;

        /// <summary>
        /// Инициализация.
        /// </summary>
        protected override void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        /// <summary>
        /// Проверка столкновения с объектами.
        /// </summary>
        /// <param name="eventData">Аргументы события.</param>
        /// <param name="resultAppendList">Список объектов, с которыми произошло столкновение.</param>
        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (!(eventData is VarwinXREventData varwinXREventData))
            {
                return;
            }

            var graphics = GraphicRegistry.GetGraphicsForCanvas(_canvas);

            for (var index = 0; index < graphics.Count; index++)
            {
                var graphic = graphics[index];
                if (!graphic.raycastTarget || graphic.canvasRenderer.cull || graphic.depth == -1)
                {
                    continue;
                }

                if (!Raycast(varwinXREventData.ControllerRay, graphic.rectTransform, out var worldPosition, out var worldNormal))
                {
                    continue;
                }

                Vector2 screenPos = varwinXREventData.Camera.WorldToScreenPoint(worldPosition);

                if (!graphic.Raycast(screenPos, varwinXREventData.Camera))
                {
                    continue;
                }

                var raycastResult = new RaycastResult();
                raycastResult.worldPosition = worldPosition;
                raycastResult.gameObject = graphic.gameObject;
                raycastResult.worldNormal = worldNormal;
                raycastResult.screenPosition = screenPos;
                raycastResult.depth = graphic.depth;
                raycastResult.sortingLayer = _canvas.sortingLayerID;
                raycastResult.sortingOrder = _canvas.sortingOrder;
                _eventCamera = varwinXREventData.Camera;
                raycastResult.module = this;
                resultAppendList.Add(raycastResult);
            }
            
            resultAppendList.Sort((g1, g2) => g2.depth.CompareTo(g1.depth));
        }

        /// <summary>
        /// Проверка столкновения с объектами UI.
        /// </summary>
        /// <param name="ray">Луч.</param>
        /// <param name="rectTransform">Компонент UI.</param>
        /// <param name="worldPosition">Мировая позиция курсора на объекте.</param>
        /// <param name="worldNormal">Мировой перпендикуляр к поверхности.</param>
        /// <returns>Истина, если столкновение произошло.</returns>
        private bool Raycast(Ray ray, RectTransform rectTransform, out Vector3 worldPosition, out Vector3 worldNormal)
        {
            var corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            var plane = new Plane(corners[0], corners[1], corners[2]);

            if (!plane.Raycast(ray, out var enter))
            {
                worldPosition = Vector2.zero;
                worldNormal = Vector3.zero;
                return false;
            }

            var intersectionPoint = ray.GetPoint(enter);
            var bottomEdge = corners[3] - corners[0];
            var leftEdge = corners[1] - corners[0];
            var bottomDot = Vector3.Dot(intersectionPoint - corners[0], bottomEdge);
            var leftDot = Vector3.Dot(intersectionPoint - corners[0], leftEdge);
            if (bottomDot < bottomEdge.sqrMagnitude && leftDot < leftEdge.sqrMagnitude && bottomDot >= 0 && leftDot >= 0)
            {
                worldPosition = corners[0] + leftDot * leftEdge / leftEdge.sqrMagnitude + bottomDot * bottomEdge / bottomEdge.sqrMagnitude;
                worldNormal = plane.normal;
                return true;
            }

            worldPosition = Vector3.zero;
            worldNormal = Vector3.zero;
            return false;
        }
    }
}
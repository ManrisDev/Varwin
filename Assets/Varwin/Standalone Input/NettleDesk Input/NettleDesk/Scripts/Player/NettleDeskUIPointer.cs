using UnityEngine;
using Varwin.NettleDesk;
using Varwin.Raycasters;

namespace Varwin.NettleDeskPlayer
{
    public class NettleDeskUIPointer : DefaultVarwinUIPointer
    {
        /// <summary>
        /// Дистанция рейкаста.
        /// </summary>
        public const float CastDistance = 50f;

        /// <summary>
        /// Камера. Используется для дебага.
        /// </summary>
        [SerializeField] private Camera _camera;

        /// <summary>
        /// Переопределение рейкастера для базового класса поинтера.
        /// </summary>
        protected override IPointableRaycaster Raycaster => _raycaster;

        /// <summary>
        /// Рейкастер для NettleDesk.
        /// </summary>
        [SerializeField] private NettleDeskRaycaster _raycaster;

        /// <summary>
        /// Можно ли провести интеракцию.
        /// </summary>
        /// <returns></returns>
        public bool CanClick() => HoveredPointableObject;

        /// <summary>
        /// Контроллер взаимодействия NettleDesk. Используется для дебага.
        /// </summary>
        public NettleDeskInteractionController NettleDeskInteractionController;

        /// <summary>
        /// Обновление состояния поинтера.
        /// </summary>
        public void FixedUpdate()
        {
            if (NettleDeskSettings.StylusSupport)
            {
                return;
            }

            UpdateState();

#if UNITY_EDITOR
            var ray = _camera.ScreenPointToRay(NettleDeskInteractionController.CursorPos);
            Debug.DrawRay(ray.origin, ray.direction, Color.red);
#endif
        }
    }
}
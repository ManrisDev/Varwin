using System;
using UnityEngine;
using UnityEngine.XR;

namespace Varwin.XR
{
    /// <summary>
    /// Контроллер управления игроком.
    /// </summary>
    public class VarwinXRPlayerMoveController : LocomotionController
    {
        /// <summary>
        /// Объект рига.
        /// </summary>
        [SerializeField] private Transform _rig;
        
        /// <summary>
        /// Объект сдвига камеры.
        /// </summary>
        [SerializeField] private Transform _offsetCameraObject;

        /// <summary>
        /// Есть ли запрос на прыжок.
        /// </summary>
        private bool _isJumping = false;
        
        /// <summary>
        /// Текущее перемещение.
        /// </summary>
        private Vector2 _movement = Vector2.zero;
        
        /// <summary>
        /// Текущее перемещение.
        /// </summary>
        protected override Vector2 Movement => _movement;
        
        /// <summary>
        /// Скорость перемещения.
        /// </summary>
        protected override float Speed => PlayerManager.SprintSpeed;
        
        /// <summary>
        /// Есть ли запрос на прыжок.
        /// </summary>
        protected override bool IsJumping => _isJumping;

        /// <summary>
        /// Высота игрока.
        /// </summary>
        protected override float Height => Camera.transform.localPosition.y + _offsetCameraObject.localPosition.y;

        /// <summary>
        /// Функция, которая приводит вектор к локальному вектору рига. 
        /// </summary>
        /// <param name="direction">Исходное направление.</param>
        /// <returns>Приведенное направление.</returns>
        protected override Vector3 TransformDirection(Vector3 direction)
        {
            return direction;
        }

        /// <summary>
        /// Применение скорости к ригу.
        /// </summary>
        /// <param name="movement">Вектор перемещения игрока.</param>
        protected override void ApplyMovement(Vector3 movement)
        {
            SetPosition(GetPosition() + movement);
        }

        /// <summary>
        /// Задать позицию рига.
        /// </summary>
        /// <param name="position">Позиция.</param>
        public void SetRigPosition(Vector3 position)
        {
            SetPosition(position);
        }

        /// <summary>
        /// Задать позицию объекта.
        /// </summary>
        /// <param name="position">Позиция.</param>
        private void SetPosition(Vector3 position)
        {
            var playerPosition = position;
            var headDelta = _rig.transform.position - Camera.transform.position;

            headDelta.y = 0;
            playerPosition += headDelta;
            _rig.transform.position = playerPosition;
        }

        /// <summary>
        /// Получение позиции объекта.
        /// </summary>
        /// <returns>Позиция объекта.</returns>
        public Vector3 GetPosition()
        {
            return new(Camera.transform.position.x, _rig.transform.position.y, Camera.transform.position.z);
        }

        /// <summary>
        /// Задает текущее перемещение игрока.
        /// </summary>
        /// <param name="movement">Перемещение.</param>
        public void SetMovement(Vector2 movement)
        {
            _movement = movement;
        }

        /// <summary>
        /// Задает состояние прыжка.
        /// </summary>
        /// <param name="isJumping">Прыгает ли.</param>
        public void SetJumpingState(bool isJumping)
        {
            _isJumping = isJumping;
        }
    }
}
using System;
using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Public;
using Varwin.SocketLibrary;

namespace Varwin
{
    /// <summary>
    /// Контроллер управления перемещением игрока.
    /// </summary>
    public abstract class LocomotionController : MonoBehaviour
    {
        /// <summary>
        /// Количество возможный столкновений.
        /// </summary>
        private const int CastCount = 16;
                
        /// <summary>
        /// Максимальная длина raycast'а.
        /// </summary>
        private const float MaxRaycastDistance = 1000f;

        /// <summary>
        /// Минимальный рост игрока.
        /// </summary>
        private const float MinHeight = 0.1f;
        
        /// <summary>
        /// Столкновения луча с поверхностями.
        /// </summary>
        private readonly RaycastHit[] _hits = new RaycastHit[CastCount];
        
        /// <summary>
        /// Радиус тела.
        /// </summary>
        [SerializeField] private float _radius = 0.1f;
        
        /// <summary>
        /// Компонент-камеры головы игрока.
        /// </summary>
        [SerializeField] private Camera _camera;
        
        /// <summary>
        /// Маска для игнорирования поиска маршрута.
        /// </summary>
        [SerializeField] private LayerMask _raycastIgnoreMask;
        
        /// <summary>
        /// Скорость перемещения.
        /// </summary>
        private Vector3 _velocity = Vector3.zero;

        /// <summary>
        /// Скалярная скорость перемещения.
        /// </summary>
        private float _movementSpeed = 0f;
        
        /// <summary>
        /// Может ли игрок падать. Игрок может падать в случае, если есть хотя бы одна TeleportArea.
        /// </summary>
        private bool _canFalling = false;
        
        /// <summary>
        /// Платформа, на которой игрок может передвигаться.
        /// </summary>
        private Collider _platform = null;
        
        /// <summary>
        /// Предыдущая платформа для отсчета скорости перемещения.
        /// </summary>
        private Collider _previousPlatform = null;
        
        /// <summary>
        /// Предыдущее положение платформы.
        /// </summary>
        private Matrix4x4 _oldPlatformMatrix;

        /// <summary>
        /// Компонент-камеры головы игрока.
        /// </summary>
        public Camera Camera => _camera;
        
        /// <summary>
        /// Использовать ли гравитацию.
        /// </summary>
        public bool UseGravity => PlayerManager.UseGravity;
        
        /// <summary>
        /// Проверять перемещение по teleportArea. 
        /// </summary>
        public bool CheckTeleportArea => PlayerManager.MovementTeleportAreaOnly;
        
        /// <summary>
        /// Находится ли игрок на земле.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// Текущее перемещение в горизонтальной плоскости.
        /// </summary>
        protected abstract Vector2 Movement { get; }
        
        /// <summary>
        /// Текущая скорость перемещения игрока.
        /// </summary>
        protected abstract float Speed { get; }
        
        /// <summary>
        /// Вызвано ли событие прыжка.
        /// </summary>
        protected abstract bool IsJumping { get; }
        
        /// <summary>
        /// Рост игрока.
        /// </summary>
        protected abstract float Height { get; }

        /// <summary>
        /// Обновление скорости и применение перемещения игрока.
        /// </summary>
        private void Update()
        {
            UpdateMovement();
            UpdateGravity();
            UpdatePlatform();
            UpdateJumping();
            UpdateMotion();
            UpdateTeleportAreaMovement();
            ApplyMovement(_velocity * Time.deltaTime);
            OnUpdate();
        }

        /// <summary>
        /// Обновление логики locomotion.
        /// </summary>
        protected virtual void OnUpdate()
        {
            
        }

        /// <summary>
        /// Метод, ограничивающий перемещение игрока по TeleportArea.
        /// </summary>
        private void UpdateTeleportAreaMovement()
        {
            _velocity = GetValidVelocity();
        }

        /// <summary>
        /// Возвращает скорость игрока, которая при перемещении будет перемещать игрока в teleportArea. 
        /// </summary>
        /// <returns>Скорость.</returns>
        private Vector3 GetValidVelocity()
        {
            var radius = GetClampedRadius();
            var footPoint = _camera.transform.position + Vector3.down * (GetClampedHeight() * 0.66f - radius);
            var startVelocity = new Vector3(0, _velocity.y, 0);
            var lastVelocity = startVelocity;

            for (var i = 0; i < CastCount; i++)
            {
                var t = Mathf.InverseLerp(0, CastCount - 1, i);
                var localPoint = Vector3.Lerp(startVelocity, _velocity, t);
                var point = footPoint + localPoint * Time.deltaTime;

                var count = Physics.SphereCastNonAlloc(point, radius, Vector3.down, _hits, MaxRaycastDistance, ~_raycastIgnoreMask, QueryTriggerInteraction.Ignore);

                if (count == 0)
                {
                    return lastVelocity;
                }

                lastVelocity = localPoint;
            }

            return lastVelocity;
        }
        
        /// <summary>
        /// Метод, обновляющий состояние платформы.
        /// </summary>
        private void UpdatePlatform()
        {
            if (!_platform)
            {
                _previousPlatform = null;
                _oldPlatformMatrix = Matrix4x4.identity;
                return;
            }

            var newMatrix = _platform.transform.localToWorldMatrix;

            if (_previousPlatform != _platform)
            {
                _oldPlatformMatrix = newMatrix;
                _previousPlatform = _platform;
                return;
            }

            var playerPosition = _camera.transform.position;
            var localPlayerPosition = _oldPlatformMatrix.inverse.MultiplyPoint(playerPosition);
            var newPlayerPosition = newMatrix.MultiplyPoint(localPlayerPosition);
            var deltaPosition = newPlayerPosition - playerPosition;

            _velocity += deltaPosition / Time.deltaTime;
            _oldPlatformMatrix = newMatrix;
            _previousPlatform = _platform;
        }
        
        /// <summary>
        /// Метод обновления гравитации исходя из состояния прыжка.
        /// </summary>
        private void UpdateJumping()
        {
            if (!IsGrounded || !IsJumping)
            {
                return;
            }
            
            _velocity.y = Mathf.Sqrt(PlayerManager.JumpHeight * -3f * Physics.gravity.y);
        }

        /// <summary>
        /// Метод, обновляющий скорость игрока исходя из ввода.
        /// </summary>
        private void UpdateMovement()
        {
            if (PlayerManager.MovementEnabled && PlayerManager.WasdMovementEnabled)
            {
                _movementSpeed = Mathf.Lerp(_movementSpeed, Speed, Time.deltaTime * 10f);

                var offset = Movement;
                var positionOffset = new Vector3(offset.x, 0.0f, offset.y);
                var velocity = TransformDirection(positionOffset) * _movementSpeed;

                _velocity.x = velocity.x;
                _velocity.z = velocity.z;
            }
            else
            {
                _velocity.x = 0;
                _velocity.z = 0;
            }
        }

        /// <summary>
        /// Метод, обновляющий гравитацию в объекте.
        /// </summary>
        private void UpdateGravity()
        {
            IsGrounded = false;
            _platform = null;

            if (!UseGravity)
            {
                _velocity.y = 0;
                return;
            }

            var headPosition = _camera.transform.position;
            var height = GetClampedHeight();
            var radius = GetClampedRadius();
            var count = Physics.SphereCastNonAlloc(headPosition, radius, Vector3.down, _hits, MaxRaycastDistance, ~_raycastIgnoreMask, QueryTriggerInteraction.Ignore);

            if (count == 0)
            {
                _velocity.y = 0;
                return;
            }

            _canFalling = false;
            for (var i = 0; i < count; i++)
            {
                var hit = _hits[i];
                var deltaPos = headPosition - hit.point;

                if (hit.collider.CompareTag("TeleportArea"))
                {
                    _canFalling = true;
                }
                else if (CheckTeleportArea)
                {
                    continue;
                }

                if (deltaPos.magnitude > height || IsGrounded)
                {
                    continue;
                }

                if (IsPartOfGrabbedObject(hit.collider))
                {
                    continue;
                }

                if (Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
                {
                    _platform = hit.collider;
                }
                
                _velocity.y = (height - deltaPos.magnitude) * 10f;
                IsGrounded = true;
            }

            if (_canFalling && !IsGrounded && UseGravity)
            {
                _velocity += Physics.gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Метод, расчитывающий скольжение вдоль поверхностей.
        /// </summary>
        private void UpdateMotion()
        {
            var height = GetClampedHeight();
            var radius = GetClampedRadius();
            var headPoint = _camera.transform.position + Vector3.down * radius;
            var footPoint = _camera.transform.position + Vector3.down * (height * 0.66f - radius);
            var castDistance = _velocity.magnitude * Time.deltaTime + radius;
            var count = Physics.CapsuleCastNonAlloc(headPoint, footPoint, radius, _velocity.normalized, _hits, castDistance, ~_raycastIgnoreMask, QueryTriggerInteraction.Ignore);
            var velocityY = _velocity.y;

            for (var i = 0; i < count; i++)
            {
                var hit = _hits[i];
                
                if (IsPartOfGrabbedObject(hit.collider))
                {
                    continue;
                }

                if (Vector3.Dot(hit.normal, _velocity.normalized) > 0)
                {
                    continue;
                }
                
                _velocity = Vector3.ProjectOnPlane(_velocity, hit.normal);
            }

            if (UseGravity)
            {
                _velocity.y = _velocity.y > velocityY ? velocityY : _velocity.y;
            }
            else
            {
                _velocity.y = 0;
            }

            count = Physics.CapsuleCastNonAlloc(headPoint, footPoint, radius, _velocity.normalized, _hits, _velocity.magnitude * Time.deltaTime, ~_raycastIgnoreMask, QueryTriggerInteraction.Ignore);

            if (count <= 0)
            {
                return;
            }
            
            _velocity.x = 0;
            _velocity.z = 0;
        }

        /// <summary>
        /// Является ли объект частью цепочки объектов.
        /// </summary>
        /// <param name="targetCollider">Целевой объект.</param>
        /// <returns>Истина, если является.</returns>
        private bool IsPartOfGrabbedObject(Collider targetCollider)
        {
            if (!targetCollider.attachedRigidbody)
            {
                return false;
            }

            var leftGrabbedObject = InputAdapter.Instance?.PlayerController?.Nodes?.LeftHand?.Controller.GetGrabbedObject();
            var rightGrabbedObject = InputAdapter.Instance?.PlayerController?.Nodes?.RightHand?.Controller.GetGrabbedObject();

            if (!leftGrabbedObject && !rightGrabbedObject)
            {
                return false;
            }
            
            var result = false;
            if (leftGrabbedObject)
            {
                result = IsPartOfGrabbedChain(leftGrabbedObject, targetCollider);
            }

            if (rightGrabbedObject)
            {
                result = IsPartOfGrabbedChain(rightGrabbedObject, targetCollider);
            }

            return result;
        }

        /// <summary>
        /// Является ли объект частью грабнутого объекта.
        /// </summary>
        /// <param name="grabbedObject">Взятый объекты.</param>
        /// <param name="targetCollider">Целевой коллайдер.</param>
        /// <returns>Истина, если является.</returns>
        private bool IsPartOfGrabbedChain(GameObject grabbedObject, Collider targetCollider)
        {
            if (grabbedObject.GetComponent<Rigidbody>() == targetCollider.attachedRigidbody)
            {
                return true;
            }

            var grabbedObjectController = grabbedObject.gameObject.GetWrapper()?.GetObjectController();
            var targetObjectController = targetCollider.gameObject.GetWrapper()?.GetObjectController();
            if (targetObjectController == null || grabbedObjectController == null)
            {
                return false;
            }
            
            if (grabbedObjectController == targetObjectController)
            {
                return true;
            }

            var socketController = grabbedObjectController.gameObject.GetComponent<SocketController>();
            if (socketController)
            {
                var result = false;
                socketController.ConnectionGraphBehaviour.ForEach(a => result |= a.gameObject.GetWrapper().GetObjectController() == targetObjectController);

                if (result)
                {
                    return true;
                }
            }

            return grabbedObjectController.LockChildren && grabbedObjectController.Descendants.Contains(targetObjectController);
        }

        /// <summary>
        /// Сброс скорости перемещения.
        /// </summary>
        public void ResetVelocity()
        {
            _velocity = Vector3.zero;
        }

        /// <summary>
        /// Получение ограниченной высоты игрока.
        /// </summary>
        /// <returns>Высота игрока.</returns>
        protected float GetClampedHeight()
        {
            return Mathf.Clamp(Height, MinHeight, Mathf.Infinity);
        }
        
        /// <summary>
        /// Получение ограниченной радиус игрока.
        /// </summary>
        /// <returns>Радиус игрока.</returns>
        protected float GetClampedRadius()
        {
            return Mathf.Clamp(_radius, MinHeight / 4f, GetClampedHeight() / 4f);
        }
        
        /// <summary>
        /// Функция, которая приводит вектор к локальному вектору рига. 
        /// </summary>
        /// <param name="direction">Исходное направление.</param>
        /// <returns>Приведенное направление.</returns>
        protected abstract Vector3 TransformDirection(Vector3 direction);
        
        /// <summary>
        /// Применение скорости к ригу.
        /// </summary>
        /// <param name="movement">Вектор перемещения игрока.</param>
        protected abstract void ApplyMovement(Vector3 movement);
    }
}
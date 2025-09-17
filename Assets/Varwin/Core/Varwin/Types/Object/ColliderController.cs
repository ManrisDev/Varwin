using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;

namespace Varwin
{
    /// <summary>
    /// Контроллер столкновений для интерфейса IColliderAware.
    /// </summary>
    public class ColliderController : MonoBehaviour
    {
        /// <summary>
        /// Интерфейс.
        /// </summary>
        private IColliderAware _colliderAware;

        /// <summary>
        /// Список столкновений с Wrapper'ами.
        /// </summary>
        public List<KeyValueContainer<Wrapper, List<KeyValueContainer<Collider, Collider>>>> _collisions = new();
        
        /// <summary>
        /// Вошедшие объекты.
        /// </summary>
        private List<Wrapper> _enteredWrappers = new();
        
        /// <summary>
        /// Вышедшие объекты.
        /// </summary>
        private List<Wrapper> _exitedWrappers = new();
        
        /// <summary>
        /// Механизм обработки столкновений.
        /// </summary>
        private CollisionDetector _collisionDetector;

        /// <summary>
        /// Дочерние коллайдеры.
        /// </summary>
        private Collider[] _childColliders;

        /// <summary>
        /// Инициализация и подписка.
        /// </summary>
        private void Awake()
        {
            var parentRigidBody = gameObject.GetComponentInParent<Rigidbody>(true);
            if (!parentRigidBody)
            {
                return;
            }

            _collisionDetector = parentRigidBody.gameObject.AddComponent<CollisionDetector>();
            _collisionDetector.CollisionEntered += CollisionEnter;
            _collisionDetector.CollisionExited += CollisionExit;
            UpdateColliders();
        }

        /// <summary>
        /// Вызов событий и анализ текщуих столкновений.
        /// </summary>
        private void Update()
        {
            if (_collisions.Count > 0)
            {
                UpdateCollisions();
            }
            
            if (_enteredWrappers.Count > 0)
            {
                _colliderAware.OnObjectEnter(_enteredWrappers.ToArray());
                _enteredWrappers.Clear();
            }
            
            if (_exitedWrappers.Count > 0)
            {
                _colliderAware.OnObjectExit(_exitedWrappers.ToArray());
                _exitedWrappers.Clear();
            }
        }

        /// <summary>
        /// Обновление состояния столкновений.
        /// </summary>
        private void UpdateCollisions()
        {
            for (int i = _collisions.Count - 1; i >= 0; i--)
            {
                var collision = _collisions[i];

                for (int j = collision.Value.Count - 1; j >= 0; j--)
                {
                    var selfCollider = collision.Value[j].Key;
                    var otherCollider = collision.Value[j].Value;
                    if (IsActiveCollider(selfCollider) && IsActiveCollider(otherCollider))
                    {
                        continue;
                    }

                    collision.Value.RemoveAt(j);
                }

                if (collision.Value.Count == 0)
                {
                    _collisions.RemoveAt(i);
                    _exitedWrappers.Add(collision.Key);
                }
            }
        }

        /// <summary>
        /// Является ли коллайдер активным.
        /// </summary>
        /// <param name="collider">Коллайдер.</param>
        /// <returns>Истина, если является.</returns>
        private bool IsActiveCollider(Collider collider)
        {
            return collider && collider.enabled && collider.gameObject.activeInHierarchy;
        }
        
        /// <summary>
        /// Метод, вызываемый при столкновении.
        /// При столкновении проверка принадлежности объекта к дочерним.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        private void CollisionEnter(Collider other)
        {
            var wrapper = other.gameObject.GetWrapper();
            if (wrapper == null || wrapper is NullWrapper)
            {
                return;
            }

            var collisionsWithWrapper = _collisions.Find(a => a.Key == wrapper);

            if (collisionsWithWrapper != null)
            {
                AddColliderToList(other, collisionsWithWrapper);
            }
            else
            {
                AddNewCollision(other, wrapper);
            }
        }

        /// <summary>
        /// Создание нового столкновения.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        /// <param name="wrapper">Другой объект.</param>
        private void AddNewCollision(Collider other, Wrapper wrapper)
        {
            foreach (var childCollider in _childColliders)
            {
                if (!IsCollided(childCollider, other))
                {
                    continue;
                }

                var collisions = new List<KeyValueContainer<Collider, Collider>> {new(childCollider, other)};
                _collisions.Add(new KeyValueContainer<Wrapper, List<KeyValueContainer<Collider, Collider>>>(wrapper, collisions));
                _enteredWrappers.Add(wrapper);
            }
        }

        /// <summary>
        /// Добавление коллайдера в список если коллайдер, с которым он столкнулся является дочерним.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        /// <param name="collisionsWithWrapper">Список столкновений объекта.</param>
        private void AddColliderToList(Collider other, KeyValueContainer<Wrapper, List<KeyValueContainer<Collider, Collider>>> collisionsWithWrapper)
        {
            foreach (var childCollider in _childColliders)
            {
                var collision = collisionsWithWrapper.Value.Find(a => a.Key == childCollider && a.Value == other);
                if (collision != null)
                {
                    continue;
                }

                if (!IsCollided(childCollider, other))
                {
                    continue;
                }

                collisionsWithWrapper.Value.Add(new KeyValueContainer<Collider, Collider>(childCollider, other));
                break;
            }
        }

        /// <summary>
        /// Метод, вызываемый при выходе из коллизии.
        /// При выходе из колизии проверка принадлежности к дочерним объектам.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        private void CollisionExit(Collider other)
        {
            var wrapper = other.gameObject.GetWrapper();
            if (wrapper == null || wrapper is NullWrapper)
            {
                return;
            }

            var collisionsWithWrapper = _collisions.Find(a => a.Key == wrapper);

            var collision = collisionsWithWrapper?.Value.Find(a => a.Value = other);
            if (collision != null && !IsCollided(collision.Key, collision.Value))
            {
                collisionsWithWrapper.Value.Remove(collision);
                if (collisionsWithWrapper.Value.Count == 0)
                {
                    _exitedWrappers.Add(wrapper);
                    _collisions.Remove(collisionsWithWrapper);
                }
            }
        }

        /// <summary>
        /// Обновление списка коллайдеров.
        /// </summary>
        public void UpdateColliders()
        {
            _childColliders = GetComponentsInChildren<Collider>(true);
        }

        /// <summary>
        /// Задать интерфейс обработки столкновений.
        /// </summary>
        /// <param name="colliderAware">Интерфейс.</param>
        public void SetColliderAware(IColliderAware colliderAware)
        {
            _colliderAware = colliderAware;
        }

        /// <summary>
        /// Столкнулись ли объекты в настоящий момент.
        /// </summary>
        /// <param name="self">Первый объект.</param>
        /// <param name="other">Другой объект.</param>
        /// <returns>Истина, если столкнулись.</returns>
        private bool IsCollided(Collider self, Collider other)
        {
            var otherTransform = other.transform;
            var selfTransform = self.transform;
            var contactVector = (otherTransform.position - selfTransform.position).normalized * Physics.defaultContactOffset;

            return Physics.ComputePenetration(self, selfTransform.position + contactVector,
                selfTransform.rotation, other, otherTransform.position - contactVector,
                otherTransform.rotation, out var direction, out var distance);
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            if (!_collisionDetector)
            {
                return;
            }

            _collisionDetector.CollisionEntered -= CollisionEnter;
            _collisionDetector.CollisionExited -= CollisionExit;

            Destroy(_collisionDetector);
        }
    }
}
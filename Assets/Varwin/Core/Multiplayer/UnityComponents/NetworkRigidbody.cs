using Unity.Netcode;
using UnityEngine;

namespace Varwin.Multiplayer
{
    /// <summary>
    /// Синхронизатор твердого тела.
    /// </summary>
    public class NetworkRigidbody : NetworkBehaviour
    {
        /// <summary>
        /// Твердое тело.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// Является ли кинмеатичным.
        /// </summary>
        public NetworkVariable<bool> IsKinematic = new();

        /// <summary>
        /// Подвержен ли гравитации.
        /// </summary>
        public NetworkVariable<bool> IsGravity = new();

        /// <summary>
        /// Инициализация.
        /// </summary>
        /// <param name="rigidbody">Твердое тело.</param>
        public void Initialize(Rigidbody rigidbody)
        {
            _rigidbody = rigidbody;
            if (IsServer)
            {
                IsGravity.Value = _rigidbody.useGravity;
                IsKinematic.Value = _rigidbody.isKinematic;
            }
            else
            {
                IsGravity.OnValueChanged += OnGravityValueChanged;
                IsKinematic.OnValueChanged += OnKinematicValueChanged;
            }
        }

        /// <summary>
        /// При изменении кинематичности тела.
        /// </summary>
        /// <param name="previousValue">Предыдущее значение.</param>
        /// <param name="newValue">Новое значение.</param>
        private void OnKinematicValueChanged(bool previousValue, bool newValue)
        {
            _rigidbody.isKinematic = newValue;
        }

        /// <summary>
        /// При изменении гравитационности тела.
        /// </summary>
        /// <param name="previousValue">Предыдущее значение.</param>
        /// <param name="newValue">Новое значение.</param>
        private void OnGravityValueChanged(bool previousValue, bool newValue)
        {
            _rigidbody.useGravity = newValue;
        }

        /// <summary>
        /// Обновление значений переменных.
        /// </summary>
        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (_rigidbody.isKinematic != IsKinematic.Value)
            {
                IsKinematic.Value = _rigidbody.isKinematic;
            }

            if (_rigidbody.useGravity != IsGravity.Value)
            {
                IsGravity.Value = _rigidbody.useGravity;
            }
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        public override void OnDestroy()
        {
            if (!IsServer)
            {
                IsGravity.OnValueChanged -= OnGravityValueChanged;
                IsKinematic.OnValueChanged -= OnKinematicValueChanged;
            }
        }
    }
}
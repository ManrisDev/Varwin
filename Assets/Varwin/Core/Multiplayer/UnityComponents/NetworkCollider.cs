using System;
using Unity.Netcode;
using UnityEngine;

namespace Varwin.Multiplayer
{
    /// <summary>
    /// Синхронизация коллайдера.
    /// </summary>
    public class NetworkCollider : NetworkBehaviour
    {
        /// <summary>
        /// Состояние триггерности коллайдера.
        /// </summary>
        public NetworkVariable<bool> IsTrigger = new();
        
        /// <summary>
        /// Коллайдер.
        /// </summary>
        private Collider _collider;

        /// <summary>
        /// Инициализация компонента.
        /// </summary>
        /// <param name="collider">Целевой коллайдер.</param>
        public void Initialize(Collider collider)
        {
            _collider = collider;
            
            if (IsServer)
            {
                IsTrigger.Value = _collider.isTrigger;
            }
            else
            {
                IsTrigger.OnValueChanged += OnTriggerValueChanged;
            }
        }

        /// <summary>
        /// При изменении значения триггерности.
        /// </summary>
        /// <param name="previousValue">Предыдущее значение.</param>
        /// <param name="newValue">Новое значение.</param>
        private void OnTriggerValueChanged(bool previousValue, bool newValue)
        {
            _collider.isTrigger = newValue;
        }

        /// <summary>
        /// Обновление параметров на сервере.
        /// </summary>
        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (_collider.isTrigger != IsTrigger.Value)
            {
                IsTrigger.Value = _collider.isTrigger;
            }
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        public override void OnDestroy()
        {
            if (!IsServer)
            {
                IsTrigger.OnValueChanged -= OnTriggerValueChanged;
            }
        }
    }
}
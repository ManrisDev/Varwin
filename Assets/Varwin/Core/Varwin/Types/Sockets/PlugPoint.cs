using UnityEngine;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Класс, описывающий работу вилки.
    /// </summary>
    public class PlugPoint : JointPoint
    {
        /// <summary>
        /// Ключ подключения.
        /// </summary>
        public string Key = "key";

        /// <summary>
        /// Физический joint.
        /// </summary>
        public Joint Joint { get; internal set; }

        /// <summary>
        /// Может ли точка быть разъединена.
        /// </summary>
        public bool CanDisconnect = true;

        /// <summary>
        /// Предварительная точка соединения.
        /// </summary>
        private SocketPoint _candidateSocketPoint;

        /// <summary>
        /// Инициализация точки.
        /// </summary>
        private void Awake()
        {
            if (SocketController)
            {
                SocketController.AddJointPoint(this);
            }
            
            if (!InstancedPlugPoints.Contains(this))
            {
                InstancedPlugPoints.Add(this);
            }

            gameObject.layer = 2;
        }

        /// <summary>
        /// Обновление состояния в случае обновления состояния кандидата.
        /// </summary>
        private void Update()
        {
            if (!_candidateSocketPoint)
            {
                return;
            }

            if (!SocketController.CanConnecting(_candidateSocketPoint, this) || !_candidateSocketPoint.isActiveAndEnabled)
            {
                SocketController.OnPlugPointTriggerExit(this, _candidateSocketPoint);
                _candidateSocketPoint = null;
            }
        }
        
        /// <summary>
        /// При столкновении с коллайдером.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        private void OnTriggerEnter(Collider other)
        {
            var socket = other.GetComponent<SocketPoint>();
            
            if (!socket || !SocketController.CanConnecting(socket, this))
            {
                return;
            }
            
            if (!socket.SocketController.IsGrabbed && !SocketController.IsGrabbed)
            {
                return;
            }

            if (_candidateSocketPoint)
            {
                SocketController.OnPlugPointTriggerExit(this, _candidateSocketPoint);
                _candidateSocketPoint = null;
            }
            
            SocketController.OnPlugPointTriggerEnter(this, socket);
            _candidateSocketPoint = socket;
        }
        
        /// <summary>
        /// При выходе из коллизии.
        /// </summary>
        /// <param name="other">Другой коллайдер.</param>
        private void OnTriggerExit(Collider other)
        {
            var socket = other.GetComponent<SocketPoint>();

            if (!socket || _candidateSocketPoint != socket)
            {
                return;
            }

            SocketController.OnPlugPointTriggerExit(this, socket);
            _candidateSocketPoint = null;
        }

        /// <summary>
        /// Удаление точки.
        /// </summary>
        private void OnDestroy()
        {
            if (InstancedPlugPoints.Contains(this))
            {
                InstancedPlugPoints.Remove(this);
            }

            if (Joint)
            {
                Destroy(Joint);
            }
        }
    }
}
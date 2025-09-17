using UnityEngine;
using Varwin.SocketLibrary.Extension;
using System.Linq;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Класс дополнительных статических методов SocketController.
    /// </summary>
    public partial class SocketController
    {
        /// <summary>
        /// Может ли подключиться вилка к розетке.
        /// </summary>
        /// <param name="socketPoint">Розетка.</param>
        /// <param name="plugPoint">Вилка.</param>
        /// <returns>Истина, если подключение возможно.</returns>
        public static bool CanConnecting(SocketPoint socketPoint, PlugPoint plugPoint)
        {
            if (!socketPoint || !plugPoint)
            {
                return false;
            }
            
            if (!socketPoint.SocketController 
                || !plugPoint.SocketController 
                || !socketPoint.SocketController.ConnectionGraphBehaviour
                || !plugPoint.SocketController.ConnectionGraphBehaviour)
            {
                return false;
            }

            if (!socketPoint.CanConnect || !plugPoint.CanConnect)
            {
                return false;
            }
            
            var connectingChecking = !socketPoint.SocketController.ConnectionGraphBehaviour.HasChild(plugPoint.SocketController) 
                                     || !plugPoint.SocketController.ConnectionGraphBehaviour.HasChild(socketPoint.SocketController);

            return socketPoint.SocketController != plugPoint.SocketController 
                   && connectingChecking 
                   && !(!plugPoint || !plugPoint.IsFree || !socketPoint || !socketPoint.IsFree) 
                   && socketPoint.AvailableKeys.Contains(plugPoint.Key);
        }

        /// <summary>
        /// Отключить указанную точку.
        /// </summary>
        /// <param name="jointPoint">Точка.</param>
        public static void Disconnect(JointPoint jointPoint)
        {
            if (jointPoint.IsFree)
            {
                return;
            }

            var connectedJointPoint = jointPoint.ConnectedPoint;
            connectedJointPoint.SocketController.ResetState();

            jointPoint.ConnectedPoint.ConnectedPoint = null;
            jointPoint.ConnectedPoint = null;

            jointPoint.SocketController.PreviewBehaviour.transform.parent = null;
            jointPoint.SocketController.ConnectionGraphBehaviour.UpdateAllTrees();
            connectedJointPoint.SocketController.ConnectionGraphBehaviour.UpdateAllTrees();

            jointPoint.SocketController.ResetState();

            Destroy(jointPoint is SocketPoint ? ((PlugPoint) connectedJointPoint).Joint : ((PlugPoint) jointPoint).Joint);

            if (jointPoint is SocketPoint)
            {
                var socketPoint = ((SocketPoint) jointPoint);
                var plugPoint = ((PlugPoint) connectedJointPoint);
                plugPoint.InvokeOnDisconnect(plugPoint, socketPoint);
                socketPoint.InvokeOnDisconnect(plugPoint, socketPoint);
            }
            if (jointPoint is PlugPoint)
            {
                var socketPoint = ((SocketPoint) connectedJointPoint);
                var plugPoint = ((PlugPoint) jointPoint);
                plugPoint.InvokeOnDisconnect(plugPoint, socketPoint);
                socketPoint.InvokeOnDisconnect(plugPoint, socketPoint);
            }
            
        }

        /// <summary>
        /// Подключение розетки к вилке. 
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        /// <returns>Истина, если подключилось.</returns>
        public static bool Connect(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            if (!CanConnecting(socketPoint, plugPoint))
            {
                return false;
            }
            
            plugPoint.SocketController.gameObject.TransformToSocket(socketPoint, plugPoint);
            CreateJoint(plugPoint, socketPoint);

            plugPoint.ConnectedPoint = socketPoint;
            socketPoint.ConnectedPoint = plugPoint;

            plugPoint.SocketController.OnConnect?.Invoke(socketPoint, plugPoint);
            socketPoint.SocketController.OnConnect?.Invoke(socketPoint, plugPoint);

            plugPoint.SocketController.ConnectionGraphBehaviour.UpdateAllTrees();
            socketPoint.SocketController.ConnectionGraphBehaviour.UpdateAllTrees();

            plugPoint.SocketController.ResetState();
            socketPoint.SocketController.ResetState();
            
            plugPoint.InvokeOnConnect(plugPoint, socketPoint);
            socketPoint.InvokeOnConnect(plugPoint, socketPoint);

            return true;
        }

        /// <summary>
        /// Создание джоинта между объектами.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public static void CreateJoint(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            var configurableJoint = plugPoint.SocketController.gameObject.AddComponent<ConfigurableJoint>();

            configurableJoint.xMotion = ConfigurableJointMotion.Locked;
            configurableJoint.yMotion = ConfigurableJointMotion.Locked;
            configurableJoint.zMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularXMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularYMotion = ConfigurableJointMotion.Locked;
            configurableJoint.angularZMotion = ConfigurableJointMotion.Locked;
            configurableJoint.projectionMode = JointProjectionMode.PositionAndRotation;
            configurableJoint.projectionDistance = 0;
            configurableJoint.projectionAngle = 0;
            configurableJoint.connectedBody = socketPoint.SocketController.Rigidbody;           

            plugPoint.Joint = configurableJoint;
        }

        /// <summary>
        /// Отображение превью у розетки.
        /// </summary>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="socketPoint">Розетка.</param>
        public static void ShowPreview(PlugPoint plugPoint, SocketPoint socketPoint)
        {
            if (!socketPoint || !plugPoint)
            {
                return;
            }

            if (!CanConnecting(socketPoint, plugPoint))
            {
                return;
            }

            if (socketPoint.ChainPreview)
            {
                Destroy(socketPoint.ChainPreview.gameObject);
            }
            
            socketPoint.ChainPreview = plugPoint.SocketController.PreviewBehaviour.CompareTreePreviewObjects();
            socketPoint.ChainPreview.TransformToSocket(socketPoint, plugPoint);
            socketPoint.ChainPreview.transform.parent = socketPoint.transform;
        }
        
        /// <summary>
        /// Отображение превью возможного объекта в сцене у розетки. 
        /// </summary>
        /// <param name="socketPoint">Розетка.</param>
        public static void ShowPreview(SocketPoint socketPoint)
        {
            if (!socketPoint)
            {
                return;
            }
            
            foreach (var plugPoint in JointPoint.InstancedPlugPoints.Where(plugPoint => CanConnecting(socketPoint, plugPoint)))
            {
                ShowPreview(plugPoint, socketPoint);
                return;
            }
        }
        
        /// <summary>
        /// Сброс отображения превью.
        /// </summary>
        /// <param name="socketPoint">Розетка.</param>
        public static void HidePreview(SocketPoint socketPoint)
        {
            if (!socketPoint)
            {
                return;
            }

            var chainPreview = socketPoint.ChainPreview;

            if (!chainPreview)
            {
                return;
            }

            Destroy(chainPreview.gameObject);
        }
    }
}
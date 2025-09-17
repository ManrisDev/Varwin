using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;

namespace Varwin.SocketLibrary.Extension
{
    /// <summary>
    /// Проще говоря дерево соединений.
    /// </summary>
    public class ConnectionGraphBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Базовый контроллер, которому принадлежит дерево.
        /// </summary>
        private SocketController _baseController;

        /// <summary>
        /// Голова дерева.
        /// </summary>
        public ElementOfTree HeadOfTree { get; private set; }

        /// <summary>
        /// Корутина подписки.
        /// </summary>
        private Coroutine _subscribeCoroutine;

        /// <summary>
        /// Базовый контроллер соединений.
        /// </summary>
        public SocketController SocketController => _baseController;

        /// <summary>
        /// Инцииализация дерева.
        /// </summary>
        /// <param name="socketController">Контроллер соединений.</param>
        /// <returns>Дерево соединений.</returns>
        public static ConnectionGraphBehaviour InitTree(SocketController socketController)
        {
            var tree = new GameObject("ConnectionTree").AddComponent<ConnectionGraphBehaviour>();
            tree._baseController = socketController;
            tree.transform.parent = socketController.transform;

            tree.UpdateTree();

            return tree;
        }

        /// <summary>
        /// Включает ли в дочерние объекты данный контроллер.
        /// </summary>
        /// <param name="socketController">Контроллер.</param>
        /// <returns>Истина, если имеется.</returns>
        public bool HasChild(SocketController socketController)
        {
            var has = false;

            ForEach(a => has |= a == socketController);
            return has;
        }

        /// <summary>
        /// Обновление всех деревьев графа.
        /// </summary>
        public void UpdateAllTrees()
        {
            ForEach(a => a.ConnectionGraphBehaviour?.UpdateTree());
        }

        /// <summary>
        /// Обновление дерева.
        /// </summary>
        private void UpdateTree()
        {
            if (!_baseController)
            {
                return;
            }

            HeadOfTree = RecursiveAppendingTree(_baseController);
            if (HeadOfTree.SelfObject.IsLocalGrabbed)
            {
                _subscribeCoroutine = StartCoroutine(SubscribeAfterFrame());
            }
            else
            {
                if (!IsGrabbed())
                {
                    ForEach(a => a.SelfObject.CollisionProvider.OnGrabEnd());
                }
            }

        }

        /// <summary>
        /// Костыль для обхода удаления CollisionController'a.
        /// </summary>
        private IEnumerator SubscribeAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (IsGrabbed())
            {
                ForEach(a => { a.SelfObject.CollisionProvider.OnGrabStart(); });
            }
            else
            {
                ForEach(a => { a.SelfObject.CollisionProvider.OnGrabEnd(); });
            }
        }

        /// <summary>
        /// Принудительное выключение подписки CollisionController'a. 
        /// </summary>
        public void ForceStopSubscribe()
        {
            if (_subscribeCoroutine != null)
            {
                StopCoroutine(_subscribeCoroutine);
            }
        }
        
        #region Различные необходимые операции для работы с деревом.

        /// <summary>
        /// Цикл по каждому элементу дерева.
        /// </summary>
        /// <param name="eventForController">Метод, выполняемый на каждом элементе.</param>
        public void ForEach(Action<SocketController> eventForController)
        {
            RecursiveEvent(arg => eventForController?.Invoke(arg.SelfObject), HeadOfTree);
        }

        /// <summary>
        /// Цикл по каждому элементу дерева.
        /// </summary>
        /// <param name="eventForElementOfTree">Метод, выполняемый на каждом элементе.</param>
        public void ForEach(Action<ElementOfTree> eventForElementOfTree)
        {
            RecursiveEvent(eventForElementOfTree, HeadOfTree);
        }

        /// <summary>
        /// Поднята ли связка.
        /// </summary>
        /// <returns>Истина, если поднята.</returns>
        public bool IsGrabbed()
        {
            var isGrabbed = false;

            ForEach(a => isGrabbed |= a && a.IsLocalGrabbed);

            return isGrabbed;
        }

        private bool IsDeleted()
        {
            var isDeleted = false;

            ForEach(a => isDeleted = a.gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true);

            return isDeleted;
        }

        /// <summary>
        /// Подключается ли связка..
        /// </summary>
        /// <returns>Истина, если подключается.</returns>
        public bool IsConnecting()
        {
            var isConnecting = false;

            ForEach(a => isConnecting |= a.Connecting);

            return isConnecting;
        }

        /// <summary>
        /// Вызов рекурсивного метода по дереву.
        /// </summary>
        /// <param name="eventForController">Метод, вызываемый для каждого элемента дерева.</param>
        /// <param name="elementOfTree">Голова дерева.</param>
        private void RecursiveEvent(Action<ElementOfTree> eventForController, ElementOfTree elementOfTree)
        {
            if (elementOfTree == null)
            {
                return;
            }

            eventForController?.Invoke(elementOfTree);

            foreach (var child in elementOfTree.Childs)
            {
                RecursiveEvent(eventForController, child);
            }
        }

        #endregion

        /// <summary>
        /// Рекурсивное заполнение дерева.
        /// </summary>
        /// <param name="socketController">Ссылка на базовый контроллер.</param>
        /// <param name="pullControllers">Пулл уже обработанных контроллеров.</param>
        /// <returns>Голова дерева.</returns>
        private ElementOfTree RecursiveAppendingTree(SocketController socketController, HashSet<SocketController> pullControllers = null)
        {
            if (socketController == null)
            {
                return null;
            }

            if (pullControllers == null)
            {
                pullControllers = new HashSet<SocketController>();
            }

            if (pullControllers.Contains(socketController))
            {
                return null;
            }

            var child = new ElementOfTree(socketController);

            pullControllers.Add(socketController);

            if (socketController.JointPoints == null)
            {
                return child;
            }

            foreach (var jointPoint in socketController.JointPoints)
            {
                if (jointPoint.IsFree)
                {
                    continue;
                }

                var connectedChild = RecursiveAppendingTree(jointPoint.ConnectedPoint.SocketController, pullControllers);

                if (connectedChild == null)
                {
                    continue;
                }

                if (jointPoint is PlugPoint point)
                {
                    point.SocketController.gameObject.TransformToSocket((SocketPoint) point.ConnectedPoint, point);
                }

                if (jointPoint is SocketPoint socketPoint)
                {
                    socketPoint.ConnectedPoint.SocketController.gameObject.TransformToSocket(socketPoint, (PlugPoint) socketPoint.ConnectedPoint);
                }

                connectedChild.ConnectionPositionOffset = socketController.transform.InverseTransformPoint(connectedChild.SelfObject.transform.position);
                connectedChild.ConnectionRotationOffset = Quaternion.Inverse(socketController.transform.rotation) * connectedChild.SelfObject.transform.rotation;

                child.Childs.Add(connectedChild);
            }

            return child;
        }

        /// <summary>
        /// При взятии в руку.
        /// </summary>
        public void OnGrabStart()
        {
            ForEach(a =>
            {
                a.CollisionProvider.OnGrabStart();
                a.JointPoints?.ForEach(b => b.OnSocketControllerGrabStart());
            });
        }

        /// <summary>
        /// При отпускании.
        /// </summary>
        public void OnGrabEnd()
        {
            if (IsDeleted())
            {   
                SocketController.SocketPoints.ForEach(a => a.CandidateJointPoint?.SocketController.ResetHighlight());
                SocketController.PlugPoints.ForEach(a => a.CandidateJointPoint?.SocketController.ResetHighlight());
            }

            if (IsGrabbed())
            {
                return;
            }

            ForEach(a =>
            {
                a.CollisionProvider.OnGrabEnd();
                a.ConnectIfPossible();
            });
        }

        /// <summary>
        /// Отрисовка в Unity дерева.
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            DrawTree(HeadOfTree);
        }

        /// <summary>
        /// Отрисовка дерева.
        /// </summary>
        /// <param name="head"></param>
        private void DrawTree(ElementOfTree head)
        {
            if (head == null || !head.SelfObject)
            {
                return;
            }

            Gizmos.DrawSphere(head.SelfObject.transform.position, 0.01f);
            foreach (var elementOfTree in head.Childs)
            {
                if (!elementOfTree.SelfObject)
                {
                    continue;
                }

                Gizmos.DrawLine(head.SelfObject.transform.position, elementOfTree.SelfObject.transform.position);
                DrawTree(elementOfTree);
            }
        }

        /// <summary>
        /// Фикс трансформации объекта при взятии в руку.
        /// </summary>
        private void LateUpdate()
        {
            if (HeadOfTree == null)
            {
                return;
            }

            if (!HeadOfTree.SelfObject.IsLocalGrabbed)
            {
                return;
            }

            if (ProjectData.PlatformMode != PlatformMode.Vr)
            {
                TransformChild(HeadOfTree);
            }
        }

        /// <summary>
        /// Трансформация объектов дерева в VR.
        /// </summary>
        /// <param name="headOfTree">Голова списка.</param>
        /// <param name="mainRigidbody">Взятый объект.</param>
        public void TransformChildByRigidbody(ElementOfTree headOfTree, Rigidbody mainRigidbody)
        {
            foreach (var element in headOfTree.Childs)
            {
                element.SelfObject.transform.rotation = headOfTree.SelfObject.transform.rotation * element.ConnectionRotationOffset;
                element.SelfObject.transform.position = headOfTree.SelfObject.transform.TransformPoint(element.ConnectionPositionOffset);
                element.SelfObject.Rigidbody.velocity = mainRigidbody.GetPointVelocity(element.SelfObject.transform.position);
                element.SelfObject.Rigidbody.angularVelocity = mainRigidbody.angularVelocity;
                element.SelfObject.Rigidbody.ResetInertiaTensor();

                TransformChildByRigidbody(element, mainRigidbody);
            }
        }

        /// <summary>
        /// Трансформация объектов дерева в DP.
        /// </summary>
        /// <param name="headOfTree">Голова дерева.</param>
        private void TransformChild(ElementOfTree headOfTree)
        {
            foreach (var element in headOfTree.Childs)
            {
                element.SelfObject.Rigidbody.velocity = Vector3.zero;
                element.SelfObject.Rigidbody.angularVelocity = Vector3.zero;

                element.SelfObject.transform.position = headOfTree.SelfObject.transform.TransformPoint(element.ConnectionPositionOffset);
                element.SelfObject.transform.rotation = headOfTree.SelfObject.transform.rotation * element.ConnectionRotationOffset;

                TransformChild(element);
            }
        }
    }
}
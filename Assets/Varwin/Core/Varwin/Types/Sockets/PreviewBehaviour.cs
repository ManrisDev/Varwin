using System.Collections.Generic;
using UnityEngine;
using Varwin.SocketLibrary.Extension;
using Object = UnityEngine.Object;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Класс, реализующий логику превью цепей.
    /// </summary>
    public class PreviewBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Объект-контроллер соединений.
        /// </summary>
        private SocketController _sourceSocketController;

        /// <summary>
        /// Отдельные объекты, которые были выделены в результате выполнения.
        /// </summary>
        private List<Object> _allocatedData = new();

        /// <summary>
        /// Инициализация контроллера превью.
        /// </summary>
        /// <param name="socketController">Контроллер соединений.</param>
        public void Init(SocketController socketController)
        {
            _sourceSocketController = socketController;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Очистка ресурсов.
        /// </summary>
        private void OnDestroy()
        {
            _allocatedData.ForEach(Destroy);
            _allocatedData.Clear();
        }

        /// <summary>
        /// Добавление в список выделенной памяти.
        /// </summary>
        /// <param name="allocatedObject">Объект.</param>
        public void AddAllocated(Object allocatedObject)
        {
            _allocatedData.Add(allocatedObject);
        }
        
        /// <summary>
        /// Отображение превью цепи.
        /// </summary>
        /// <param name="socketPoint">Розетка.</param>
        /// <param name="plugPoint">Вилка.</param>
        public void Show(SocketPoint socketPoint, PlugPoint plugPoint)
        {
            gameObject.TransformToSocket(socketPoint, plugPoint);
            DrawTreePreview(_sourceSocketController.ConnectionGraphBehaviour.HeadOfTree);
        }

        /// <summary>
        /// Отобразить превью дерева объектов.
        /// </summary>
        /// <param name="elementOfTree">Первый элемент дерева.</param>
        private void DrawTreePreview(ElementOfTree elementOfTree)
        {
            elementOfTree.SelfObject.PreviewBehaviour.gameObject.SetActive(true);
            elementOfTree.SelfObject.PreviewBehaviour.transform.localScale = elementOfTree.SelfObject.transform.localScale;
            
            foreach (var child in elementOfTree.Childs)
            {
                var position = elementOfTree.SelfObject.PreviewBehaviour.transform.TransformPoint(child.ConnectionPositionOffset);
                
                var rotation = elementOfTree.SelfObject.PreviewBehaviour.transform.rotation * child.ConnectionRotationOffset;

                child.SelfObject.PreviewBehaviour.transform.position = position;
                child.SelfObject.PreviewBehaviour.transform.rotation = rotation;
                
                DrawTreePreview(child);
            }
        }
        
        /// <summary>
        /// Скрыть превью дерева.
        /// </summary>
        /// <param name="elementOfTree">Первый элемент дерева.</param>
        private void HideTreePreview(ElementOfTree elementOfTree)
        {
            elementOfTree.SelfObject.PreviewBehaviour.gameObject.SetActive(false);
            
            foreach (var child in elementOfTree.Childs)
            {
                HideTreePreview(child);
            }
        }

        /// <summary>
        /// Скрыть превью дерева.
        /// </summary>
        public void Hide()
        {
            HideTreePreview(_sourceSocketController.ConnectionGraphBehaviour.HeadOfTree);
        }

        /// <summary>
        /// Сборка превью в виде самостоятельного объекта.
        /// </summary>
        /// <returns>Экземпляр головы списка.</returns>
        public GameObject CompareTreePreviewObjects()
        {
            return CompareTreePreviewObjects(_sourceSocketController.ConnectionGraphBehaviour.HeadOfTree, null);
        }
        
        /// <summary>
        /// Сборка превью в виде самостоятельного объекта.
        /// </summary>
        /// <param name="elementOfTree">Первый элемент дерева.</param>
        /// <param name="headOfTreeTransform">Верхний элемент дерева.</param>
        /// <returns>Экземпляр головы списка.</returns>
        private GameObject CompareTreePreviewObjects(ElementOfTree elementOfTree, Transform headOfTreeTransform)
        {
            elementOfTree.SelfObject.PreviewBehaviour.transform.localScale = elementOfTree.SelfObject.transform.localScale;
            var headGameObject = Instantiate(elementOfTree.SelfObject.PreviewBehaviour.gameObject, headOfTreeTransform);
            headGameObject.gameObject.SetActive(true);

            foreach (var child in elementOfTree.Childs)
            {
                var childInstance = CompareTreePreviewObjects(child, headGameObject.transform);

                var position = headGameObject.transform.TransformPoint(child.ConnectionPositionOffset);
                
                var rotation = headGameObject.transform.rotation * child.ConnectionRotationOffset;

                childInstance.transform.position = position;
                childInstance.transform.rotation = rotation;
            }

            return headGameObject;
        }
    }
}
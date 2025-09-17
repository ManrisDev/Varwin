using System.Collections.Generic;
using UnityEngine;

namespace Varwin.SocketLibrary.Extension
{
    /// <summary>
    /// Элемент графа.
    /// </summary>
    public class ElementOfTree
    {
        /// <summary>
        /// Ссылка на главный объект элемента.
        /// </summary>
        public SocketController SelfObject;

        /// <summary>
        /// Оффсет позиции данного объекта относительно родительского.
        /// </summary>
        public Vector3 ConnectionPositionOffset;
        
        /// <summary>
        /// Оффсет вращения данного объекта относительно родительского.
        /// </summary>
        public Quaternion ConnectionRotationOffset;
        
        /// <summary>
        /// Список дочерних объектов.
        /// </summary>
        public List<ElementOfTree> Childs;

        /// <summary>
        /// Инициализация элемента дерева.
        /// </summary>
        /// <param name="selfObject">Контроллер соединений.</param>
        public ElementOfTree(SocketController selfObject)
        {
            SelfObject = selfObject;
            Childs = new List<ElementOfTree>();
        }
    }
}
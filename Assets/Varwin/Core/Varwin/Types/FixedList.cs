using System;
using System.Collections;
using System.Collections.Generic;

namespace Varwin
{
    /// <summary>
    /// Фиксированный список объектов. Если добавлен объект, который увеличивает размер списка, то первый уничтожается. 
    /// </summary>
    /// <typeparam name="T">Тип.</typeparam>
    public class FixedList<T> : IList<T>
    {
        /// <summary>
        /// Список объектов.
        /// </summary>
        private List<T> _list;
        
        /// <summary>
        /// Размер списка.
        /// </summary>
        private int _size;

        /// <summary>
        /// Инициализация списка.
        /// </summary>
        /// <param name="size">Размер списка.</param>
        public FixedList(int size)
        {
            _list = new List<T>();
            _size = size;
        }
        
        /// <summary>
        /// Получить enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        /// <summary>
        /// Получить enumerator.
        /// </summary>
        /// <returns>Enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Добавить элемент в список.
        /// </summary>
        /// <param name="item">Объект.</param>
        public void Add(T item)
        {
            if (_list.Count - 1 > _size)
            {
                _list.RemoveAt(0);
            }
            
            _list.Add(item);
        }

        /// <summary>
        /// Очистить список.
        /// </summary>
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Содержится ли элемент в списке.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Истина, если содержится.</returns>
        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        /// <summary>
        /// Копировать массив в список. Однако в случае со списком, данная операция будет недоступна.
        /// </summary>
        /// <param name="array">Массив.</param>
        /// <param name="arrayIndex">Индекс.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        /// <summary>
        /// Удалить элемент из списка.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Истина, если удаление прошло успешно.</returns>
        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        /// <summary>
        /// Длина списка.
        /// </summary>
        public int Count => _list.Count;
        
        /// <summary>
        /// Является ли только для чтения.
        /// </summary>
        public bool IsReadOnly => false;
        
        /// <summary>
        /// Индекс элемента в списке.
        /// </summary>
        /// <param name="item">Элемент.</param>
        /// <returns>Индекс элемента.</returns>
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        /// <summary>
        /// Вставить объект по индексу. Однако в случае со списком, данная операция будет недоступна.
        /// </summary>
        /// <param name="index">Индекс.</param>
        /// <param name="item">Элемент.</param>
        public void Insert(int index, T item)
        {
            throw new InvalidOperationException("Operation not supported");
        }

        /// <summary>
        /// Удалить элемент по индексу.
        /// </summary>
        /// <param name="index">Индекс элемента.</param>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        /// <summary>
        /// Обращение к списку как массиву (по индексу).
        /// </summary>
        /// <param name="index">Индекс.</param>
        public T this[int index]
        {
            get => _list[index];
            set => _list[index] = value;
        }
    }
}
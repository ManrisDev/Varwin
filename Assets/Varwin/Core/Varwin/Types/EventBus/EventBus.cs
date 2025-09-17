using System;
using System.Collections.Generic;

namespace Varwin.Events
{
    /// <summary>
    /// Класс для отправки сообщений всем слушателям.
    /// </summary>
    public class EventBus
    {
        #region fields

        /// <summary>
        /// Список слушателей.
        /// </summary>
        private readonly Dictionary<Type, List<IBaseEventReceiver>> _receivers = new();

        #endregion

        #region public methods

        public void Register<T>(IEventReceiver<T> receiver) where T : struct, IEvent
        {
            var eventType = typeof(T);
            if (!_receivers.ContainsKey(eventType))
            {
                _receivers[eventType] = new List<IBaseEventReceiver>();
            }

            _receivers[eventType].Add(receiver);
        }

        public void Unregister<T>(IEventReceiver<T> receiver) where T : struct, IEvent
        {
            var eventType = typeof(T);
            if (_receivers.TryGetValue(eventType, out var receiversList))
            {
                if (receiversList.Contains(receiver))
                {
                    receiversList.Remove(receiver);
                }
            }
        }

        public void Raise<T>(T eventArg) where T : struct, IEvent
        {
            var eventType = typeof(T);
            if (!_receivers.TryGetValue(eventType, out var receiversList))
                return;

            for (var i = receiversList.Count - 1; i >= 0; i--)
            {
                if (receiversList[i] != null)
                {
                    ((IEventReceiver<T>)receiversList[i]).OnEvent(eventArg);
                }
            }
        }

        public void Clear()
        {
            _receivers.Clear();
        }

        #endregion
    }
}
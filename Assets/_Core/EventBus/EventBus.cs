using System;
using System.Collections.Generic;

namespace DungeonShooter
{
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
        void Publish<T>(T eventData);
    }
    
    /// <summary>
    /// 이벤트 타입별로 구독, 구독해제, 퍼블리시를 수행하는 이벤트 버스
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null)
                return;

            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _handlers[type] = list;
            }

            list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
                return;

            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                    _handlers.Remove(type);
            }
        }

        public void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list))
                return;

            var copy = new List<Delegate>(list);
            foreach (var d in copy)
            {
                ((Action<T>)d).Invoke(eventData);
            }
        }
    }
}

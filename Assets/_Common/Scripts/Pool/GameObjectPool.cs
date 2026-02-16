using System.Collections.Generic;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 키별로 GameObjects를 보관하는 오브젝트 풀.
    /// 팩토리에서 풀을 관리할 때 사용한다.
    /// </summary>
    public class GameObjectPool
    {
        private Transform _poolRoot;
        private readonly Dictionary<string, Stack<GameObject>> _pools = new Dictionary<string, Stack<GameObject>>();

        public GameObjectPool()
        {
            var rootGo = GameObject.Find(nameof(GameObjectPool));
            _poolRoot = rootGo != null? rootGo.transform : new GameObject(nameof(GameObjectPool)).transform;
        }
        
        /// <summary>
        /// 해당 키의 풀에서 오브젝트를 꺼낸다. 없으면 null.
        /// </summary>
        public GameObject Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            if (!_pools.TryGetValue(key, out var stack) || stack.Count == 0)
            {
                return null;
            }

            var go = stack.Pop();
            if (go == null)
            {
                return Get(key);
            }

            return go;
        }

        /// <summary>
        /// 오브젝트를 해당 키의 풀에 반환한다.
        /// </summary>
        public void Return(string key, GameObject gameObject)
        {
            if (string.IsNullOrEmpty(key) || gameObject == null)
            {
                return;
            }
            
            gameObject.SetActive(false);
            
            if (!_pools.TryGetValue(key, out var stack))
            {
                stack = new Stack<GameObject>();
                _pools[key] = stack;
            }

            stack.Push(gameObject);
            
            gameObject.transform.SetParent(_poolRoot);
        }

        /// <summary>
        /// 지정 키의 풀에 보관된 개수를 반환한다.
        /// </summary>
        public int GetCount(string key)
        {
            if (string.IsNullOrEmpty(key) || !_pools.TryGetValue(key, out var stack))
            {
                return 0;
            }

            return stack.Count;
        }
    }
}

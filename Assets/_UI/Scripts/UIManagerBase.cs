using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// UI 생성/제거 및 타입별 캔버스/정렬 담당 매니저의 기본 구현 클래스.
    /// </summary>
    public abstract class UIManagerBase : MonoBehaviour, IUIManager
    {
        private readonly List<UIBase> _uiList = new();
        private readonly Dictionary<string, UIBase> _uniqueUICache = new();
        private readonly Dictionary<string, UniTask<UIBase>> _loadingUniqueUI = new();
        private Dictionary<UIType, Transform> _canvasByType;
        
        protected IResourceProvider _resourceProvider;
        
        protected void Initialize(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }
        
        public void Awake()
        {
            _canvasByType = CreateCanvasesByType();
        }

        /// <summary>
        /// UIType별 캔버스를 생성한다. 캔버스 정렬 순서는 열거 순서를 따른다.
        /// </summary>
        private Dictionary<UIType, Transform> CreateCanvasesByType()
        {
            var result = new Dictionary<UIType, Transform>();

            foreach (UIType type in Enum.GetValues(typeof(UIType)))
            {
                var go = new GameObject($"Canvas_{type}");
                go.transform.SetParent(transform, false);

                var canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = (int)type;
                var scaler = go.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(Constants.ScreenSizeX, Constants.ScreenSizeY);
                go.AddComponent<GraphicRaycaster>();
                
                result[type] = go.transform;
            }

            return result;
        }

        /// <summary>
        /// 싱글턴으로 UI를 생성해 반환합니다, 이후 다시 요청시 이전에 생성했던 UI를 반환합니다
        /// </summary>
        public async UniTask<T> GetSingletonUIAsync<T>(string addressableKey, bool active = true) where T : UIBase
        {
            if (_uniqueUICache.TryGetValue(addressableKey, out var cached) && cached != null)
                return (T)cached;
            if (_loadingUniqueUI.TryGetValue(addressableKey, out var loadingTask))
                return (T)(await loadingTask);

            var task = LoadAndRegisterUniqueUIAsync<T>(addressableKey, active);
            _loadingUniqueUI[addressableKey] = task;
            return (T)(await task);
        }
        
        /// <summary>
        /// UI를 생성해 반환합니다.
        /// </summary>
        public async UniTask<T> CreateUIAsync<T>(string addressableKey, bool active = true) where T : UIBase
        {
            return await LoadUIAsync<T>(addressableKey, active);
        }

        /// <summary>
        /// 싱글턴으로 UI를 동기 생성해 반환합니다. 이후 다시 요청 시 이전에 생성했던 UI를 반환합니다.
        /// </summary>
        public T GetSingletonUISync<T>(string addressableKey, bool active = true) where T : UIBase
        {
            if (_uniqueUICache.TryGetValue(addressableKey, out var cached) && cached != null)
                return (T)cached;
            return (T)LoadAndRegisterUniqueUISync<T>(addressableKey, active);
        }

        /// <summary>
        /// UI를 동기 생성해 반환합니다.
        /// </summary>
        public T CreateUISync<T>(string addressableKey, bool active = true) where T : UIBase
        {
            return LoadUISync<T>(addressableKey, active);
        }

        private UIBase LoadAndRegisterUniqueUISync<T>(string addressableKey, bool active) where T : UIBase
        {
            var ui = LoadUISync<T>(addressableKey, active);
            if (ui != null)
            {
                _uniqueUICache[addressableKey] = ui;
            }
            if (ui != null)
            {
                ui.OnDestroyEvent += () => _uniqueUICache.Remove(addressableKey);
            }
            return ui;
        }

        private T LoadUISync<T>(string addressableKey, bool active) where T : UIBase
        {
            var instance = _resourceProvider.GetInstanceSync(addressableKey);
            if (instance == null)
                return null;

            var ui = instance.GetComponent<T>();
            if (ui == null)
            {
                LogHandler.LogError<UIManagerBase>($"프리팹에 UIBase가 없음: {addressableKey}");
                Destroy(instance);
                return null;
            }

            var parent = _canvasByType[ui.Type];
            instance.transform.SetParent(parent, false);
            instance.SetActive(active);
            _uiList.Add(ui);
            return ui;
        }

        private async UniTask<UIBase> LoadAndRegisterUniqueUIAsync<T>(string addressableKey, bool active) where T : UIBase
        {
            var ui = await LoadUIAsync<T>(addressableKey, active);
            if (ui != null)
            {
                _uniqueUICache[addressableKey] = ui;
            }
            _loadingUniqueUI.Remove(addressableKey);
            ui.OnDestroyEvent += () => _uniqueUICache.Remove(addressableKey);
            return ui;
        }

        private async UniTask<T> LoadUIAsync<T>(string addressableKey, bool active) where T : UIBase
        {
            var instance = await _resourceProvider.GetInstanceAsync(addressableKey);
            var ui = instance.GetComponent<T>();
            if (ui == null)
            {
                LogHandler.LogError<UIManagerBase>($"프리팹에 UIBase가 없음: {addressableKey}");
                Destroy(instance);
                return null;
            }

            var parent = _canvasByType[ui.Type];
            instance.transform.SetParent(parent, false);
            instance.SetActive(active);
            _uiList.Add(ui);
            return ui;
        }

        /// <summary>
        /// UI를 제거.
        /// </summary>
        public bool RemoveUI(UIBase uiBase)
        {
            if (uiBase == null)
                return false;

            var removed = _uiList.Remove(uiBase);
            if (removed)
            {
                uiBase.Destroy();
            }
            return removed;
        }

        /// <summary>
        /// 해당 타입 캔버스 내에서 UI의 정렬 순서(형제 인덱스)를 반환한다.
        /// </summary>
        public int GetOrder(UIBase uiBase)
        {
            if (uiBase == null)
                return -1;
            return uiBase.transform.GetSiblingIndex();
        }

        /// <summary>
        /// 해당 타입 캔버스 내에서 UI의 정렬 순서(형제 인덱스)를 설정한다.
        /// </summary>
        public void SetOrder(UIBase uiBase, int order)
        {
            if (uiBase == null)
                return;
            uiBase.transform.SetSiblingIndex(Mathf.Clamp(order, 0, uiBase.transform.parent.childCount - 1));
        }
    }
}

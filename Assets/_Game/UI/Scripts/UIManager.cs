using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// UI 생성/제거 및 타입별 캔버스·정렬을 담당하는 매니저.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private readonly List<UIBase> _uiList = new();
        private Dictionary<UIType, Transform> _canvasByType;
        private AddressablesScope _scope;

        public void Start()
        {
            _canvasByType = CreateCanvasesByType();
            _scope = new AddressablesScope();
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
        /// 어드레서블 키로 UI 프리팹을 로드해 해당 타입 캔버스에 생성한다.
        /// </summary>
        /// <param name="addressableKey">어드레서블 키</param>
        /// <returns>생성된 UI. 실패 시 null</returns>
        public async UniTask<T> CreateUIAsync<T>(string addressableKey) where T : UIBase
        {
            var handle = _scope.InstantiateAsync(addressableKey);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                LogHandler.LogError<UIManager>($"UI 생성 실패: {addressableKey}");
                return null;
            }

            var instance = handle.Result;
            var ui = instance.GetComponent<T>();
            if (ui == null)
            {
                LogHandler.LogError<UIManager>($"프리팹에 UIBase가 없음: {addressableKey}");
                Object.Destroy(instance);
                return null;
            }
            
            var parent = _canvasByType[ui.Type];
            instance.transform.SetParent(parent, false);

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
                uiBase.Destroy();
            return true;
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

        private void OnDestroy()
        {
            _scope?.Dispose();   
        }
    }
}

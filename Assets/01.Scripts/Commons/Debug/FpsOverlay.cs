using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// IMGUI로 화면 우측 상단에 FPS와 프레임 시간(ms)을 표시합니다.
    /// 씬의 GameObject에 붙여 사용합니다.
    /// </summary>
    public class FpsOverlay : MonoBehaviour
    {
        // 몇초에 한번 프레임을 업데이트 할지
        [SerializeField] 
        private float _targetUpdateTime = 0.1f;
        private float _updateTime = 0f;
        
        [SerializeField]
        private bool _visible = true;
        [SerializeField]
        private int _fontSize = 18;
        [SerializeField]
        private float _padding = 12f;

        private Rect _rect;
        private GUIContent _labelContent;
        private GUIStyle _labelStyle;
        private float _deltaTime = 0.16f;
        private void Update()
        {
            if (!_visible)
                return;

            _updateTime += Time.unscaledDeltaTime;

            if (_updateTime >= _targetUpdateTime)
            {
                _updateTime = 0f;
                _deltaTime = Time.unscaledDeltaTime;    
            }
        }
        
        private void OnGUI()
        {
            if (!_visible)
                return;
            
            if (_labelStyle == null)
            {
                _rect = new Rect(30, 30, Screen.width, Screen.height);
                _labelContent = new();
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _fontSize,
                    alignment = TextAnchor.UpperLeft
                };
            }
            
            var fps = Mathf.RoundToInt(1f / _deltaTime);
            var ms = _deltaTime * 1000f;
            
            _labelContent.text = $"{fps}FPS({ms:F1}ms)";
            
            GUI.Label(_rect, _labelContent, _labelStyle);
        }
    }
}

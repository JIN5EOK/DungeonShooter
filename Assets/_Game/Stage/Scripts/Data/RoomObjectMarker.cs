using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonShooter
{
    /// <summary>
    /// 방 에디터에서 테이블 ID 기반으로 배치된 오브젝트를 표시하기 위한 마커.
    /// 저장 시 ObjectData.TableId로 직렬화됩니다.
    /// </summary>
    public class RoomObjectMarker : MonoBehaviour
    {
        [SerializeField] private int _tableId;

        public int TableId
        {
            get => _tableId;
            set => _tableId = value;
        }
        private void OnDrawGizmos()
        {
            if (!Application.isEditor)
                return;
            var pos = transform.position;
            Gizmos.color = new Color(0f, 1f, 1f, 0.95f);
            Gizmos.DrawWireSphere(pos, 0.5f);
            Gizmos.DrawLine(pos, pos + Vector3.up * 0.6f);
#if UNITY_EDITOR
            var label = $"ID:{_tableId}";
            Handles.Label(pos + Vector3.up * 0.7f, label, GetLabelStyle());
#endif
        }

#if UNITY_EDITOR
        private static GUIStyle _labelStyle;

        private static GUIStyle GetLabelStyle()
        {
            if (_labelStyle != null)
                return _labelStyle;
            _labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(6, 6, 3, 3)
            };
            _labelStyle.normal.textColor = Color.white;
            var bg = new Texture2D(1, 1);
            bg.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.8f));
            bg.Apply();
            _labelStyle.normal.background = bg;
            return _labelStyle;
        }
#endif
    }
}

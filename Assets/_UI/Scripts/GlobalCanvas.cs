using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// 로딩처럼 씬이 넘어가도 파괴되지 않는 UI 요소들을 추가하기 위한 객체
    /// </summary>
    public class GlobalCanvas : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        [SerializeField] private Canvas _canvas;
    }
}
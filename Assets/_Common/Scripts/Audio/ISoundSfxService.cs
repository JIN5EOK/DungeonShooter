using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine.Audio;

namespace DungeonShooter
{
    /// <summary>
    /// 주소 기반 효과음 재생 서비스.
    /// ISceneResourceProvider를 이용해 리소스 로드, AudioPlayer를 이용해 오디오 재생합니다.
    /// </summary>
    public interface ISoundSfxService
    {
        /// <summary>
        /// 주소로 효과음을 OneShot 재생합니다.
        /// </summary>
        void PlayOneShot(string address, AudioMixerGroup audioMixerGroup = null);

        /// <summary>
        /// 주소로 효과음을 OneShot 재생합니다 (비동기)
        /// </summary>
        UniTask<AudioPlayer.PlayResult> PlayOneShotAsync(string address, AudioMixerGroup audioMixerGroup = null);
    }
}

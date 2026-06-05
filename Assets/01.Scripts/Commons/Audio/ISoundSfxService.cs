using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine.Audio;

namespace DungeonShooter
{
    /// <summary>
    /// 주소 기반 효과음 재생 서비스.
    /// ISceneResourceProvider를 이용해 리소스 로드, Jin5eok 라이브러리의 AudioPlayer를 이용해 오디오 재생합니다.
    /// </summary>
    public interface ISoundSfxService
    {
        /// <summary>
        /// 주소로 효과음을 OneShot 재생합니다.
        /// </summary>
        public void PlayOneShot(string address, AudioMixerGroup audioMixerGroup = null, Action<AudioPlayer.PlayResult> onComplete = null);

        /// <summary>
        /// 주소로 효과음을 비동기로 OneShot 재생합니다
        /// </summary>
        public UniTask<AudioPlayer.PlayResult> PlayOneShotAsync(string address, AudioMixerGroup audioMixerGroup = null);
    }
}

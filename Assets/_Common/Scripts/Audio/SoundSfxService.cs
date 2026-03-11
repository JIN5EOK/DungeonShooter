using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.Audio;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 주소 기반 효과음 재생. SceneResourceProvider로 AudioClip을 로드한 뒤 AudioPlayer로 재생합니다.
    /// </summary>
    public class SoundSfxService : ISoundSfxService
    {
        private readonly IResourceProvider _resourceProvider;

        [Inject]
        public SoundSfxService(IResourceProvider resourceProvider)
        {
            _resourceProvider = resourceProvider;
        }

        public void PlayOneShot(string address, AudioMixerGroup audioMixerGroup = null, Action<AudioPlayer.PlayResult> onComplete = null)
        {
            var clip = _resourceProvider.GetAssetSync<AudioClip>(address);
            AudioPlayer.PlayOneShot(clip, audioMixerGroup, onComplete);
        }

        public async UniTask<AudioPlayer.PlayResult> PlayOneShotAsync(string address, AudioMixerGroup audioMixerGroup = null)
        {
            var clip = await _resourceProvider.GetAssetAsync<AudioClip>(address);
            return await AudioPlayer.PlayOneShotAsync(clip, audioMixerGroup);
        }
    }
}

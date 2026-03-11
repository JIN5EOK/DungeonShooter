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
        private readonly SceneResourceProvider _sceneResourceProvider;

        [Inject]
        public SoundSfxService(SceneResourceProvider sceneResourceProvider)
        {
            _sceneResourceProvider = sceneResourceProvider;
        }

        public void PlayOneShot(string address, AudioMixerGroup audioMixerGroup = null, Action<AudioPlayer.PlayResult> onComplete = null)
        {
            var clip = _sceneResourceProvider.GetAssetSync<AudioClip>(address);
            AudioPlayer.PlayOneShot(clip, audioMixerGroup, onComplete);
        }

        public async UniTask<AudioPlayer.PlayResult> PlayOneShotAsync(string address, AudioMixerGroup audioMixerGroup = null)
        {
            var clip = await _sceneResourceProvider.GetAssetAsync<AudioClip>(address);
            return await AudioPlayer.PlayOneShotAsync(clip, audioMixerGroup);
        }
    }
}

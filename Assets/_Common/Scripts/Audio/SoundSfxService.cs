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
        private readonly ISceneResourceProvider _sceneResourceProvider;

        [Inject]
        public SoundSfxService(ISceneResourceProvider sceneResourceProvider)
        {
            _sceneResourceProvider = sceneResourceProvider;
        }

        public void PlayOneShot(string address, AudioMixerGroup audioMixerGroup = null)
        {
            PlayOneShotAsync(address, audioMixerGroup).Forget();
        }

        public async UniTask<AudioPlayer.PlayResult> PlayOneShotAsync(string address, AudioMixerGroup audioMixerGroup = null)
        {
            if (string.IsNullOrEmpty(address))
            {
                return AudioPlayer.PlayResult.Failed;
            }

            var clip = await _sceneResourceProvider.GetAssetAsync<AudioClip>(address);
            if (clip == null)
            {
                return AudioPlayer.PlayResult.Failed;
            }

            return await AudioPlayer.PlayOneShotAsync(clip, audioMixerGroup);
        }
    }
}

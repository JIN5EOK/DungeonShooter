using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace DungeonShooter
{
    /// <summary>
    /// 효과음을 OneShot으로 재생하는 이펙트. 주소 기반으로 SoundSfxService를 통해 재생합니다.
    /// </summary>
    [Serializable]
    public class PlaySoundEffect : EffectBase
    {
        [Header("재생할 오디오 클립")]
        [SerializeField]
        private AssetReferenceT<AudioClip> _audioClipRef;

        private string AudioClipAddress => _audioClipRef.AssetGUID.ToString();

        public override async UniTask<bool> Execute(SkillExecutionContext context, SkillTableEntry entry)
        {
            if (!await base.Execute(context, entry))
                return false;

            if (_audioClipRef == null)
            {
                LogHandler.LogWarning<PlaySoundEffect>("오디오 클립 에셋 레퍼런스가 비어 있습니다.");
                return false;
            }

            if (context.SoundSfxService == null)
            {
                LogHandler.LogWarning<PlaySoundEffect>("SoundSfxService가 컨텍스트에 없습니다.");
                return false;
            }

            context.SoundSfxService.PlayOneShot(AudioClipAddress);
            return true;
        }
    }
}

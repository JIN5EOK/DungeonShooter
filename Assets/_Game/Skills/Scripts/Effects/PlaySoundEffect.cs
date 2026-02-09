using System;
using Cysharp.Threading.Tasks;
using DungeonShooter;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

namespace DungeonShooter
{
    /// <summary>
    /// 효과음을 OneShot으로 재생하는 이펙트
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

            var clip = await context.SceneResourceProvider.GetAssetAsync<AudioClip>(AudioClipAddress);
            
            if (clip == null)
                return false;
            
            AudioPlayer.PlayOneShot(clip);
            return true;
        }
    }
}

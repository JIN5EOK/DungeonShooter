using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IPlayerFactory
    {
        public UniTask<EntityBase> GetPlayerAsync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        public EntityBase GetPlayerSync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성/이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private int _playerConfigTableId = 12000001; // 임시 ID
        private readonly IResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly ISkillFactory _skillFactory;

        [Inject]
        public PlayerFactory(
            IResourceProvider resourceProvider,
            ITableRepository tableRepository,
            ISkillFactory skillFactory,
            ICameraManager cameraManager)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _skillFactory = skillFactory;
        }

        /// <summary>
        /// 플레이어 캐릭터를 생성합니다
        /// </summary>
        public async UniTask<EntityBase> GetPlayerAsync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var playerAddress = GetPlayerAddress(config);
            if (string.IsNullOrEmpty(playerAddress))
                return null;
            var playerInstance = await _resourceProvider.GetInstanceAsync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
            var entity = InitializePlayerInstance(playerInstance);
            return entity;
        }

        /// <summary>
        /// 플레이어 캐릭터를 동기적으로 생성합니다.
        /// </summary>
        public EntityBase GetPlayerSync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true)
        {
            var playerAddress = GetPlayerAddress(config);
            if (string.IsNullOrEmpty(playerAddress))
                return null;
            var playerInstance = _resourceProvider.GetInstanceSync(playerAddress, position, rotation, parent, instantiateInWorldSpace);
            var entity = InitializePlayerInstance(playerInstance);
            return entity;
        }

        /// <summary>
        /// 플레이어 프리팹 어드레스 추출 및 검증
        /// </summary>
        private string GetPlayerAddress(PlayerConfigSo config)
        {
            var address = GetAddressOrNull(config.GameObjectRef);
            return address;
        }

        private EntityBase InitializePlayerInstance(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;

            var config = _tableRepository.GetTableEntry<PlayerConfigSo>(_playerConfigTableId);

            var player = playerInstance.AddOrGetComponent<Player>();

            var entityStats = new EntityStats();
            entityStats.Initialize(config?.Stats ?? new StatsDto());
            var context = new EntityContext(
                new EntityInputContext(),
                entityStats,
                new EntityHealth(entityStats.GetStat(StatType.Hp)),
                new EntitySkills());
            player.SetContext(context);

            RegistPlayerSkills(config, player.GetContext().Skills);
            
            return player;
        }

        private void RegistPlayerSkills(PlayerConfigSo config, IEntitySkills entitySkills)
        {
            if (config == null || entitySkills == null)
                return;

            var skill1 = _skillFactory.CreateSkillSync(config.Skill1Ref);
            if (skill1 != null)
                entitySkills.RegistSkill(skill1);

            var skill2 = _skillFactory.CreateSkillSync(config.Skill2Ref);
            if (skill2 != null)
                entitySkills.RegistSkill(skill2);

            if (config.Skills == null)
                return;

            foreach (var skillRef in config.Skills)
            {
                var skill = _skillFactory.CreateSkillSync(skillRef);
                if (skill != null)
                    entitySkills.RegistSkill(skill);
            }
        }

        private static string GetAddressOrNull(AssetReference assetReference)
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                return null;
            var key = assetReference.RuntimeKey.ToString();
            return string.IsNullOrEmpty(key) ? null : key;
        }
    }
}

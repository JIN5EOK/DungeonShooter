using System;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace DungeonShooter
{
    /// <summary>
    /// 플레이어 캐릭터를 생성하는 팩토리 인터페이스
    /// </summary>
    public interface IPlayerFactory
    {
        event Action<EntityBase, PlayerConfigSo, Vector3> PlayerSpawned;
        event Action<EntityBase, PlayerConfigSo, Vector3> PlayerDied;
        public UniTask<EntityBase> GetPlayerAsync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
        public EntityBase GetPlayerSync(PlayerConfigSo config, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool instantiateInWorldSpace = true);
    }

    /// <summary>
    /// 런타임 중 플레이어 캐릭터를 생성하는 팩토리.
    /// 엔티티 컴포넌트 초기화 및 플레이어 관련 UI 생성/이벤트 연동을 담당합니다.
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        public event Action<EntityBase, PlayerConfigSo, Vector3> PlayerSpawned;
        public event Action<EntityBase, PlayerConfigSo, Vector3> PlayerDied;

        private int _playerConfigTableId = 12000001;
        private readonly IResourceProvider _resourceProvider;
        private readonly ITableRepository _tableRepository;
        private readonly ISkillFactory _skillFactory;
        private readonly ICameraManager _cameraManager;
        private readonly PlayerInputManager _playerInputManager;
        private readonly LifetimeScope _sceneLifetimeScope;
        [Inject]
        public PlayerFactory(
            IResourceProvider resourceProvider
            , ITableRepository tableRepository
            , LifetimeScope sceneLifetimeScope
            , ISkillFactory skillFactory
            , ICameraManager cameraManager
            , PlayerInputManager inputManager)
        {
            _resourceProvider = resourceProvider;
            _tableRepository = tableRepository;
            _skillFactory = skillFactory;
            _cameraManager = cameraManager;
            _playerInputManager = inputManager;
            _sceneLifetimeScope = sceneLifetimeScope;
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

        /// <summary>
        /// Player 게임오브젝트 초기화, 컴포넌트 부착, 바인딩, UI 연동
        /// </summary>
        private EntityBase InitializePlayerInstance(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                LogHandler.LogWarning<PlayerFactory>($"플레이어 인스턴스가 생성되지 않았습니다.");
                return null;
            }

            playerInstance.tag = GameTags.Player;
            playerInstance.layer = PhysicalLayers.Player.LayerIndex;

            // 씬 LifeTimeScope를 부모로 삼기
            EntityLifeTimeScope entityLifeTimeScope = null;
            using (LifetimeScope.EnqueueParent(_sceneLifetimeScope))
            {
                entityLifeTimeScope = playerInstance.AddOrGetComponent<EntityLifeTimeScope>();    
            }
            
            var entity = entityLifeTimeScope.Container.Resolve<EntityBase>();

            var config = _tableRepository.GetTableEntry<PlayerConfigSo>(_playerConfigTableId);
            var statsDto = config?.Stats ?? new StatsDto();
            var statGroup = new EntityStats();
            statGroup.Initialize(statsDto);
            var entitySkills = new EntitySkills();
            var context = new EntityContext(
                new EntityInputContext(),
                statGroup,
                new HealthModel(statGroup.GetStat(StatType.Hp)),
                entitySkills);
            entity.SetContext(context);
            
            RegistPlayerSkills(config, entitySkills);
            
            var stateMachine = entityLifeTimeScope.Container.Resolve<IEntityStateMachine>();

            stateMachine.Initialize(
                entityLifeTimeScope.Container.Resolve<IdleState>(),
                entityLifeTimeScope.Container.Resolve<MoveState>(),
                entityLifeTimeScope.Container.Resolve<SkillState>());

            _cameraManager?.BindAsync(entity.transform).Forget();

            entity.GetContext().HealthModel.OnDeath += () =>
            {
                Object.Destroy(entity.gameObject);
                PlayerDied?.Invoke(entity, config, playerInstance.transform.position);
            };
            _playerInputManager.BindControlledEntity(entity);
            PlayerSpawned?.Invoke(entity, config, playerInstance.transform.position);
            return entity;
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

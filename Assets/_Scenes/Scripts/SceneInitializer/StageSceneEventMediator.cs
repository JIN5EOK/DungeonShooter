using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 스테이지 씬에서 사용될 매니저,서비스들의 이벤트들을 바인딩합니다
    /// </summary>
    public class StageSceneEventMediator
    {
        private readonly IPlayerFactory _playerFactory;
        private readonly IEnemyFactory _enemyFactory;
        private readonly IPlayerLevelService _playerLevelService;
        private readonly ISkillService _skillService;
        private readonly IPlayerContextManager _playerContextManager;
        private readonly PlayerInputManager _playerInputManager;
        private readonly SkillLevelUpUI _skillLevelUpUI;
        private readonly ISkillSlotService _skillSlotService;
        private readonly IItemDropService _itemDropService;
        private readonly IInventory _inventory;
        private readonly ObjectCullingManager _objectCullingManager;
        private readonly EntityManager _entityManager;
        private readonly GameHudGroupUI _gameHudGroupUI;
        [Inject]
        public StageSceneEventMediator(
            GameHudGroupUI gameHudGroupUI,
            IPlayerFactory playerFactory,
            IEnemyFactory enemyFactory,
            IPlayerLevelService playerLevelService,
            ISkillService skillService,
            IPlayerContextManager playerContextManager,
            PlayerInputManager playerInputManager,
            SkillLevelUpUI skillLevelUpUI,
            ISkillSlotService skillSlotService,
            IItemDropService itemDropService,
            IInventory inventory,
            ObjectCullingManager objectCullingManager,
            EntityManager entityManager)
        {
            _gameHudGroupUI = gameHudGroupUI;
            _playerFactory = playerFactory;
            _enemyFactory = enemyFactory;
            _playerLevelService = playerLevelService;
            _skillService = skillService;
            _playerContextManager = playerContextManager;
            _playerInputManager = playerInputManager;
            _skillLevelUpUI = skillLevelUpUI;
            _skillSlotService = skillSlotService;
            _itemDropService = itemDropService;
            _inventory = inventory;
            _objectCullingManager = objectCullingManager;
            _entityManager = entityManager;
        }

        public void Register()
        {
            _playerFactory.PlayerSpawned += ForwardPlayerSpawnToHealthHudUI;
            _playerFactory.PlayerSpawned += ForwardPlayerSpawnToInput;
            _playerFactory.PlayerDestroyed += ForwardPlayerDespawnToInput;

            _playerLevelService.OnLevelChanged += ForwardPlayerLevelChanged;

            _skillService.OnSkillLeveledUp += ForwardSkillLeveledUp;

            _enemyFactory.EnemyDied += ForwardEnemyDeadToDrop;

            _playerFactory.PlayerSpawned += ForwardPlayerSpawnToInventory;
            _playerFactory.PlayerDestroyed += ForwardPlayerDespawnInventory;

            _enemyFactory.EnemySpawned += ForwardEnemySpawnedCulling;
            _playerFactory.PlayerSpawned += ForwardPlayerSpawnedCulling;
            _playerFactory.PlayerDestroyed += ForwardPlayerDestroyedCulling;

            _enemyFactory.EnemySpawned += ForwardEnemySpawnedEntity;
            _enemyFactory.EnemyDied += ForwardEnemyDeadEntity;
        }

        public void Unregister()
        {
            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnToHealthHudUI;
            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnToInput;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDespawnToInput;

            _playerLevelService.OnLevelChanged -= ForwardPlayerLevelChanged;

            _skillService.OnSkillLeveledUp -= ForwardSkillLeveledUp;

            _enemyFactory.EnemyDied -= ForwardEnemyDeadToDrop;

            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnToInventory;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDespawnInventory;

            _enemyFactory.EnemySpawned -= ForwardEnemySpawnedCulling;
            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnedCulling;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDestroyedCulling;

            _enemyFactory.EnemySpawned -= ForwardEnemySpawnedEntity;
            _enemyFactory.EnemyDied -= ForwardEnemyDeadEntity;
        }

        private void ForwardPlayerSpawnToHealthHudUI(EntityBase player, PlayerConfigTableEntry config, Vector3 position)
        {
            _gameHudGroupUI.HealthBarHudUI.SetHealth(player.EntityContext.Statuses.GetStatus(StatusType.Hp).GetValue());
            _gameHudGroupUI.HealthBarHudUI.SetMaxHealth(player.EntityContext.Stat.GetStat(StatType.Hp).GetValue());
            player.EntityContext.Statuses.GetStatus(StatusType.Hp).OnValueChanged += _gameHudGroupUI.HealthBarHudUI.SetHealth;
            player.EntityContext.Stat.GetStat(StatType.Hp).OnValueChanged += _gameHudGroupUI.HealthBarHudUI.SetMaxHealth;
        }
        
        private void ForwardPlayerSpawnToInput(EntityBase player, PlayerConfigTableEntry config, Vector3 position) =>
            _playerInputManager.BindControlledEntity(player);

        private void ForwardPlayerDespawnToInput(EntityBase player, Vector3 position) =>
            _playerInputManager.UnbindControlledEntity();

        private void ForwardPlayerLevelChanged(int level)
        {
            var skills = _playerContextManager?.EntityContext?.Skill?.GetRegistedSkills();
            var levelUpableList = _skillService.GetLevelUpableSkills(skills);

            _skillLevelUpUI.ShowLevelUpSkillOptions(levelUpableList, selectedSkill =>
            {
                _skillService.TrySkillLevelUp(_playerContextManager?.EntityContext?.Skill, selectedSkill);
            });
        }

        private void ForwardSkillLeveledUp(Skill beforeSkill, Skill afterSkill) =>
            _skillSlotService.ReplaceSkillSlot(beforeSkill, afterSkill);

        private void ForwardEnemyDeadToDrop(EntityBase enemy, EnemyConfigTableEntry enemyConfigTableEntry, Vector3 position)
        {
            var weights = enemyConfigTableEntry?.DropItemWeights;
            if (weights == null || weights.Count == 0)
                return;

            _itemDropService.TryDropItemsByWeight(weights, position);
        }

        private void ForwardPlayerSpawnToInventory(EntityBase player, PlayerConfigTableEntry config, Vector3 position) =>
            _inventory.BindItemUserEntity(player);

        private void ForwardPlayerDespawnInventory(EntityBase player, Vector3 position) =>
            _inventory.UnbindItemUserEntity();

        private void ForwardEnemySpawnedCulling(EntityBase enemy) =>
            _objectCullingManager.AttachEntityToDistanceCullingRoot(enemy);

        private void ForwardPlayerSpawnedCulling(EntityBase player, PlayerConfigTableEntry config, Vector3 position) =>
            _objectCullingManager.SetPlayerDistanceReference(player);

        private void ForwardPlayerDestroyedCulling(EntityBase player, Vector3 position) =>
            _objectCullingManager.ClearPlayerDistanceReference();

        private void ForwardEnemySpawnedEntity(EntityBase enemy) =>
            _entityManager.RegisterSpawnedEnemy(enemy);

        private void ForwardEnemyDeadEntity(EntityBase enemy, EnemyConfigTableEntry enemyConfigTableEntry, Vector3 position)
        {
            var exp = enemyConfigTableEntry != null ? enemyConfigTableEntry.Exp : 0;
            _entityManager.NotifyEnemyDefeated(enemy, exp);
        }
    }
}

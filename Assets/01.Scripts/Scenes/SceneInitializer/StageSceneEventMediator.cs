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
        private readonly IItemDropService _itemDropService;
        private readonly IInventory _inventory;
        private readonly IPauseManager _pauseManager;
        private readonly IGameExitService _gameExitService;
        private readonly PauseMenuUI _pauseMenuUI;
        private readonly IGameMessageService _gameMessageService;
        private readonly AlertMessageUI _alertMessageUI;
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
            IItemDropService itemDropService,
            IInventory inventory,
            IPauseManager pauseManager,
            IGameExitService gameExitService,
            PauseMenuUI pauseMenuUI,
            IGameMessageService gameMessageService,
            AlertMessageUI alertMessageUI,
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
            _itemDropService = itemDropService;
            _inventory = inventory;
            _pauseManager = pauseManager;
            _gameExitService = gameExitService;
            _pauseMenuUI = pauseMenuUI;
            _gameMessageService = gameMessageService;
            _alertMessageUI = alertMessageUI;
            _objectCullingManager = objectCullingManager;
            _entityManager = entityManager;
        }

        public void Register()
        {
            _gameHudGroupUI.OnInventoryRequested += _inventory.Open;
            _gameHudGroupUI.OnPauseRequested += _pauseMenuUI.Show;

            _skillLevelUpUI.OnShow += () => _pauseManager.PauseRequest(_skillLevelUpUI);
            _skillLevelUpUI.OnHide += () => _pauseManager.ResumeRequest(_skillLevelUpUI);
            _gameMessageService.OnAlertMessageRequested += _alertMessageUI.ShowMessage;
            _pauseMenuUI.OnShow += () => _pauseManager.PauseRequest(_pauseMenuUI);
            _pauseMenuUI.OnHide += () => _pauseManager.ResumeRequest(_pauseMenuUI);
            _pauseMenuUI.OnResumeClickedEvent += _pauseMenuUI.Hide;
            _pauseMenuUI.OnExitClickedEvent += _gameExitService.ExitToMainMenu;

            _playerFactory.PlayerSpawned += ForwardPlayerSpawnToHudUI;
            _playerFactory.PlayerSpawned += ForwardPlayerSpawnToInput;
            _playerFactory.PlayerDestroyed += ForwardPlayerDespawnToInput;
            _playerFactory.PlayerDestroyed += ForwardPlayerDestroyedUnbindHud;

            _playerLevelService.OnLevelChanged += ForwardPlayerLevelChanged;
            _playerLevelService.OnExpChanged += ForwardPlayerExpChanged;
            _playerLevelService.OnMaxExpChanged += ForwardPlayerMaxExpChanged;

            _skillService.OnSkillLeveledUp += ForwardSkillLeveledUp;

            _playerContextManager.OnActiveSkillSlotChanged += ForwardActiveSkillSlotChangedToHud;
            _gameHudGroupUI.TouchInputUI.SetSkillSlot(0, _playerContextManager.GetActiveSkill(0));
            _gameHudGroupUI.TouchInputUI.SetSkillSlot(1, _playerContextManager.GetActiveSkill(1));

            _enemyFactory.EnemyDied += ForwardEnemyDeadToDrop;

            _playerFactory.PlayerSpawned += ForwardPlayerSpawnToInventory;
            _playerFactory.PlayerDestroyed += ForwardPlayerDespawnInventory;

            _enemyFactory.EnemySpawned += ForwardEnemySpawnedCulling;
            _playerFactory.PlayerSpawned += ForwardPlayerSpawnedCulling;
            _playerFactory.PlayerDestroyed += ForwardPlayerDestroyedCulling;

            _enemyFactory.EnemySpawned += ForwardEnemySpawnedEntity;
            _enemyFactory.EnemyDied += ForwardEnemyDeadEntity;

            _entityManager.OnRemainingEnemyCountChanged += ForwardRemainingEnemyCountChanged;
            _gameHudGroupUI.PlayerStatusHudUI.SetRemainingEnemyCount(_entityManager.RemainingEnemyCount);

            _gameHudGroupUI.ExpGaugeHudUI.SetLevel(_playerLevelService.Level);
            _gameHudGroupUI.ExpGaugeHudUI.SetMaxExp(_playerLevelService.MaxExp);
            _gameHudGroupUI.ExpGaugeHudUI.SetExp(_playerLevelService.Exp);
        }

        public void Unregister()
        {
            _gameHudGroupUI.OnInventoryRequested -= _inventory.Open;
            _gameHudGroupUI.OnPauseRequested -= _pauseMenuUI.Show;

            _gameMessageService.OnAlertMessageRequested -= _alertMessageUI.ShowMessage;
            _pauseMenuUI.OnResumeClickedEvent -= _pauseMenuUI.Hide;
            _pauseMenuUI.OnExitClickedEvent -= _gameExitService.ExitToMainMenu;
            
            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnToHudUI;
            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnToInput;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDespawnToInput;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDestroyedUnbindHud;

            _playerLevelService.OnLevelChanged -= ForwardPlayerLevelChanged;
            _playerLevelService.OnExpChanged -= ForwardPlayerExpChanged;
            _playerLevelService.OnMaxExpChanged -= ForwardPlayerMaxExpChanged;

            _skillService.OnSkillLeveledUp -= ForwardSkillLeveledUp;

            _playerContextManager.OnActiveSkillSlotChanged -= ForwardActiveSkillSlotChangedToHud;

            _enemyFactory.EnemyDied -= ForwardEnemyDeadToDrop;

            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnToInventory;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDespawnInventory;

            _enemyFactory.EnemySpawned -= ForwardEnemySpawnedCulling;
            _playerFactory.PlayerSpawned -= ForwardPlayerSpawnedCulling;
            _playerFactory.PlayerDestroyed -= ForwardPlayerDestroyedCulling;

            _enemyFactory.EnemySpawned -= ForwardEnemySpawnedEntity;
            _enemyFactory.EnemyDied -= ForwardEnemyDeadEntity;

            _entityManager.OnRemainingEnemyCountChanged -= ForwardRemainingEnemyCountChanged;
        }

        private void ForwardPlayerSpawnToHudUI(EntityBase player, PlayerConfigTableEntry config, Vector3 position)
        {
            var attack = player?.EntityContext?.Stat?.GetStat(StatType.Attack);
            var defense = player?.EntityContext?.Stat?.GetStat(StatType.Defense);
            var moveSpeed = player?.EntityContext?.Stat?.GetStat(StatType.MoveSpeed);
            var hpStatus = player?.EntityContext?.Statuses.GetStatus(StatusType.Hp);
            var hpStat = player?.EntityContext?.Stat.GetStat(StatType.Hp);

            _gameHudGroupUI.HealthBarHudUI.SetHealth(hpStatus.GetValue());
            hpStatus.OnValueChanged += _gameHudGroupUI.HealthBarHudUI.SetHealth;
            _gameHudGroupUI.HealthBarHudUI.SetMaxHealth(hpStat.GetValue());
            hpStat.OnValueChanged += _gameHudGroupUI.HealthBarHudUI.SetMaxHealth;
            
            _gameHudGroupUI.PlayerStatusHudUI.SetAttack(attack.GetValue());
            attack.OnValueChanged += _gameHudGroupUI.PlayerStatusHudUI.SetAttack;
            _gameHudGroupUI.PlayerStatusHudUI.SetDefense(defense.GetValue());
            defense.OnValueChanged += _gameHudGroupUI.PlayerStatusHudUI.SetDefense;
            _gameHudGroupUI.PlayerStatusHudUI.SetMoveSpeed(moveSpeed.GetValue());
            moveSpeed.OnValueChanged += _gameHudGroupUI.PlayerStatusHudUI.SetMoveSpeed;
        }
        
        private void ForwardPlayerSpawnToInput(EntityBase player, PlayerConfigTableEntry config, Vector3 position) =>
            _playerInputManager.BindControlledEntity(player);

        private void ForwardPlayerDespawnToInput(EntityBase player, Vector3 position) =>
            _playerInputManager.UnbindControlledEntity();

        private void ForwardPlayerDestroyedUnbindHud(EntityBase player, Vector3 position) =>
            UnbindHudFromPlayer(player);

        private void ForwardPlayerLevelChanged(int level)
        {
            _gameHudGroupUI.ExpGaugeHudUI.SetLevel(level);

            var skills = _playerContextManager?.EntityContext?.Skill?.GetRegistedSkills();
            var levelUpableList = _skillService.GetLevelUpableSkills(skills);

            if (levelUpableList.Count > 0)
            {
                _skillLevelUpUI.Show();
                _skillLevelUpUI.ShowLevelUpSkillOptions(levelUpableList, selectedSkill =>
                {
                    _skillService.TrySkillLevelUp(_playerContextManager?.EntityContext?.Skill, selectedSkill);
                    _skillLevelUpUI.Hide();
                });    
            }
        }

        private void ForwardPlayerExpChanged(int exp) =>
            _gameHudGroupUI.ExpGaugeHudUI.SetExp(exp);

        private void ForwardPlayerMaxExpChanged(int maxExp) =>
            _gameHudGroupUI.ExpGaugeHudUI.SetMaxExp(maxExp);

        private void ForwardSkillLeveledUp(Skill beforeSkill, Skill afterSkill) =>
            _playerContextManager.ReplaceActiveSkillSlot(beforeSkill, afterSkill);

        private void ForwardActiveSkillSlotChangedToHud(int idx, Skill skill) =>
            _gameHudGroupUI.TouchInputUI.SetSkillSlot(idx, skill);

        private void ForwardEnemyDeadToDrop(EntityBase enemy, EnemyConfigTableEntry enemyConfigTableEntry, Vector3 position) =>
            _itemDropService.TryDropItemsByWeight(enemyConfigTableEntry?.DropItemWeights, position);

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

        private void ForwardRemainingEnemyCountChanged(int count) =>
            _gameHudGroupUI.PlayerStatusHudUI.SetRemainingEnemyCount(count);

        private void UnbindHudFromPlayer(EntityBase player)
        {
            var statuses = player?.EntityContext?.Statuses;
            var stat = player?.EntityContext?.Stat;

            var hpStatus = statuses.GetStatus(StatusType.Hp);
            hpStatus.OnValueChanged -= _gameHudGroupUI.HealthBarHudUI.SetHealth;

            stat.GetStat(StatType.Hp).OnValueChanged -= _gameHudGroupUI.HealthBarHudUI.SetMaxHealth;
            stat.GetStat(StatType.Attack).OnValueChanged -= _gameHudGroupUI.PlayerStatusHudUI.SetAttack;
            stat.GetStat(StatType.Defense).OnValueChanged -= _gameHudGroupUI.PlayerStatusHudUI.SetDefense;
            stat.GetStat(StatType.MoveSpeed).OnValueChanged -= _gameHudGroupUI.PlayerStatusHudUI.SetMoveSpeed;
        }
    }
}

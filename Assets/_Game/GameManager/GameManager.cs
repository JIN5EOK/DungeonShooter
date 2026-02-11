using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임 매니저. 스테이지 시작 전 플레이어 세션 초기화 및 스테이지 생성을 담당합니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private StageManager _stageManager;
        private StageContext _stageContext;
        private ITableRepository _tableRepository;
        private PlayerStatusController _playerStatusController;
        private PlayerSkillController _playerSkillController;
        private Inventory _inventory;
        private IItemFactory _itemFactory;

        [Inject]
        public void Construct(StageManager stageManager
            , StageContext stageContext
            , ITableRepository tableRepository
            , PlayerStatusController playerStatusController
            , PlayerSkillController playerSkillController
            , Inventory inventory
            , IItemFactory itemFactory)
        {
            _stageManager = stageManager;
            _stageContext = stageContext;
            _tableRepository = tableRepository;
            _playerStatusController = playerStatusController;
            _playerSkillController = playerSkillController;
            _inventory = inventory;
            _itemFactory = itemFactory;
        }

        private async UniTaskVoid Start()
        {
            var config = _tableRepository.GetTableEntry<PlayerConfigTableEntry>(_stageContext.PlayerConfigTableId);
            if (config == null)
            {
                UnityEngine.Debug.LogError($"[{nameof(GameManager)}] PlayerConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.PlayerConfigTableId}");
                return;
            }

            _playerStatusController.Initialize(config);
            await _playerSkillController.InitializeAsync(config);

            _inventory.Clear();
            _inventory.SetStatGroup(_playerStatusController.StatGroup);
            _inventory.SetSkillGroup(_playerSkillController.SkillGroup);
            
            var weapon = await _itemFactory.CreateItemAsync(config.StartWeaponId);
            await _inventory.AddItem(weapon);
            await _inventory.EquipItem(weapon);

            await _stageManager.CreateStageAsync();
        }
    }
}

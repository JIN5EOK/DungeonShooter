using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace DungeonShooter
{
    public class StageManager
    {
        private Stage _stage;

        public Stage Stage => _stage;

        private IStageGenerator _stageGenerator;
        private IStageInstantiator _stageInstantiator;
        private ITableRepository _tableRepository;
        private StageContext _stageContext;

        [Inject]
        public void Construct(IStageGenerator stageGenerator, IStageInstantiator stageInstantiator, ITableRepository tableRepository, StageContext stageContext)
        {
            _stageGenerator = stageGenerator;
            _stageInstantiator = stageInstantiator;
            _tableRepository = tableRepository;
            _stageContext = stageContext;
        }

        /// <summary>
        /// 스테이지 구조와 인스턴스를 생성합니다
        /// </summary>
        public async UniTask CreateStageAsync()
        {
            _stage = await _stageGenerator.GenerateStage();
            var stageConfigEntry = _tableRepository.GetTableEntry<StageConfigTableEntry>(_stageContext.StageConfigTableId);
            if (stageConfigEntry == null)
            {
                Debug.LogError($"[{nameof(StageManager)}] StageConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.StageConfigTableId}");
                return;
            }
            await _stageInstantiator.InstantiateStage(stageConfigEntry, _stage);
        }
    }
}
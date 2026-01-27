using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;
using Random = UnityEngine.Random;

namespace DungeonShooter
{
    public interface IRoomDataRepository : IDisposable
    {
        UniTask<RoomData> GetRandomRoom(RoomType type);
    }

    public class RoomDataRepository : IRoomDataRepository
    {
        private AddressablesScope _addressablesScope = new AddressablesScope();
        private readonly ITableRepository _tableRepository;
        private readonly StageContext _stageContext;
        private List<string> StartRoomDataAddresses { get; set; }
        private List<string> NormalRoomDataAddresses { get; set; }
        private List<string> BossRoomDataAddresses { get; set; }
        private bool _isInitialized;
        private TaskCompletionSource<bool> _initializationTcs;

        [Inject]
        public RoomDataRepository(ITableRepository tableRepository, StageContext stageContext)
        {
            _tableRepository = tableRepository;
            _stageContext = stageContext;
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        private async UniTask InitializeAsync(TaskCompletionSource<bool> initializationTcs)
        {
            try
            {
                var stageConfigEntry = _tableRepository.GetTableEntry<StageConfigTableEntry>(_stageContext.StageConfigTableId);
                if (stageConfigEntry == null)
                {
                    Debug.LogError($"[{nameof(RoomDataRepository)}] StageConfigTableEntry를 찾을 수 없습니다. ID: {_stageContext.StageConfigTableId}");
                    initializationTcs.SetResult(false);
                    return;
                }

                if (!string.IsNullOrEmpty(stageConfigEntry.StartRoomsLabel))
                {
                    var handle = Addressables.LoadResourceLocationsAsync(stageConfigEntry.StartRoomsLabel);
                    await handle.Task;
                    StartRoomDataAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                    Addressables.Release(handle);
                }

                if (!string.IsNullOrEmpty(stageConfigEntry.NormalRoomsLabel))
                {
                    var handle = Addressables.LoadResourceLocationsAsync(stageConfigEntry.NormalRoomsLabel);
                    await handle.Task;
                    NormalRoomDataAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                    Addressables.Release(handle);
                }

                if (!string.IsNullOrEmpty(stageConfigEntry.BossRoomsLabel))
                {
                    var handle = Addressables.LoadResourceLocationsAsync(stageConfigEntry.BossRoomsLabel);
                    await handle.Task;
                    BossRoomDataAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                    Addressables.Release(handle);
                }
                initializationTcs.SetResult(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"{nameof(RoomDataRepository)} : 데이터 초기화 실패, {e}");
                initializationTcs.SetResult(false);
                throw;
            }
        }

        /// <summary>
        /// 초기화가 완료될 때까지 대기합니다. 이미 초기화되어 있으면 즉시 반환합니다.
        /// </summary>
        private async UniTask EnsureInitializedAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_initializationTcs == null)
            {
                _initializationTcs = new TaskCompletionSource<bool>();
                await InitializeAsync(_initializationTcs);
            }

            await _initializationTcs.Task;
        }

        /// <summary>
        /// 타입에 맞는 랜덤한 방을 반환합니다.
        /// </summary>
        /// <param name="type">방 타입</param>
        /// <returns>방 데이터</returns>
        public async UniTask<RoomData> GetRandomRoom(RoomType type)
        {
            await EnsureInitializedAsync();

            var targetAddresses = type == RoomType.Start ? StartRoomDataAddresses : type == RoomType.Normal ? NormalRoomDataAddresses : BossRoomDataAddresses;
            
            if (targetAddresses == null || targetAddresses.Count == 0)
            {
                Debug.LogWarning($"[{nameof(RoomDataRepository)}] {type} 타입의 방 데이터 주소 목록이 비어있습니다.");
                return null;
            }

            var targetAddress = targetAddresses[Random.Range(0, targetAddresses.Count)];
            var handle = _addressablesScope.LoadAssetAsync<TextAsset>(targetAddress);
            await handle.Task;
            return RoomDataSerializer.DeserializeRoom(handle.Result);
        }

        public void Dispose()
        {
            _addressablesScope.Dispose();
        }
    }
}
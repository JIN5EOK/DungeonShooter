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
        private StageConfig _stageConfig;
        private List<string> StartRoomDataAddresses { get; set; }
        private List<string> NormalRoomDataAddresses { get; set; }
        private List<string> BossRoomDataAddresses { get; set; }
        private bool _isInitialized;
        private TaskCompletionSource<bool> _initializationTcs;

        [Inject]
        public RoomDataRepository(StageContext stageContext)
        {
            _stageConfig = stageContext.StageConfig;
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        private async UniTask InitializeAsync(TaskCompletionSource<bool> initializationTcs)
        {
            try
            {
                if (!string.IsNullOrEmpty(_stageConfig.StartRoomDataLabel.labelString))
                {
                    var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.StartRoomDataLabel.labelString);
                    await handle.Task;
                    StartRoomDataAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                    Addressables.Release(handle);
                }

                if (!string.IsNullOrEmpty(_stageConfig.NormalRoomDataLabel.labelString))
                {
                    var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.NormalRoomDataLabel.labelString);
                    await handle.Task;
                    NormalRoomDataAddresses = handle.Result.Select(location => location.PrimaryKey).ToList();

                    Addressables.Release(handle);
                }

                if (!string.IsNullOrEmpty(_stageConfig.BossRoomDataLabel.labelString))
                {
                    var handle = Addressables.LoadResourceLocationsAsync(_stageConfig.BossRoomDataLabel.labelString);
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
using System;
using System.Collections.Generic;
using System.Linq;
using Jin5eok;
using UnityEngine;
using UnityEngine.AddressableAssets;
using VContainer;

namespace DungeonShooter
{
    public interface IRoomDataRepository : IDisposable
    {
        Awaitable<RoomData> GetRandomRoom(RoomType type);
    }

    public class RoomDataRepository : IRoomDataRepository
    {
        private AddressablesScope _addressablesScope = new AddressablesScope();
        private StageConfig _stageConfig;
        private List<string> StartRoomDataAddresses { get; set; }
        private List<string> NormalRoomDataAddresses { get; set; }
        private List<string> BossRoomDataAddresses { get; set; }

        public RoomDataRepository(StageConfig config)
        {
            _stageConfig = config;
        }

        /// <summary>
        /// StageConfig의 Label 데이터를 기반으로 에셋의 어드레스 목록을 로드하여 저장합니다.
        /// </summary>
        public async Awaitable InitializeAsync()
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
        }

        /// <summary>
        /// 타입에 맞는 랜덤한 방을 반환합니다.
        /// </summary>
        /// <param name="type">방 타입</param>
        /// <returns>방 데이터</returns>
        public async Awaitable<RoomData> GetRandomRoom(RoomType type)
        {
            var targetAddresses = type == RoomType.Start ? StartRoomDataAddresses : type == RoomType.Normal ? NormalRoomDataAddresses : BossRoomDataAddresses;
            var targetAddress = targetAddresses[UnityEngine.Random.Range(0, targetAddresses.Count)];
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
using System;
using Jin5eok;
using UnityEngine;

using VContainer;

namespace DungeonShooter
{
    public interface IRoomDataRepository : IDisposable
    {
        Awaitable<RoomData> GetRandomRoom(RoomType type);
    }

    public class RoomDataRepository : IRoomDataRepository
    {
        AddressablesScope _addressablesScope = new AddressablesScope();
        private string[] _startRoomAddresses;
        private string[] _normalRoomAddresses;
        private string[] _bossRoomAddresses;

        [Inject]
        public RoomDataRepository(string[] startRoomAddresses, string[] normalRoomAddresses, string[] bossRoomAddresses)
        {
            _startRoomAddresses = startRoomAddresses;
            _normalRoomAddresses = normalRoomAddresses;
            _bossRoomAddresses = bossRoomAddresses;
        }

        /// <summary>
        /// 타입에 맞는 랜덤한 방을 반환합니다.
        /// </summary>
        /// <param name="type">방 타입</param>
        /// <returns>방 데이터</returns>
        public async Awaitable<RoomData> GetRandomRoom(RoomType type)
        {
            var targetAddresses = type == RoomType.Start ? _startRoomAddresses : type == RoomType.Normal ? _normalRoomAddresses : _bossRoomAddresses;
            var targetAddress = targetAddresses[UnityEngine.Random.Range(0, targetAddresses.Length)];
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
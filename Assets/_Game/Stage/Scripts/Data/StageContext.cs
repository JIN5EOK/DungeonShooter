using System;
using Jin5eok;
using UnityEngine;

namespace DungeonShooter
{
    /// <summary>
    /// StageConfig을 포함한 스테이지 구성에 필요한 문맥정보
    /// 런타임에 지정될만한 정보들을 담는다
    /// </summary>
    public class StageContext : IDisposable
    {
        private readonly AddressablesScope _scope = new AddressablesScope();
        public StageConfig StageConfig { get; private set; }
        
        public async Awaitable<bool> LoadConfigAsync(string configKey)
        {
            var handle = _scope.LoadAssetAsync<StageConfig>(configKey);
            StageConfig = await handle.Task;
            return handle.IsValid();
        }

        public void Dispose()
        {
            _scope?.Dispose();
        }
    }
}

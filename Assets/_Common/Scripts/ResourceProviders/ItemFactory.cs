using Cysharp.Threading.Tasks;
using VContainer;

namespace DungeonShooter
{
    public interface IItemFactory
    {
        public UniTask<Item> CreateItemAsync(int itemId);
    }
    /// <summary>
    /// Item 인스턴스를 생성하는 팩토리
    /// </summary>
    public class ItemFactory : IItemFactory
    {
        private readonly ITableRepository _tableRepository;
        private readonly ISkillFactory _skillFactory;

        [Inject]
        public ItemFactory(ITableRepository tableRepository, ISkillFactory skillFactory)
        {
            _tableRepository = tableRepository;
            _skillFactory = skillFactory;
        }

        /// <summary>
        /// 아이템 ID를 기반으로 Item을 생성하고 초기화합니다.
        /// </summary>
        /// <param name="itemEntryId">아이템 테이블 엔트리 ID</param>
        /// <returns>생성 및 초기화된 Item 인스턴스</returns>
        public async UniTask<Item> CreateItemAsync(int itemEntryId)
        {
            if (_tableRepository == null)
            {
                LogHandler.LogError<ItemFactory>("TableRepository가 null입니다.");
                return null;
            }

            // ItemTableEntry 조회
            var itemTableEntry = _tableRepository.GetTableEntry<ItemTableEntry>(itemEntryId);
            if (itemTableEntry == null)
            {
                LogHandler.LogError<ItemFactory>($"ItemTableEntry를 찾을 수 없습니다: {itemEntryId}");
                return null;
            }

            var item = new Item(itemTableEntry, _skillFactory);
            await item.InitializeSkillsAsync();
            
            return item;
        }
    }
}

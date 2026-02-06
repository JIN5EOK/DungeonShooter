using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

namespace DungeonShooter
{
    /// <summary>
    /// 게임시작 UI, 스테이지와 캐릭터를 선택후 게임을 시작합니다
    /// </summary>
    public class GameStartUI : PopupUI
    {
        [Header("캐릭터 버튼")]
        [SerializeField] private RectTransform _characterButtonContent;
        [SerializeField] private GameObject _characterButtonPrefab;

        [Header("스테이지 버튼")]
        [SerializeField] private RectTransform _stageButtonContent;
        [SerializeField] private GameObject _stageButtonPrefab;

        [Header("게임 시작")]
        [SerializeField] private Button _gameStartButton;

        [Header("캐릭터 미리보기 프리펩 관련 세팅")]
        [SerializeField] private Transform _previewRoot;
        [SerializeField] private Camera _previewCamera;
        [SerializeField] private RawImage _characterPreviewImage;
        [SerializeField] private int _previewTextureWidth = 512;
        [SerializeField] private int _previewTextureHeight = 512;
        private RenderTexture _previewRenderTexture;

        [Header("정보 패널")]
        [SerializeField] private TextMeshProUGUI _infoName;
        [SerializeField] private TextMeshProUGUI _infoDescription;
        [SerializeField] private TextMeshProUGUI _infoStartWeapon;
        [SerializeField] private TextMeshProUGUI _infoSkills;
        [SerializeField] private TextMeshProUGUI _infoStats;

        private readonly List<Button> _characterButtons = new();
        private readonly List<Button> _stageButtons = new();
        private GameObject _previewInstance;
        private ITableRepository _tableRepository;
        private ISceneResourceProvider _resourceProvider;
        private PlayerConfigTableEntry _selectedEntry;
        private StageConfigTableEntry _selectedStageEntry;

        /// <summary> 현재 선택된 플레이어 설정 엔트리 </summary>
        public PlayerConfigTableEntry SelectedPlayerConfigEntry => _selectedEntry;
        /// <summary> 현재 선택된 스테이지 설정 엔트리 </summary>
        public StageConfigTableEntry SelectedStageConfigEntry => _selectedStageEntry;
        /// <summary> 캐릭터 선택 시 이벤트 </summary>
        public event Action<PlayerConfigTableEntry> OnCharacterSelected;
        /// <summary> 스테이지 선택 시 이벤트 </summary>
        public event Action<StageConfigTableEntry> OnStageSelected;
        /// <summary> 게임 시작 버튼 클릭 시 이벤트 (선택된 캐릭터/스테이지로 실제 시작은 구독처에서 처리) </summary>
        public event Action OnGameStartRequested;

        [Inject]
        public void Construct(ITableRepository tableRepository, ISceneResourceProvider resourceProvider)
        {
            _tableRepository = tableRepository;
            _resourceProvider = resourceProvider;
            SetupPreviewCameraAndTexture();
            var characterEntries = _tableRepository.GetAllTableEntries<PlayerConfigTableEntry>();
            foreach (var entry in characterEntries)
            {
                AddCharacterButton(entry);
            }

            ClearStageButtons();
            var stageEntries = _tableRepository.GetAllTableEntries<StageConfigTableEntry>();
            foreach (var entry in stageEntries)
            {
                AddStageButton(entry);
            }

            SetCharacterButtonsInteractable(false);
            if (_gameStartButton != null)
                _gameStartButton.interactable = false;
            if (_gameStartButton != null)
                _gameStartButton.onClick.AddListener(GameStart);
        }

        private void SetupPreviewCameraAndTexture()
        {
            if (_previewCamera == null || _characterPreviewImage == null)
                return;

            if (_previewRenderTexture == null)
            {
                _previewRenderTexture = new RenderTexture(_previewTextureWidth, _previewTextureHeight, 16);
            }

            _previewCamera.targetTexture = _previewRenderTexture;
            _previewCamera.enabled = true;
            _characterPreviewImage.texture = _previewRenderTexture;
        }

        private void DestroyPreviewInstance()
        {
            if (_previewInstance != null)
            {
                Destroy(_previewInstance);
                _previewInstance = null;
            }
        }

        private async UniTask LoadAndShowPreviewAsync(PlayerConfigTableEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.GameObjectKey))
            {
                DestroyPreviewInstance();
                return;
            }

            if (_previewRoot == null || _resourceProvider == null)
                return;

            DestroyPreviewInstance();

            var go = await _resourceProvider.GetInstanceAsync(entry.GameObjectKey);
            if (go == null)
                return;

            go.transform.SetParent(_previewRoot, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            _previewInstance = go;
        }


        private void ClearStageButtons()
        {
            foreach (var button in _stageButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _stageButtons.Clear();

            if (_stageButtonContent == null)
                return;

            for (var i = _stageButtonContent.childCount - 1; i >= 0; i--)
            {
                var child = _stageButtonContent.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        private void AddStageButton(StageConfigTableEntry entry)
        {
            if (_stageButtonContent == null || _stageButtonPrefab == null)
                return;

            var go = Instantiate(_stageButtonPrefab, _stageButtonContent, false);
            var button = go.GetComponent<Button>();
            if (button == null)
                button = go.GetComponentInChildren<Button>(true);

            if (button != null)
            {
                var label = go.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    label.text = entry?.Name ?? string.Empty;

                var captured = entry;
                button.onClick.AddListener(() => SelectStage(captured));
                _stageButtons.Add(button);
            }
        }

        private void SelectStage(StageConfigTableEntry entry)
        {
            _selectedStageEntry = entry;
            SetCharacterButtonsInteractable(true);
            OnStageSelected?.Invoke(entry);
        }

        private void SetCharacterButtonsInteractable(bool interactable)
        {
            foreach (var button in _characterButtons)
            {
                if (button != null)
                    button.interactable = interactable;
            }
        }

        private void AddCharacterButton(PlayerConfigTableEntry entry)
        {
            if (_characterButtonContent == null || _characterButtonPrefab == null)
                return;

            var go = Instantiate(_characterButtonPrefab, _characterButtonContent, false);
            var button = go.GetComponent<Button>();
            if (button == null)
                button = go.GetComponentInChildren<Button>(true);

            if (button != null)
            {
                var label = go.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                    label.text = entry?.Name ?? string.Empty;

                var captured = entry;
                button.onClick.AddListener(() => SelectCharacter(captured));
                _characterButtons.Add(button);
            }
        }

        private void SelectCharacter(PlayerConfigTableEntry entry)
        {
            _selectedEntry = entry;
            RefreshInfoPanel();
            LoadAndShowPreviewAsync(entry).Forget();
            if (_gameStartButton != null)
                _gameStartButton.interactable = true;
            OnCharacterSelected?.Invoke(entry);
        }

        private void RefreshInfoPanel()
        {
            if (_selectedEntry == null)
            {
                SetInfoText(_infoName, string.Empty);
                SetInfoText(_infoDescription, string.Empty);
                SetInfoText(_infoStartWeapon, string.Empty);
                SetInfoText(_infoSkills, string.Empty);
                SetInfoText(_infoStats, string.Empty);
                return;
            }

            SetInfoText(_infoName, _selectedEntry.Name);
            SetInfoText(_infoDescription, _selectedEntry.Description);

            var weaponEntry = _tableRepository?.GetTableEntry<ItemTableEntry>(_selectedEntry.StartWeaponId);
            SetInfoText(_infoStartWeapon, weaponEntry?.ItemName ?? string.Empty);

            var skill1 = _tableRepository?.GetTableEntry<SkillTableEntry>(_selectedEntry.Skill1Id);
            var skill2 = _tableRepository?.GetTableEntry<SkillTableEntry>(_selectedEntry.Skill2Id);
            var skillNames = new List<string>();
            if (!string.IsNullOrEmpty(skill1?.SkillName))
                skillNames.Add(skill1.SkillName);
            if (!string.IsNullOrEmpty(skill2?.SkillName))
                skillNames.Add(skill2.SkillName);
            SetInfoText(_infoSkills, string.Join(", ", skillNames));

            var statsEntry = _tableRepository?.GetTableEntry<EntityStatsTableEntry>(_selectedEntry.StatsId);
            if (statsEntry != null)
            {
                var statsText = $"체력: {statsEntry.MaxHp}  공격력: {statsEntry.Attack}  방어력: {statsEntry.Defense}  이동속도: {statsEntry.MoveSpeed}";
                SetInfoText(_infoStats, statsText);
            }
            else
            {
                SetInfoText(_infoStats, string.Empty);
            }
        }

        private static void SetInfoText(TextMeshProUGUI textUi, string value)
        {
            if (textUi != null)
                textUi.text = value ?? string.Empty;
        }

        private void OnDestroy()
        {
            DestroyPreviewInstance();
            if (_previewRenderTexture != null)
            {
                _previewRenderTexture.Release();
                _previewRenderTexture = null;
            }
        }

        private void GameStart()
        {
            if (_selectedEntry == null || _selectedStageEntry == null)
                return;
            OnGameStartRequested?.Invoke();
        }
    }
}

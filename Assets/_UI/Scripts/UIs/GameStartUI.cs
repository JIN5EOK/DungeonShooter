using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using _MainMenu;

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
        private IGameStartService _gameStartService;
        private ITableRepository _tableRepository;
        private ISceneResourceProvider _resourceProvider;

        /// <summary> 현재 선택된 플레이어 설정 엔트리 </summary>
        public PlayerConfigTableEntry SelectedPlayerConfigEntry => _gameStartService?.SelectedPlayer;
        /// <summary> 현재 선택된 스테이지 설정 엔트리 </summary>
        public StageConfigTableEntry SelectedStageConfigEntry => _gameStartService?.SelectedStage;
        /// <summary> 캐릭터 선택 시 이벤트 </summary>
        public event Action<PlayerConfigTableEntry> OnCharacterSelected;
        /// <summary> 스테이지 선택 시 이벤트 </summary>
        public event Action<StageConfigTableEntry> OnStageSelected;

        [Inject]
        public void Construct(IGameStartService gameStartService, ITableRepository tableRepository, ISceneResourceProvider resourceProvider)
        {
            _gameStartService = gameStartService;
            _tableRepository = tableRepository;
            _resourceProvider = resourceProvider;
            SetupPreviewCameraAndTexture();

            var characterEntries = _gameStartService.GetSelectablePlayers();
            foreach (var entry in characterEntries)
            {
                AddCharacterButton(entry);
            }

            ClearStageButtons();
            var stageEntries = _gameStartService.GetSelectableStages();
            foreach (var entry in stageEntries)
            {
                AddStageButton(entry);
            }

            SetCharacterButtonsInteractable(false);
            if (_gameStartButton != null)
                _gameStartButton.interactable = false;
            if (_gameStartButton != null)
                _gameStartButton.onClick.AddListener(OnGameStartButtonClicked);
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

        private void LoadAndShowPreview(PlayerConfigTableEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.GameObjectKey))
            {
                DestroyPreviewInstance();
                return;
            }

            if (_previewRoot == null || _resourceProvider == null)
                return;

            DestroyPreviewInstance();

            var go = _resourceProvider.GetInstanceSync(entry.GameObjectKey);
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
                    label.text = entry != null ? _tableRepository.GetStringText(entry.NameId) : string.Empty;

                var captured = entry;
                button.onClick.AddListener(() => SelectStage(captured));
                _stageButtons.Add(button);
            }
        }

        private void SelectStage(StageConfigTableEntry entry)
        {
            if (_gameStartService != null)
                _gameStartService.SelectedStage = entry;
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
                    label.text = entry != null ? _tableRepository.GetStringText(entry.NameId) : string.Empty;

                var captured = entry;
                button.onClick.AddListener(() => SelectCharacter(captured));
                _characterButtons.Add(button);
            }
        }

        private void SelectCharacter(PlayerConfigTableEntry entry)
        {
            if (_gameStartService != null)
                _gameStartService.SelectedPlayer = entry;
            RefreshInfoPanel();
            LoadAndShowPreview(entry);
            if (_gameStartButton != null)
                _gameStartButton.interactable = true;
            OnCharacterSelected?.Invoke(entry);
        }

        private void RefreshInfoPanel()
        {
            var selectedEntry = _gameStartService?.SelectedPlayer;
            if (selectedEntry == null)
            {
                SetInfoText(_infoName, string.Empty);
                SetInfoText(_infoDescription, string.Empty);
                SetInfoText(_infoStartWeapon, string.Empty);
                SetInfoText(_infoSkills, string.Empty);
                SetInfoText(_infoStats, string.Empty);
                return;
            }

            SetInfoText(_infoName, _tableRepository.GetStringText(selectedEntry.NameId));
            SetInfoText(_infoDescription, _tableRepository.GetStringText(selectedEntry.DescriptionId));

            var weaponEntry = _tableRepository?.GetTableEntry<ItemTableEntry>(selectedEntry.StartWeaponId);
            SetInfoText(_infoStartWeapon, weaponEntry != null ? _tableRepository.GetStringText(weaponEntry.ItemNameId) : string.Empty);

            var skill1 = _tableRepository?.GetTableEntry<SkillTableEntry>(selectedEntry.Skill1Id);
            var skill2 = _tableRepository?.GetTableEntry<SkillTableEntry>(selectedEntry.Skill2Id);
            var skillNames = new List<string>();
            if (skill1 != null)
                skillNames.Add(_tableRepository.GetStringText(skill1.SkillNameId));
            if (skill2 != null)
                skillNames.Add(_tableRepository.GetStringText(skill2.SkillNameId));
            SetInfoText(_infoSkills, string.Join(", ", skillNames));

            var statsEntry = _tableRepository?.GetTableEntry<EntityStatsTableEntry>(selectedEntry.StatsId);
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DestroyPreviewInstance();
            if (_previewRenderTexture != null)
            {
                _previewRenderTexture.Release();
                _previewRenderTexture = null;
            }
        }

        private void OnGameStartButtonClicked()
        {
            if (_gameStartService == null)
                return;
            _gameStartService.GameStart().Forget();
        }
    }
}

using System;
using VContainer;

namespace DungeonShooter
{
    public class StageEventMediator : IDisposable
    {
        private readonly IPauseManager _pauseManager;
        private readonly IPlayerLevelService _playerLevelService;
        private readonly ISkillService _skillService;
        private readonly SkillLevelUpUI _skillLevelUpUI;

        private Player _player;

        [Inject]
        public StageEventMediator(
            IPauseManager pauseManager,
            IPlayerLevelService playerLevelService,
            ISkillService skillService,
            SkillLevelUpUI skillLevelUpUI)
        {
            _pauseManager = pauseManager;
            _playerLevelService = playerLevelService;
            _skillService = skillService;
            _skillLevelUpUI = skillLevelUpUI;

            _playerLevelService.OnLevelChanged += OnLevelChanged;
        }

        public void RegisterPlayer(Player player)
        {
            _player = player;
        }

        private void OnLevelChanged(int level)
        {
            ShowLevelUpSelection();
        }
        
        private void ShowLevelUpSelection()
        {
            var skills = _player?.GetContext().Skills.GetSkills();
            if (skills == null || skills.Count == 0)
            {
                ResumeGame();
                return;
            }

            var levelUpableSkills = _skillService.GetLevelUpableSkills(skills);
            if (levelUpableSkills.Count == 0)
            {
                ResumeGame();
                return;
            }

            _pauseManager.PauseRequest(this);
            _skillLevelUpUI.Show();
            _skillLevelUpUI.ShowLevelUpSkillOptions(levelUpableSkills, OnSkillSelected);
        }

        private void OnSkillSelected(Skill skill)
        {
            skill.LevelUp();
            _pauseManager.PauseRequest(this);
        }

        private void ResumeGame()
        {
            _pauseManager.ResumeRequest(this);
        }

        public void Dispose()
        {
            _playerLevelService.OnLevelChanged -= OnLevelChanged;
            _pauseManager.ResumeRequest(this);
        }
    }
}

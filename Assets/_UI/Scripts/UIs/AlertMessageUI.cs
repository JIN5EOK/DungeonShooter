using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    public class AlertMessageViewModel
    {
        public event Action<string> OnMessageSet;
        
        public void SetMessage(string message)
        {
            OnMessageSet?.Invoke(message);
        }
    }
    
    public class AlertMessageUI : HudUI
    {
        [SerializeField]
        private Image _messagePanel;
        [SerializeField]
        private TMP_Text _text;
        private AlertMessageViewModel _viewModel;
        private ISoundSfxService _soundSfxService;

        [Inject]
        public void Construct(AlertMessageViewModel viewModel, ISoundSfxService soundSfxService)
        {
            _viewModel = viewModel;
            _soundSfxService = soundSfxService;
            _viewModel.OnMessageSet += ShowMessage;
        }

        public void ShowMessage(string message)
        {
            _soundSfxService?.PlayOneShot(AudioAddresses.AlertSound);
            _text.text = message;
            _messagePanel.gameObject.SetActive(true);
            _messagePanel.DOKill();
            _messagePanel.color = Color.white;
            _messagePanel.DOColor(Color.white, 2.0f).OnComplete(() =>
            {
                _messagePanel.DOColor(Color.clear, 1.0f).OnComplete(() => _messagePanel.gameObject.SetActive(false));    
            });
        }

        public override void Destroy()
        {
            base.Destroy();
            if (_viewModel != null)
            {
                _viewModel.OnMessageSet -= ShowMessage;
            }
        }
    }
}
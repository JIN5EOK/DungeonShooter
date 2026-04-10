using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace DungeonShooter
{
    public class AlertMessageView : HudUI
    {
        [SerializeField]
        private Image _messagePanel;
        [SerializeField]
        private TMP_Text _text;
        private ISoundSfxService _soundSfxService;

        [Inject]
        public void Construct(ISoundSfxService soundSfxService)
        {
            _soundSfxService = soundSfxService;
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
    }
}
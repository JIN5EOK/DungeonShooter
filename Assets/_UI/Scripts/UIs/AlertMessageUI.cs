using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
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
        private Transform _messageTransform;
        [SerializeField]
        private TMP_Text _text;
        private AlertMessageViewModel _viewModel;
        
        [Inject]
        public void Construct(AlertMessageViewModel viewModel)
        {
            _viewModel.OnMessageSet += ShowMessage;
        }
        
        public void ShowMessage(string message)
        {
            _messageTransform.gameObject.SetActive(true);
            _text.DOKill();
            _text.color = Color.white;
            _text.DOColor(Color.white, 2.0f);
            _text.DOColor(Color.clear, 1.0f).OnComplete(() => _messageTransform.gameObject.SetActive(false));
        }

        public override void Destroy()
        {
            base.Destroy();
            _viewModel.OnMessageSet -= ShowMessage;
        }
    }
}
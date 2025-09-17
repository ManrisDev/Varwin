using Unity.Netcode;
using Varwin.Public;

namespace Varwin.Multiplayer
{
    public class NetworkVarwinBotTextBubble : NetworkBehaviour
    {
        private VarwinBotTextBubble _targetBehaviour;

        public NetworkVariable<bool> ShowBubble = new();
        public NetworkVariable<bool> ContainerIsActive = new();

        public NetworkVariable<VarwinBot.TextBubbleHideType> HideType = new();

        public override void OnNetworkSpawn()
        {
            _targetBehaviour = GetComponent<VarwinBotTextBubble>();

            if (!IsServer)
            {
                ShowBubble.OnValueChanged += OnShowBubbleValueChanged;
                ContainerIsActive.OnValueChanged += OnContainerIsActiveChanged;
                HideType.OnValueChanged += OnHideTypeChanged;
            }
            else
            {
                _targetBehaviour.SpeechStarted += OnSpeechStarted;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer)
            {
                return;
            }

            if (ShowBubble.Value != _targetBehaviour.ShowTextBubble)
            {
                ShowBubble.Value = _targetBehaviour.ShowTextBubble;
            }

            if (HideType.Value != _targetBehaviour.HideType)
            {
                HideType.Value = _targetBehaviour.HideType;
            }

            if (ContainerIsActive.Value != _targetBehaviour.Container.activeSelf)
            {
                ContainerIsActive.Value = _targetBehaviour.Container.activeSelf;
            }
        }

        private void OnHideTypeChanged(VarwinBot.TextBubbleHideType previousValue, VarwinBot.TextBubbleHideType newValue)
        {
            _targetBehaviour.HideType = newValue;
        }

        private void OnContainerIsActiveChanged(bool previousValue, bool newValue)
        {
            _targetBehaviour.Container.SetActive(newValue);
        }
        
        private void OnSpeechStarted(string title, string text)
        {
            ShowBubbleClientRpc(title, text, NetworkManager.LocalClient.ClientId);
        }

        [ClientRpc]
        private void ShowBubbleClientRpc(string title, string text, ulong clientId)
        {
            if (NetworkManager.LocalClient.ClientId == clientId)
            {
                return;
            }
            
            _targetBehaviour.ShowText(title, text);
        }

        private void OnShowBubbleValueChanged(bool previousValue, bool newValue)
        {
            _targetBehaviour.ShowTextBubble = newValue;
        }

        public override void OnDestroy()
        {
            if (!IsServer)
            {
                ShowBubble.OnValueChanged -= OnShowBubbleValueChanged;
            }
            else
            {
                _targetBehaviour.SpeechStarted -= OnSpeechStarted;
            }
        }
    }
}
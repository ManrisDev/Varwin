using Unity.Netcode;
using Varwin.Public;

namespace Varwin.Multiplayer
{
    public class NetworkVarwinBot : NetworkBehaviour
    {
        private VarwinBot _targetBehaviour;
        
        private VarwinBot.MovementDirection _movementDirection;
        private VarwinBot.MovementPace _movementPace;
        private VarwinBot.MovementType _movementType;
        private VarwinBot.RotationDirection _rotationDirection;
        private VarwinBot.TextBubbleHideType _textBubbleHideType;

        public override void OnNetworkSpawn()
        {
            _targetBehaviour = gameObject.GetComponent<VarwinBot>();

            if (!IsServer)
            {
                _targetBehaviour.CharacterController.enabled = false;
            }
        }

        private void FixedUpdate()
        {
            if (!IsServer)
            {
                return;
            }

            if (_movementPace != _targetBehaviour.CurrentMovementPace)
            {
                _movementPace = _targetBehaviour.CurrentMovementPace;
            }

            if (_movementDirection != _targetBehaviour.CurrentMovementDirection)
            {
                _movementDirection = _targetBehaviour.CurrentMovementDirection;
            }
            
            if (_movementType != _targetBehaviour.CurrentMovementType)
            {
                _movementType = _targetBehaviour.CurrentMovementType;
                if (_movementType == VarwinBot.MovementType.None)
                {
                    StopMovementClientRpc();
                }
                else
                {
                    MoveBotClientRpc(_movementDirection, _movementPace);
                }
            }
        }

        [ClientRpc]
        private void MoveBotClientRpc(VarwinBot.MovementDirection movementDirection, VarwinBot.MovementPace movementPace)
        {
            _targetBehaviour.MoveBot(movementDirection, movementPace);            
        }

        [ClientRpc]
        private void StopMovementClientRpc()
        {
            _targetBehaviour.StopMovement();
        }
    }
}
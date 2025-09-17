using UnityEngine;
using Varwin.DesktopPlayer;

namespace Varwin.NettleDeskPlayer
{
    public class NettleDeskLocomotionController : LocomotionController
    {
        public delegate void EventHandler();

        public event EventHandler PositionUpdated;
        
        [SerializeField] private DesktopPlayerInput _input;
        
        protected override Vector2 Movement => _input.PlayerMovement;

        protected override float Speed
        {
            get
            {
                var speed = PlayerManager.WalkSpeed;
                if (_input.IsCrouching)
                {
                    speed = PlayerManager.CrouchSpeed;
                }
                else if (_input.IsSprinting)
                {
                    speed = PlayerManager.SprintSpeed;
                }

                return speed;
            }
        }
        
        protected override bool IsJumping => _input.IsJumping;
        protected override float Height => _height;

        private float _height = 0f;

        private void Awake()
        {
            _height = PlayerManager.PlayerNormalHeight;
        }

        protected override void OnUpdate()
        {
            var height = _input.IsCrouching ? PlayerManager.PlayerNormalHeight / 2f: PlayerManager.PlayerNormalHeight;

            _height = Mathf.Lerp(_height, height, Time.deltaTime * 10f);
            
            Camera.transform.localPosition = Vector3.up * GetClampedHeight();
            
            PositionUpdated?.Invoke();
        }

        protected override Vector3 TransformDirection(Vector3 direction)
        {
            return transform.rotation * direction;
        }

        protected override void ApplyMovement(Vector3 movement)
        {
            transform.position += movement;
        }
    }
}
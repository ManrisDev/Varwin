using UnityEngine;
using UnityEngine.SceneManagement;

namespace Varwin.XR
{
    /// <summary>
    /// Контроллер locomotion.
    /// </summary>
    public class VarwinXRPlayerMoveTypeLocomotion : VarwinXRPlayerMoveBase
    {
        /// <summary>
        /// Ключ хранения настроек типа.
        /// </summary>
        private const string SettingsKey = "vw.player_locomotion_vignette_type";
        
        /// <summary>
        /// Ключ локализации названия.
        /// </summary>
        public override string LocalizationNameKey => "PLAYER_MOVE_TYPE_LOCOMOTION";

        /// <summary>
        /// Управление гравитацией игрока.
        /// </summary>
        [SerializeField] private VarwinXRPlayerMoveController _controller;

        /// <summary>
        /// Текущее перемещение.
        /// </summary>
        private Vector2 _movement;

        /// <summary>
        /// Виньетка.
        /// </summary>
        public VarwinXRVignette Vignette;
        
        /// <summary>
        /// Тип виньетки.
        /// </summary>
        private VignetteType _vignetteType;

        /// <summary>
        /// Тип виньетки.
        /// </summary>
        public VignetteType VignetteType
        {
            get => GetVignetteType();
            set => SetVignetteType(value);
        }

        /// <summary>
        /// Деактивация телепорта на контроллере.
        /// </summary>
        protected override void OnEnable()
        {
            _leftController.InvokeRotate = false;
            _leftController.InvokeTeleport = false;
            _rightController.InvokeTeleport = false;
            
            Vignette.enabled = VignetteType != VignetteType.Off;
        }

        /// <summary>
        /// Скрытие виньетки.
        /// </summary>
        protected override void OnDisable()
        {
            Vignette.enabled = false;
            Vignette.Renderer.enabled = false;
            
            _movement = Vector2.zero;
            _controller.SetMovement(Vector2.zero);
        }

        /// <summary>
        /// Обновление перемещения.
        /// </summary>
        private void Update()
        {
            UpdateMovement();

            var isJumpPressed = !_rightController.Is3Dof && (_rightController.TeleportOnThumbstick ? _rightController.IsPrimaryButtonPressed : !_rightController.IsTurnPressed && _rightController.IsPrimary2DAxisPressed);

            _player.SetJumpingState(isJumpPressed);
        }

        /// <summary>
        /// Обновление перемещения.
        /// </summary>
        private void UpdateMovement()
        {
            Vector2 velocity = default;

            if (PlayerManager.MovementEnabled)
            {
                var primaryAxis = _leftController.Is3Dof ? Vector2.ClampMagnitude(_leftController.Primary2DAxisValue + _rightController.Primary2DAxisValue, 1f) : _leftController.Primary2DAxisValue;
                var primaryAxisPressed = _leftController.Is3Dof ? _leftController.IsPrimary2DAxisPressed | _rightController.IsPrimary2DAxisPressed : _leftController.IsPrimary2DAxisPressed;
            
                var forwardVector = Vector3.ProjectOnPlane(_player.Camera.transform.forward, Vector3.up).normalized * primaryAxis.y;
                var rightVector = Vector3.ProjectOnPlane(_player.Camera.transform.right, Vector3.up).normalized * primaryAxis.x;

                var direction = forwardVector + rightVector;
                
                velocity = primaryAxisPressed ? Vector2.zero : new Vector2(direction.x, direction.z);
            }

            _movement = Vector2.Lerp(_movement, velocity, Time.deltaTime * 10f);
            _player.SetMovement(_movement);

            if (VignetteType == VignetteType.Off)
            {
                return;
            }
            
            Vignette.SetForce(SceneManager.GetActiveScene().buildIndex != -1 ? 0 : Mathf.Clamp01(_movement.magnitude));
            Vignette.SetType(VignetteType);
        }
        
        /// <summary>
        /// Задать тип виньетки.
        /// </summary>
        /// <param name="value">Тип виньетки.</param>
        private void SetVignetteType(VignetteType value)
        {
            if (!enabled)
            {
                return;
            }
            
            Vignette.enabled = value != VignetteType.Off;
            _vignetteType = value;
            PlayerPrefs.SetInt(SettingsKey, (int) _vignetteType);
        }

        /// <summary>
        /// Возвращает тип виньетки. Если сохраненного нет, то fallback на Strong.
        /// </summary>
        /// <returns>Тип виньетки.</returns>
        private VignetteType GetVignetteType()
        {
            if (!PlayerPrefs.HasKey(SettingsKey))
            {
                SetVignetteType(VignetteType.Strong);
                return VignetteType.Strong;
            }

            _vignetteType = (VignetteType) PlayerPrefs.GetInt(SettingsKey);
            return _vignetteType;
        }
    }
}
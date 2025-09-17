using System;
using Illumetry.Unity.Stylus;
using UnityEngine;

namespace Varwin.NettleDeskPlayer
{
    /// <summary>
    /// Контроллер ввода со стилусов.
    /// </summary>
    public class NettleDeskStylusInputHandler : MonoBehaviour
    {
        /// <summary>
        /// Время, необходимое для удержании кнопки на стилусе для граба объекта.
        /// </summary>
        private const float TimeForGrabPressing = 0.75f;
        
        /// <summary>
        /// Объект стилуса.
        /// </summary>
        private Stylus _stylus;

        /// <summary>
        /// Нажата ли кнопка на стилусе.
        /// </summary>
        private bool _isPressed;
        
        /// <summary>
        /// Предыдущее состояние кнопки стилуса.
        /// </summary>
        private bool _previousIsPressed;
        
        /// <summary>
        /// Время нажатия.
        /// </summary>
        private DateTime _pressedTime;
        
        /// <summary>
        /// Событие, вызываемое при инициализации.
        /// </summary>
        public event Action Initialized;

        /// <summary>
        /// Событие, вызываемое при деинициализации.
        /// </summary>
        public event Action Deinitialized;
        
        /// <summary>
        /// Событие, вызываемое при изменении позы (позиции и ориентации).
        /// </summary>
        public event Action<Pose> PoseChanged;
        
        /// <summary>
        /// Событие, вызываемое при нажатии на использование.
        /// </summary>
        public event Action UsePressed;

        /// <summary>
        /// Событие, вызываемое при отпускании использования.
        /// </summary>
        public event Action UseReleased;
        
        /// <summary>
        /// Событие, вызываемое при нажатии на захват.
        /// </summary>
        public event Action GrabPressed;

        /// <summary>
        /// Событие, вызываемое при отпускании кнопки захвата.
        /// </summary>
        public event Action GrabReleased;

        /// <summary>
        /// Нажата ли кнопка использования.
        /// </summary>
        public bool IsUsePressed { get; private set; }

        /// <summary>
        /// Отпущена ли кнопка использования.
        /// </summary>
        public bool IsUseReleased { get; private set; }
        
        /// <summary>
        /// Нажата ли кнопка захвата.
        /// </summary>
        public bool IsGrabPressed { get; private set; }

        /// <summary>
        /// Отпущена ли кнопка использования.
        /// </summary>
        public bool IsGrabReleased { get; private set; }

        /// <summary>
        /// Инцииализация событий контроллера.
        /// </summary>
        /// <param name="stylus">Целевой стилус.</param>
        public void Initialize(Stylus stylus)
        {
            if (!stylus || _stylus)
            {
                return;
            }
            
            stylus.OnUpdatedButtonPhase += OnUpdatedButtonState;
            stylus.OnDestroying += OnDestroying;
            stylus.OnUpdatedPose += OnUpdatedPose;
            Initialized?.Invoke();
            _stylus = stylus;
        }

        /// <summary>
        /// Деинициализация стилуса и отписка.
        /// </summary>
        public void Deinitialize()
        {
            if (!_stylus)
            {
                return;
            }
            
            _stylus.OnUpdatedButtonPhase -= OnUpdatedButtonState;
            _stylus.OnDestroying -= OnDestroying;
            _stylus.OnUpdatedPose -= OnUpdatedPose;
            _stylus = null;
            Deinitialized?.Invoke();
        }

        /// <summary>
        /// Обновление состояния, а также вызов событий стилуса.
        /// </summary>
        private void Update()
        {
            ProcessGrab();
            ProcessUse();

            _previousIsPressed = _isPressed;
        }

        /// <summary>
        /// Обновление состояния использования.
        /// </summary>
        private void ProcessUse()
        {
            IsUseReleased = false;
            if (!_isPressed && IsUsePressed)
            {
                IsUsePressed = false;
                IsUseReleased = true;
                UseReleased?.Invoke();
            }
            
            if (_previousIsPressed && !_isPressed && !IsGrabPressed)
            {
                IsUsePressed = true;
                UsePressed?.Invoke();
            }
        }
        
        /// <summary>
        /// Обновление состояния захвата.
        /// </summary>
        private void ProcessGrab()
        {
            IsGrabReleased = false;
            if (_isPressed && (DateTime.Now - _pressedTime).TotalSeconds > TimeForGrabPressing && !IsGrabPressed)
            {
                IsGrabPressed = true;
                GrabPressed?.Invoke();
            }

            if (!_isPressed && IsGrabPressed)
            {
                IsGrabPressed = false;
                IsGrabReleased = true;
                GrabReleased?.Invoke();
            }
        }

        /// <summary>
        /// При обновлении позы, вызывается соответствующее событие.
        /// </summary>
        private void OnUpdatedPose(Pose pose, Vector3 velocity, Vector3 angularVelocity)
        {
            PoseChanged?.Invoke(pose);
        }
        
        /// <summary>
        /// Метод, вызываемый при обновлении состояния кнопки на стилусе.
        /// </summary>
        /// <param name="stylus">Стилус.</param>
        /// <param name="state">Состояние.</param>
        private void OnUpdatedButtonState(Stylus stylus, bool state)
        {
            if (state && !_isPressed)
            {
                _pressedTime = DateTime.Now;
            }

            _isPressed = state;
        }
        
        /// <summary>
        /// Отписка при удалении.
        /// </summary>
        /// <param name="stylus">Стилус.</param>
        private void OnDestroying(Stylus stylus)
        {
            Deinitialize();
        }

        /// <summary>
        /// При уничтожении объекта отписка.
        /// </summary>
        private void OnDestroy()
        {
            Deinitialize();
        }
    }
}
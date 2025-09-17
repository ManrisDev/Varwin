using Unity.Netcode;
using UnityEngine;
using Varwin.Multiplayer.Types;

namespace Varwin.Multiplayer
{
    public class NetworkAnimation : NetworkBehaviour
    {
        #region Syncronized Variables
        /// <summary>
        /// Синхронизируемый компонент анимации
        /// </summary>
        private Animation _animation;

        /// <summary>
        /// Скорость анимации
        /// </summary>
        private NetworkVariable<float> _animationSpeed = new();

        /// <summary>
        /// Режим воспроизведения
        /// </summary>
        private NetworkVariable<WrapMode> _wrapMode = new();

        /// <summary>
        /// Флаг паузы анимации
        /// </summary>
        private NetworkVariable<bool> _isPlaying = new();

        private string _clipName;

        #endregion

        /// <summary>
        /// Инициализация сетевого копонента
        /// </summary>
        /// <param name="animationComponent">Синхронизируемый компонент</param>
        public void Initialize(Animation animationComponent)
        {
            _animation = animationComponent;

            if (IsServer)
            {
                var clip = _animation.clip;
                _clipName = clip.name;
                _animationSpeed.Value = _animation[clip.name].speed;
                _wrapMode.Value = _animation.wrapMode;
                _isPlaying.Value = _animation.isPlaying;

                return;
            }

            _animationSpeed.OnValueChanged += OnAnimationSpeedChanged;
            _wrapMode.OnValueChanged += OnWrapModeChanged;
            _isPlaying.OnValueChanged += OnIsPlayingChanged;
        }

        private void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (_animation.clip && _animation.clip.name != _clipName)
            {
                _clipName = _animation.clip.name;
                ChangeClipNameClientRpc(_clipName);
            }

            var animationState = _animation[_clipName];
            if (animationState && Mathf.Abs(animationState.speed - _animationSpeed.Value) > 0.01f)
            {
                _animationSpeed.Value = animationState.speed;
            }

            if (_animation.wrapMode != _wrapMode.Value)
            {
                _wrapMode.Value = _animation.wrapMode;
            }

            if (_animation.isPlaying != _isPlaying.Value)
            {
                _isPlaying.Value = _animation.isPlaying;
            }
        }

        #region Network Variables Callbacks

        private void OnIsPlayingChanged(bool oldValue, bool newValue)
        {
            var isPlaying = newValue;

            if (isPlaying && !_animation.isPlaying)
            {
                _animation.Play(_clipName);
            }
            else if(!isPlaying && _animation.isPlaying)
            {
                _animation.Stop();
            }
        }

        private void OnWrapModeChanged(WrapMode oldValue, WrapMode newValue)
        {
            _animation.wrapMode = newValue;
        }

        private void OnAnimationSpeedChanged(float oldValue, float newValue)
        {
            foreach (AnimationState animationState in _animation)
            {
                _animation[animationState.clip.name].speed = newValue;
            }
        }

        #endregion

        [ClientRpc]
        private void ChangeClipNameClientRpc(string clipName)
        {
            _animation.clip = _animation.GetClip(clipName);
            _clipName = clipName;
        }

        public override void OnDestroy()
        {
            _animationSpeed.OnValueChanged -= OnAnimationSpeedChanged;
            _wrapMode.OnValueChanged -= OnWrapModeChanged;
            _isPlaying.OnValueChanged -= OnIsPlayingChanged;
        }
    }
}
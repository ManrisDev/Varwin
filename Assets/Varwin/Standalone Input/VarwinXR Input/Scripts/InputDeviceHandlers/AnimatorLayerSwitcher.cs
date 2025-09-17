using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin.XR
{
    /// <summary>
    /// Переключатель слоев в аниматоре.
    /// </summary>
    public class AnimatorLayerSwitcher : MonoBehaviour
    {
        /// <summary>
        /// Целевой объект-аниматор.
        /// </summary>
        public Animator Animator;
        
        /// <summary>
        /// Индикаторы захвата внимания аниматора.
        /// </summary>
        public List<InputDeviceHandlerBase> Indicators;

        /// <summary>
        /// Активный элемент управления.
        /// </summary>
        private InputDeviceHandlerBase _activeInputHandler;
        
        /// <summary>
        /// Время переключения кнопок.
        /// </summary>
        public float TimeSwitching = 0.1f;

        /// <summary>
        /// Подпрограмма переключения.
        /// </summary>
        private Coroutine _switchCoroutine;

        /// <summary>
        /// Подписка на элементы управления.
        /// </summary>
        private void Awake()
        {
            Indicators.ForEach(a => a.Used += OnUsed);
        }

        /// <summary>
        /// При использовании смена слоя в аниматоре.
        /// </summary>
        /// <param name="inputDeviceHandlerBase">Использованный элемент управления.</param>
        private void OnUsed(InputDeviceHandlerBase inputDeviceHandlerBase)
        {
            _activeInputHandler = inputDeviceHandlerBase;
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (_switchCoroutine != null)
            {
                StopCoroutine(_switchCoroutine);
            }

            _switchCoroutine = StartCoroutine(Switch());
        }

        /// <summary>
        /// Переключение через заданное время.
        /// </summary>
        private IEnumerator Switch()
        {
            var time = 0f;
            while (time < TimeSwitching)
            {
                SetWeight(Mathf.Clamp01(Mathf.InverseLerp(0, TimeSwitching, time)));
                time += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            SetWeight(1);
        }

        /// <summary>
        /// Задать переключение слоев.
        /// </summary>
        /// <param name="weight">Вес слоя.</param>
        private void SetWeight(float weight)
        {
            foreach (var indicator in Indicators)
            {
                var currentWeight = Animator.GetLayerWeight(indicator.LayerIndex);
                var targetWeight = Mathf.Lerp(currentWeight, indicator.gameObject == _activeInputHandler.gameObject ? 1 : 0, weight);
                Animator.SetLayerWeight(indicator.LayerIndex, targetWeight);
            }
        }

        /// <summary>
        /// Отписка.
        /// </summary>
        private void OnDestroy()
        {
            Indicators.ForEach(a => a.Used -= OnUsed);
        }
    }
}
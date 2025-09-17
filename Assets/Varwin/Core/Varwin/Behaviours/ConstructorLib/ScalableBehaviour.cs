using System;
using System.Collections;
using UnityEngine;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    [Obsolete]
    public class ScalableBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            return true;
        }
    }
    
    [Obsolete]
    [VarwinComponent(English: "Scaling", Russian: "Масштабирование", Chinese: "縮放")]
    public class ScalableBehaviour : VarwinBehaviour
    {
        private bool _forceStopScaling;
        private bool _pauseScaling;
        private bool _backfront;

        private Coroutine _scaleOnXCoroutine;
        private Coroutine _scaleOnYCoroutine;
        private Coroutine _scaleOnZCoroutine;

        private bool _scaleOnXRunning;
        private bool _scaleOnYRunning;
        private bool _scaleOnZRunning;
        
        public enum Axis
        {
            [Item(English: "X", Chinese: "X")]
            X,
            [Item(English: "Y", Chinese: "Y")]
            Y,
            [Item(English: "Z", Chinese: "Z")]
            Z,
        }
        
        public enum ScaleType
        {
            [Item(English: "Once", Russian: "Один раз", Chinese: "一次")]
            Once = 0,
            [Item(English: "Repeatedly", Russian: "Повторяясь", Chinese: "反复")]
            Repeat,
            [Item(English: "Back and forth", Russian: "Туда-сюда", Chinese: "來回")]
            PingPong
        }
        
        public delegate void ScalingCompleteHandler ([Parameter(English:"finished", Russian:"завершено", Chinese: "完成的")] bool finished);

        protected override void AwakeOverride()
        {
            
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopScaling")]
        [Action(English: "Pause scaling", Russian: "Приостановить масштабирование", Chinese: "暫停縮放")]
        public void PauseScaling()
        {
            _pauseScaling = true;
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopScaling")]
        [Action(English: "Continue scaling", Russian: "Возобновить масштабирование", Chinese: "繼續縮放")]
        public void ContinueScaling()
        {
            _pauseScaling = false;
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopScaling")]
        [Action(English: "Stop scaling", Russian: "Остановить масштабирование", Chinese: "停止縮放")]
        public void StopScaling()
        {
            _forceStopScaling = true;
        }
        
        [Obsolete]
        [Event(English: "scaling complete", Russian: "масштабирование завершено", Chinese: "縮放完成")]
        public event ScalingCompleteHandler OnScalingComplete;
        
        [Obsolete]
        [Action(English: "Scale", Russian: "Масштабировать", Chinese: "縮放")]
        [ArgsFormat(English:"to {%} on axis {%} with time {%}s {%}", Russian: "в {%} раз по оси {%} в течение {%}сек {%}")]
        public void ScaleWithTimeAndSize(float scale, Axis axis, float time, ScaleType type)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            Vector3 startScale = transform.localScale;
            
            float axisStep = 0f;
            float finalScale = 0f;
            float startAxisScale = 0f;

            time = Mathf.Clamp(time, Time.deltaTime, time);

            if (axis == Axis.X)
            {
                finalScale = startScale.x * scale;
                startAxisScale = startScale.x;
                axisStep = (finalScale - startScale.x) / time;

                StartScalingCoroutine(ref _scaleOnXCoroutine, ref _scaleOnXRunning);
            }
            else if(axis == Axis.Y)
            {
                finalScale = startScale.y * scale;
                startAxisScale = startScale.y;
                axisStep = (finalScale - startScale.y) / time;

                StartScalingCoroutine(ref _scaleOnYCoroutine, ref _scaleOnYRunning);
            }
            else if(axis == Axis.Z)
            {
                finalScale = startScale.z * scale;
                startAxisScale = startScale.z;
                axisStep = (finalScale - startScale.z) / time;

                StartScalingCoroutine(ref _scaleOnZCoroutine, ref _scaleOnZRunning);
            }

            void StartScalingCoroutine(ref Coroutine coroutine, ref bool scaleRunning)
            {
                if (coroutine != null)
                {
                    StopCoroutine(coroutine);
                }

                scaleRunning = true;
            
                if (type == ScaleType.PingPong)
                {
                    coroutine = StartCoroutine(PingPongScaleCoroutine(startAxisScale, finalScale, axis, axisStep));
                }
                else
                {
                    coroutine = StartCoroutine(ScaleWithSizeCoroutine(startAxisScale, finalScale, axis, axisStep, type));
                }
            }
        }

        [Obsolete]
        [Action(English: "Scale", Russian: "Масштабировать", Chinese: "縮放")]
        [ArgsFormat(English:"with rate {%}m/sec on axis {%} with time {%}s {%}", Russian: "со скоростью {%}м/с по оси {%} в течение {%}сек {%}")]
        public void ScaleWithTimeAndSpeed(float speed, Axis axis, float time, ScaleType type)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            var scale = CalculateScale(speed, axis, time);
            ScaleWithTimeAndSize(scale, axis, time, type);
        }

        private float CalculateScale(float speed, Axis axis, float time)
        {
            Vector3 startScale = transform.localScale;
            
            float scale = speed * time;
            float scaleValue = 0;
            
            switch (axis)
            {
                case Axis.X:
                    scaleValue = startScale.x;
                    break;
                case Axis.Y:
                    scaleValue = startScale.y;
                    break;
                case Axis.Z:
                    scaleValue = startScale.z;
                    break;
            }
            
            return (scale + scaleValue) / scaleValue;
        }
        
        [Obsolete]
        [Action(English: "Set axis scale", Russian: "Масштабировать", Chinese: "設置軸刻度")]
        [ArgsFormat(English:"for axis {%} to {%}", Russian: "по оси {%} в {%} раз ")]
        public void SetAxisScale(Axis axis, float size)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            Vector3 currentScale = transform.localScale;
            Vector3 newScale = currentScale;
            
            if (axis == Axis.X)
            {
                newScale = new Vector3(currentScale.x * size, currentScale.y, currentScale.z);
            }
            else if (axis == Axis.Y)
            {
                newScale = new Vector3(currentScale.x, currentScale.y * size, currentScale.z);
            }
            else if (axis == Axis.Z)
            {
                newScale = new Vector3(currentScale.x, currentScale.y, currentScale.z * size);
            }

            transform.localScale = newScale;
        }

        private IEnumerator PingPongScaleCoroutine(float startAxisScale, float finalScale, Axis axis, float axisStep)
        {
            while (true)
            {
                Vector3 currentScale = transform.localScale;
                
                while (_pauseScaling)
                {
                    if (_forceStopScaling)
                    {
                        SetScalingStateOnAxis(axis, false);
                        OnScalingComplete?.Invoke(false);

                        yield break;
                    }
                    yield return null;
                }
                
                if (_forceStopScaling)
                {
                    OnScalingComplete?.Invoke(false);
                    yield break;
                }
                
                Vector3 newScale = currentScale;
                float changeAxis = 0f;

                float minValue = Mathf.Min(startAxisScale, finalScale);
                float maxValue = Mathf.Max(startAxisScale, finalScale);

                if (axis == Axis.X)
                {
                    changeAxis = Mathf.Clamp(currentScale.x + axisStep * Time.deltaTime, minValue, maxValue);
                    newScale = new Vector3(changeAxis, currentScale.y,
                        currentScale.z);
                }
                else if (axis == Axis.Y)
                {
                    changeAxis = Mathf.Clamp(currentScale.y + axisStep * Time.deltaTime, minValue, maxValue);
                    newScale = new Vector3(currentScale.x, changeAxis,
                        currentScale.z);
                }
                else if(axis == Axis.Z)
                {
                    changeAxis = Mathf.Clamp(currentScale.z + axisStep * Time.deltaTime, minValue, maxValue);
                    newScale = new Vector3(currentScale.x, currentScale.y,
                        changeAxis);
                }

                transform.localScale = newScale;
                
                float targetScale = axisStep > 0 ? maxValue : minValue;
                
                if (Math.Abs(changeAxis - targetScale) < Mathf.Epsilon)
                {
                    axisStep *= -1;

                    yield return null;
                } 
                
                yield return null;
            }
        }

        private IEnumerator ScaleWithSizeCoroutine(float startAxisScale, float finalScale, Axis axis, float axisStep, ScaleType type)
        {
            bool complete = false;
 
            while (!complete)
            {
                Vector3 currentScale = transform.localScale;
                
                while (_pauseScaling)
                {
                    if (_forceStopScaling)
                    {
                        SetScalingStateOnAxis(axis, false);
                        OnScalingComplete?.Invoke(false);
                        yield break;
                    }
                    yield return null;
                }
                
                if (_forceStopScaling)
                {
                    SetScalingStateOnAxis(axis, false);
                    OnScalingComplete?.Invoke(false);
                    yield break;
                }
                
                Vector3 newScale = currentScale;
                float changeAxis = 0f;

                float minValue = Mathf.Min(startAxisScale, finalScale);
                float maxValue = Mathf.Max(startAxisScale, finalScale);
                
                if (axis == Axis.X)
                {
                    changeAxis = Mathf.Clamp(currentScale.x + axisStep * Time.deltaTime, minValue, maxValue);
                    newScale = new Vector3(changeAxis, currentScale.y,
                        currentScale.z);
                }
                else if (axis == Axis.Y)
                {
                    changeAxis = Mathf.Clamp(currentScale.y + axisStep * Time.deltaTime, minValue, maxValue);
                    newScale = new Vector3(currentScale.x, changeAxis,
                        currentScale.z);
                }
                else if(axis == Axis.Z)
                {
                    changeAxis = Mathf.Clamp(currentScale.z + axisStep * Time.deltaTime, minValue, maxValue);
                    newScale = new Vector3(currentScale.x, currentScale.y,
                        changeAxis);
                }

                transform.localScale = newScale;
                
                if (Math.Abs(changeAxis - finalScale) < Mathf.Epsilon)
                {
                    if (type == ScaleType.Once)
                    {
                        complete = true;
                        SetScalingStateOnAxis(axis, false);
                        OnScalingComplete?.Invoke(!_scaleOnXRunning && !_scaleOnYRunning && !_scaleOnZRunning);
                    }
                    else
                    {
                        Vector3 scale = transform.localScale;
                            
                        if (axis == Axis.X)
                        {
                            scale.x = startAxisScale;
                        }
                        else if (axis == Axis.Y)
                        {
                            scale.y = startAxisScale;
                        }
                        else if (axis == Axis.Z)
                        {
                            scale.z = startAxisScale;
                        }

                        transform.localScale = scale;
                        
                        OnScalingComplete?.Invoke(false);
                    }

                    yield return null;
                } 
                
                yield return null;
            }
        }

        private void SetScalingStateOnAxis(Axis axis, bool value)
        {
            switch (axis)
            {
                case Axis.X:
                    _scaleOnXRunning = value;
                    break;
                case Axis.Y:
                    _scaleOnYRunning = value;
                    break;
                case Axis.Z:
                    _scaleOnZRunning = value;
                    break;
            }
        }
    }
}

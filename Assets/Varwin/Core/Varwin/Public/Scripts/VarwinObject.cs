using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Varwin.Public
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class VarwinObject : MonoBehaviour, IWrapperAware
    {
        private Wrapper _wrapper;
        public Wrapper Wrapper() => _wrapper ?? (_wrapper = gameObject.GetWrapper());
        
        public bool IsObjectEnabled
        {
            set => Wrapper().Enabled = value;
            get => Wrapper().Enabled;
        }

        public bool IsObjectActive
        {
            set => Wrapper().Activity = value;
            get => Wrapper().Activity;
        }

        public bool GrabEnabled
        {
            get => Wrapper().GrabEnabled;
            set => Wrapper().GrabEnabled = value;
        }

        public bool UseEnabled
        {
            get => Wrapper().UseEnabled;
            set => Wrapper().UseEnabled = value;
        }

        public bool TouchEnabled
        {
            get => Wrapper().TouchEnabled;
            set => Wrapper().TouchEnabled = value;
        }

        public void EnableObject()
        {
            IsObjectEnabled = true;
        }

        public void DisableObject()
        {
            IsObjectEnabled = false;
        }

        public void EnableGrab()
        {
            GrabEnabled = true;
        }

        public void DisableGrab()
        {
            GrabEnabled = false;
        }

        public void EnableUse()
        {
            UseEnabled = true;
        }

        public void DisableUse()
        {
            UseEnabled = false;
        }

        public void EnableTouch()
        {
            TouchEnabled = true;
        }

        public void DisableTouch()
        {
            TouchEnabled = false;
        }

        public void VibrateWithObject(
            GameObject go,
            GameObject controllerObject,
            float strength,
            float duration,
            float interval)
        {
            Wrapper().VibrateWithObject(go, controllerObject, strength, duration, interval);
        }

        /// <summary>
        /// Метод для определения поведения Varwin Object'а во время OnDestroy.
        /// Вызывается перед обнулением переменной _wrapper.
        /// </summary>
        protected virtual void DoOnDestroy()
        {
            
        }

        /// <summary>
        /// Метод, который вызывается после инициализации объекта.
        /// </summary>
        public virtual void OnObjectInitialized()
        {
        }

        private void OnDestroy()
        {
            DoOnDestroy();
            _wrapper = null;
        }
    }
}
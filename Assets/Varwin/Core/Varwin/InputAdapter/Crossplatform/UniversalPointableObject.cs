using UnityEngine;
using UnityEngine.Events;
using Varwin.Public;

namespace Varwin.PlatformAdapter
{
    /// <summary>
    /// Универсальный объект для обработки событий UI поинтера.
    /// </summary>
    public class UniversalPointableObject : PointableObject, ISwitchModeSubscriber
    {
        [SerializeField]
        private bool workInEditMode;
        
        public bool IsHovering { get; private set; }
        
        public UnityEvent OnHover;
        public UnityEvent OnOut;
        public UnityEvent OnClick;
        public UnityEvent OnDown;
        public UnityEvent OnUp;
        
        private Collider _collider;
        
        private void Start()
        {
            _collider = GetComponent<Collider>();
        }
        
        public override void OnPointerIn()
        {
            IsHovering = true;
            OnHover?.Invoke();
        }

        public override void OnPointerOut()
        {
            IsHovering = false;
            OnOut?.Invoke();
        }

        public override void OnPointerDown()
        {
            OnDown?.Invoke();
        }

        public override void OnPointerUp()
        {
            OnUp?.Invoke();
        }

        public override void OnPointerUpAsButton()
        {
            OnClick?.Invoke();
        }

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            if (workInEditMode || _collider == null) return;
                
            _collider.enabled = newMode != GameMode.Edit;
        }
    }
}

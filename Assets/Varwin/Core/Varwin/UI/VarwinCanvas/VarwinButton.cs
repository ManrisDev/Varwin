using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.Core.UI.VarwinCanvas
{
    public class VarwinButton : PointableObject, ISwitchModeSubscriber
    {
        internal bool workInEditMode;
        
        private Button _button;
        private PointerEventData _pointerEventData;
        private Collider _collider;
        private bool _isPressed;
        
        protected override void Awake()
        {
            _button = GetComponent<Button>();

            _pointerEventData = new PointerEventData(EventSystem.current);
        }


        private void Start()
        {
            _collider = GetComponent<Collider>();
        }

        public override void OnPointerIn()
        {
            _button.OnPointerEnter(_pointerEventData);
        }

        public override void OnPointerOut()
        {
            _button.OnPointerExit(_pointerEventData);
        }

        public override void OnPointerDown()
        {
            if (!_isPressed)
            {
                _isPressed = true;
                _button.OnPointerDown(_pointerEventData);
            }
        }

        public override void OnPointerUp()
        {
            _isPressed = false;
            _button.OnPointerClick(_pointerEventData);
            _button.OnPointerUp(_pointerEventData);
        }

        public override void OnPointerUpAsButton()
        {
            
        }

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            if (workInEditMode || _collider == null) return;
            
            _collider.enabled = newMode != GameMode.Edit;
        }
    }
}

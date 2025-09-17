using System;
using UnityEngine;
#if VARWINCLIENT
using Varwin.UI.VRErrorManager;
using Varwin.UI.VRMessageManager;
#endif
using Debug = UnityEngine.Debug;

namespace Varwin.PlatformAdapter
{
    public class UiPointer : MonoBehaviour, IBasePointer
    {        
        public Color DefaultColor = Color.cyan;
        public float Thickness = 0.002f;
        public Color ClickColor = Color.white;
        public GameObject Holder;
        public Color HoverColor = Color.green;
        public GameObject Pointer;
        public bool AddRigidBody;
        public bool IsPriority;

        public Transform _pointerOrigin;

        private bool _active;
        private bool _canClick;
        private ControllerInput.ControllerEvents _events;
        
        private Transform _previousContact;

        private bool _isActive;
        private static readonly int ColorShader = Shader.PropertyToID("_Color");
        private Color _currentColor = Color.cyan;
        private float dist;
        private PointableObject _сurrentPointable;
        private PointableObject _pointableOnPress;
        
        private readonly float _сastDistance = 50f;
        
        public bool CanPress() => _events.IsTriggerPressed() && CheckIfOverPointable();

        public bool CanRelease() => _events.IsTriggerReleased();

        public bool CanToggle() => CheckIfOverPointable() || IsPriority;

        public void Press()
        {
            if (_pointableOnPress != _сurrentPointable)
            {
                OnPointerAction(_сurrentPointable.OnPointerDown, "down");
            }
            _pointableOnPress = _сurrentPointable;
        }

        public void Release()
        {
            if (!CheckIfOverPointable())
            {
                _pointableOnPress = null;
                return;    
            }
            
            if (_pointableOnPress == _сurrentPointable)
            {
                OnPointerAction(_сurrentPointable.OnPointerUpAsButton, "up as button");
            }
            
            _pointableOnPress = null;

            OnPointerAction(_сurrentPointable.OnPointerUp, "up");
            Pointer.GetComponent<MeshRenderer>().material.color = ClickColor;
            Pointer.transform.localScale = new Vector3(Thickness * 3.5f, Thickness * 3.5f, dist);
        }

        public void Toggle(bool value)
        {
            if (Pointer)
            {
                Pointer.SetActive(value);
            }
        }

        public void Toggle()
        {
            bool dialogueWindowIsOpen = false;
            
            #if !UNITY_ANDROID && VARWINCLIENT
            dialogueWindowIsOpen = VRMessageManager.Instance && VRErrorManager.Instance &&
                                   (VRMessageManager.Instance.Panel.activeInHierarchy || VRErrorManager.Instance.Panel.activeInHierarchy);
            #endif
            
            if (!Pointer || (dialogueWindowIsOpen && Pointer.activeSelf))
            {
                return;
            }

            Pointer.SetActive(!Pointer.activeSelf);
        }

        public bool IsActive() => true;

        public virtual void Init()
        {
            Transform origins = transform.Find("PointerOrigins");

            if (origins)
            {
                if (DeviceHelper.IsOculus)
                {
                    _pointerOrigin = origins.Find("Oculus");
                }
                else
                {
                    _pointerOrigin = origins.Find("Generic");
                }

                if (_pointerOrigin == null)
                {
                    _pointerOrigin = transform;
                }
            }
            else
            {
                _pointerOrigin = transform;
            }
            
            Holder = new GameObject();
            Holder.transform.parent = _pointerOrigin;
            Holder.transform.localPosition = Vector3.zero;
            Holder.transform.localRotation = Quaternion.identity;

            Pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Pointer.transform.parent = Holder.transform;
            Pointer.transform.localScale = new Vector3(Thickness, Thickness, 100f);
            Pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            Pointer.transform.localRotation = Quaternion.identity;

            BoxCollider boxCollider = Pointer.GetComponent<BoxCollider>();

            if (AddRigidBody)
            {
                if (boxCollider)
                {
                    boxCollider.isTrigger = true;
                }

                Rigidbody rigidBody = Pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (boxCollider)
                {
                    Destroy(boxCollider);
                }
            }

            Material newMaterial = new Material(Shader.Find("Unlit/TransparentColor"));
            newMaterial.SetColor(ColorShader, _currentColor);
            newMaterial.renderQueue = 3000;
            Pointer.GetComponent<MeshRenderer>().material = newMaterial;
            Pointer.SetActive(false);

            _events = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(gameObject);
        }

        private void OnPointerAction(Action pointerAction, string pointerActionName)
        {
            if (!_сurrentPointable)
            {
                return;
            }

            try
            {
                pointerAction();
            }
            catch (Exception e)
            {
                Debug.LogError($"On pointer {pointerActionName} error in {_сurrentPointable.name}");
                Debug.LogError(e.StackTrace);
            }
        }

        private void OnPointerIn()
        {
            _currentColor = HoverColor;
            _сurrentPointable.OnPointerIn();
        }


        private void OnPointerOut()
        {
            _currentColor = DefaultColor;
            _сurrentPointable.OnPointerOut();
        }

        private bool CheckIfOverPointable() => _сurrentPointable != null;

        public void UpdateState()
        {
            dist = 100f;

            Vector3 originPosition = _pointerOrigin != null ? _pointerOrigin.position : transform.position;
            Vector3 originDirection = _pointerOrigin != null ? _pointerOrigin.forward : transform.forward;

            var ray = new Ray(originPosition, originDirection);           
            var mask = LayerMask.GetMask("UI") | LayerMask.GetMask("Default") | LayerMask.GetMask("Location");
            var isHit = Physics.Raycast(ray, out RaycastHit hit, _сastDistance, mask);

            var currentPointable = isHit ? hit.collider.GetComponent<PointableObject>() : null;

            if (_сurrentPointable && _сurrentPointable != currentPointable)
            {
                OnPointerAction(_сurrentPointable.OnPointerOut, "out");
                _previousContact = null;
            }

            _сurrentPointable = currentPointable;
            if (isHit && _сurrentPointable && _previousContact != hit.transform)
            {
                OnPointerAction(OnPointerIn, "in");
                _previousContact = hit.transform;
            }

            if (!isHit)
            {
                _previousContact = null;
            }

            if (isHit && hit.distance < 100f)
            {
                dist = hit.distance;
            }

            Pointer.transform.localScale = new Vector3(Thickness, Thickness, dist);
            Pointer.GetComponent<MeshRenderer>().material.color = _currentColor;
            Pointer.transform.localPosition = _pointerOrigin.localRotation * Vector3.forward * dist / 2f + _pointerOrigin.localPosition;
        }
    }
}
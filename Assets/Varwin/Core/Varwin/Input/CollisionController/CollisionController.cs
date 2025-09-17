using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Varwin.Core.Behaviours.ConstructorLib;
using Varwin.Public;
using Varwin.PlatformAdapter;

namespace Varwin.ObjectsInteractions
{
    public class CollisionController : MonoBehaviour
    {
        [SerializeField]
        private List<Collider> _childColliders;

        public bool IsDestroying { get; private set; } = false;
        
        private bool _isBlocked;

        [SerializeField]
        private List<CollisionControllerElement> _tempColliders;
        private List<Highlighter> _highlights = new List<Highlighter>();

        private IHighlightConfig _defaultHighlightConfig;
        private IHighlightConfig _collisionHighlightConfig;
        private IHighlightConfig _jointHighlightConfig;

        private InputController _inputController;

        public bool IsBlocked() => _isBlocked;

        private ControllerInput.ControllerInteractionEventHandler _eventHandler;
        private ControllerInput.ControllerInteractionEventArgs _controllerInteractionEventArgs;

        [SerializeField]
        private ChainedJointControllerBase _chainedJointControllerBase;

        public InputController InputController => _inputController;

        
        public void InitializeController(InputController inputController = null)
        {
            _inputController = inputController;

            if (_inputController != null)
            {
                _defaultHighlightConfig = _inputController.DefaultHighlightConfig;
            }
            else
            {
                var defHighlighter = gameObject.AddComponent<DefaultHighlighter>();
                _defaultHighlightConfig = defHighlighter.HighlightConfig();
            }

            _collisionHighlightConfig = HighlightAdapter.Instance.Configs.CollisionHighlight;
            _jointHighlightConfig = HighlightAdapter.Instance.Configs.JointHighlight;
            
            _tempColliders = new List<CollisionControllerElement>();
            _childColliders = new List<Collider>();

            
            _highlights.Clear();
            var objectsWithColliders = new List<GameObject>();
            var children = new List<Collider>();

            var objectController = _inputController?.ObjectController;
            if (objectController != null && objectController.LockChildren)
            {
                var hierarchyControllers = new List<ObjectController>();
                hierarchyControllers.Add(objectController.LockParent);
                hierarchyControllers.AddRange(objectController.LockParent.Descendants);

                foreach (var child in hierarchyControllers)
                {
                    children.AddRange(child.gameObject.GetComponentsInChildren<Collider>().ToList());
                    AddHighlight(child.gameObject);
                }
            }
            else
            {
                children.AddRange(gameObject.GetComponentsInChildren<Collider>().ToList());
                AddHighlight(gameObject);
            }

            foreach (Collider child in children)
            {
                if (child.isTrigger || !child.enabled)
                {
                    continue;
                }

                _childColliders.Add(child);

                if (!objectsWithColliders.Contains(child.gameObject))
                {
                    objectsWithColliders.Add(child.gameObject);
                }
            }

            foreach (GameObject objectWithCollider in objectsWithColliders)
            {
                var triggerObjects = CreateTriggersColliders(objectWithCollider);

                foreach (var trigger in triggerObjects)
                {
                    trigger.OnCollisionEnterDelegate += OnCollide;
                    trigger.OnTriggerExitDelegate += OnColliderExit;
                    _tempColliders.Add(trigger);
                }
            }

            ProjectData.GameModeChanged += GameModeChanged;
        }

        private void AddHighlight(GameObject go)
        {
            var highlight = go.GetComponentInChildren<Highlighter>(true);
            
            if (!highlight)
            {
                var overrider = go.GetComponent<HighlightOverrider>();
                if (overrider)
                {
                    go = overrider.ObjectToHightlight;
                }
                
                highlight = HighlightAdapter.Instance.AddHighlighter(go);
            }
            
            _highlights.Add(highlight);
        }

        private void Update()
        {
            _getCollidersDebounce.Update();

            if (_inputController == null)
            {
                return;
            }

            if (_isBlocked)
            {
                _inputController.DisableDrop();
            }
            else if (!_inputController.IsDropEnabled())
            {
                _inputController.EnableDrop();
                StartCoroutine(DropAfterOneFrame());
            }
        }

        private IEnumerator DropAfterOneFrame()
        {
            yield return new WaitForEndOfFrame();
            _inputController.ForceDropIfNeeded();
        }

        private void OnDestroy()
        {
            if (_childColliders?.Count > 0)
            {
                foreach (Collider childCollider in _childColliders)
                {
                    if (childCollider)
                    {
                        childCollider.enabled = true;
                    }
                }
            }

            if (_tempColliders != null)
            {
                foreach (CollisionControllerElement collisionControllerElement in _tempColliders)
                {
                    if (!collisionControllerElement)
                    {
                        continue;
                    }
                    
                    collisionControllerElement.OnTriggerExitDelegate -= OnColliderExit;
                    collisionControllerElement.OnCollisionEnterDelegate -= OnCollide;

                    if (collisionControllerElement.gameObject)
                    {
                        DestroyImmediate(collisionControllerElement.gameObject);
                    }
                }
            }

            _inputController?.EnableDrop();
            _inputController = null;
            UnsubscribeFromJointControllerEvents(_chainedJointControllerBase);

            ProjectData.GameModeChanged -= GameModeChanged;
        }

        private void OnEnable()
        {
            if (_childColliders?.Count > 0)
            {
                foreach (Collider childCollider in _childColliders)
                {
                    if (childCollider)
                    {
                        childCollider.enabled = false;
                    }
                }
            }
            
            if (_tempColliders == null)
            {
                return;
            }

            foreach (CollisionControllerElement element in _tempColliders)
            {
                if (!element)
                {
                    continue;
                }
                
                element.OnCollisionEnterDelegate += OnCollide;
                element.OnTriggerExitDelegate += OnColliderExit;
            }
        }

        private void OnDisable()
        {
            if (_childColliders?.Count > 0)
            {
                foreach (Collider childCollider in _childColliders)
                {
                    if (childCollider)
                    {
                        childCollider.enabled = true;
                    }
                }
            }
            
            if (_tempColliders == null)
            {
                return;
            }

            foreach (CollisionControllerElement element in _tempColliders)
            {
                if (!element)
                {
                    continue;
                }
                
                element.OnCollisionEnterDelegate -= OnCollide;
                element.OnTriggerExitDelegate -= OnColliderExit;
            }

            Unblock();
        }

        public void SubscribeToJointControllerEvents(ChainedJointControllerBase controller)
        {
            _chainedJointControllerBase = controller;
            _chainedJointControllerBase.OnCollisionEnter += CollisionEnterCheck;
            _chainedJointControllerBase.OnCollisionExit += CollisionExitCheck;
        }

        public void UnsubscribeFromJointControllerEvents(ChainedJointControllerBase controller)
        {
            _chainedJointControllerBase = null;
            
            if (!controller)
            {
                return;
            }
            
            controller.OnCollisionEnter -= CollisionEnterCheck;
            controller.OnCollisionExit -= CollisionExitCheck;
        }

        private void GameModeChanged(GameMode gm)
        {
            Unblock();

            if (_inputController.ControllerEvents == null)
            {
                return;
            }

            _inputController.EnableDrop();
            StartCoroutine(DropAfterOneFrame());
            _inputController.ControllerEvents.OnGripReleased(_controllerInteractionEventArgs);
        }

        private List<CollisionControllerElement> CreateTriggersColliders(GameObject originalColliderHolder)
        {
            var elements = new List<CollisionControllerElement>();
            
            GameObject collidersContainer = CreateColliderContainer(originalColliderHolder, "TempCollidersContainer");

            MovableBehaviour movableBehaviour = originalColliderHolder.GetComponent<MovableBehaviour>();
            bool movableBehExist = movableBehaviour;
            
            Collider[] colliders = originalColliderHolder.GetComponents<Collider>();

            foreach (Collider col in colliders)
            {
                if (col is CharacterController characterController)
                {
                    var newCollider = collidersContainer.AddComponent<CapsuleCollider>();

                    newCollider.direction = 1;
                    newCollider.center = characterController.center;
                    newCollider.radius = characterController.radius;
                    newCollider.height = characterController.height;

                    newCollider.isTrigger = true;

                    if (ProjectData.GameMode == GameMode.Edit)
                    {
                        characterController.enabled = false;
                    }
                    
                    continue;
                }
                
                if (!col.isTrigger)
                {
                    Collider newCollider;
                    
                    if (movableBehExist)
                    {
                        GameObject collisionContainer = CreateColliderContainer(originalColliderHolder, "TempCollisionContainer");

                        var collisionElement = collisionContainer.AddComponent<MovableCollisionControllerElement>();
                        collisionElement.MovableBehaviour = movableBehaviour;
                        elements.Add(collisionElement);
                        
                        newCollider = DuplicateComponent(col, collisionContainer);
                    }
                    else
                    {
                        newCollider = DuplicateComponent(col, collidersContainer);
                    }
       

                    if (newCollider.GetType() == typeof(MeshCollider))
                    {
                        var meshCollider = (MeshCollider) newCollider;
                        meshCollider.convex = true;
                    }

                    newCollider.isTrigger = true;

                    if (ProjectData.GameMode == GameMode.Edit)
                    {
                        col.enabled = false;
                    }

                }
            }
            
            elements.Add(collidersContainer.AddComponent<CollisionControllerElement>());
            return elements;
        }

        private GameObject CreateColliderContainer(GameObject originalColliderHolder, string name)
        {
            GameObject collidersContainer = new GameObject("TempCollidersContainer");
            collidersContainer.transform.parent = originalColliderHolder.transform;
            collidersContainer.transform.localPosition = Vector3.zero;
            collidersContainer.transform.localRotation = Quaternion.identity;
            collidersContainer.transform.localScale = Vector3.one;

            return collidersContainer;
        }
        
        private T DuplicateComponent<T>(T original, GameObject destination) where T : Component
        {
            Type type = original.GetType();
            var dst = destination.AddComponent(type) as T;
            var fields = type.GetFields();

            foreach (FieldInfo field in fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }

                field.SetValue(dst, field.GetValue(original));
            }

            var props = type.GetProperties();

            foreach (PropertyInfo prop in props)
            {
                if (!prop.CanWrite || !prop.CanRead || prop.Name == "name")
                {
                    continue;
                }

                prop.SetValue(dst, prop.GetValue(original, null), null);
            }

            return dst;
        }

        private void OnCollide(Collider other)
        {
            WakeUpCollidedRigidbody(other);
            
            if (_chainedJointControllerBase)
            {
                _chainedJointControllerBase.CollisionEnter(other);
                return;
            }

            CollisionEnterCheck(other);
        }

        private void WakeUpCollidedRigidbody(Collider other)
        {
            var otherRigidbody = other.GetComponentInParent<Rigidbody>();
            if (otherRigidbody)
            {
                otherRigidbody.WakeUp();
            }
        }

        private void OnColliderExit(Collider other)
        {
            if (_chainedJointControllerBase)
            {
                _chainedJointControllerBase.CollisionExit(other);

                return;
            }

            CollisionExitCheck(other);
        }

        private void CollisionExitCheck(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return;
            }
            
            Unblock();
        }

        private void CollisionEnterCheck(Collider other)
        {
            GetObjectColliders();

            if(_childColliders.Contains(other))
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                return;
            }

            Rigidbody otherBody = other.attachedRigidbody;

            if (otherBody)
            {
                if (other.isTrigger)
                {
                    return;
                }

                if (otherBody.isKinematic)
                {
                    Block();
                }
            }
            else if (!other.isTrigger)
            {
                Block();
            }
        }

        /// <summary>
        /// Подсвечиваем и убираем коллайдеры
        /// </summary>
        private void Block()
        {
            if (_isBlocked || ProjectData.GameMode == GameMode.Edit)
            {
                return;
            }

            TurnCollidersOn(false);

            foreach (var highlight in _highlights)
            {
                highlight.IsEnabled = true;
                if (_chainedJointControllerBase)
                {
                    highlight.SetConfig(_chainedJointControllerBase.Connecting ? _jointHighlightConfig : _collisionHighlightConfig, null, false);
                }
                else
                {
                    highlight.SetConfig(_collisionHighlightConfig, null, false);
                }
            }

            if (_inputController?.ControllerEvents != null
                && (ProjectData.GameMode == GameMode.Edit)
                && !_isBlocked)
            {
                _inputController.ControllerEvents.GripPressed += OnGripPressed;
            }

            _isBlocked = true;
        }

        public void OnGripPressed(object sender, ControllerInput.ControllerInteractionEventArgs e)
        {
            _controllerInteractionEventArgs = e;

            if (_inputController != null
                && (ProjectData.GameMode == GameMode.Edit)
                && _isBlocked)
            {
                _inputController.ControllerEvents.GripPressed -= OnGripPressed;

                Unblock();

                _inputController.EnableDrop();

                StartCoroutine(DropAfterOneFrame());
                _inputController.ReturnPosition();
                _inputController.ControllerEvents.OnGripReleased(e);
            }
        }

        public void ForcedUnblock()
        {
            Unblock();
        }

        /// <summary>
        /// Рассвечиваем и возвращаем коллайдеры
        /// </summary>
        private void Unblock()
        {
            if (ProjectData.GameMode != GameMode.Edit)
            {
                TurnCollidersOn(true);
            }

            foreach (var highlight in _highlights)
            {
                if (!highlight)
                {
                    continue;
                }
                
                if (_chainedJointControllerBase && _chainedJointControllerBase.Connecting)
                {
                    highlight.IsEnabled = true;
                    highlight.SetConfig(_jointHighlightConfig, null, false);
                }
                else
                {
                    highlight.IsEnabled = false;
                }
            }

            _isBlocked = false;

            if (_inputController?.ControllerEvents != null && ProjectData.GameMode == GameMode.Edit)
            {
                _inputController.ControllerEvents.GripPressed -= OnGripPressed;
            }
        }

        private void TurnCollidersOn(bool mode)
        {
            foreach (Collider childCollider in _childColliders)
            {
                if (childCollider)
                {
                    childCollider.enabled = mode;
                }
            }
        }

        private Debounce _getCollidersDebounce = new Debounce(0.5f);

        private void GetObjectColliders()
        {
            //In chained object it will be too much of a pain to collect them on every event so just don't
            if (!_getCollidersDebounce.CanReset())
            {
                return;
            }

            _getCollidersDebounce.Reset();
        }

        public void ForceDestroy()
        {
            DestroyImmediate(this);
        }
        
        public void Destroy()
        {
            IsDestroying = true;
            Destroy(this);
        }
    }
}

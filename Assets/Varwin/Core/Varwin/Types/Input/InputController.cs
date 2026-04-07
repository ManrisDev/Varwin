using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Varwin.Models.Data;
using Varwin.ObjectsInteractions;
using Varwin.Public;
using Varwin.PlatformAdapter;

#pragma warning disable 618

namespace Varwin
{
    public class InputController
    {
        public ObjectController ObjectController => _objectController;
        public ControllerInput.ControllerEvents ControllerEvents;

        public bool IsRoot => _isRoot;

        // ReSharper disable once NotAccessedField.Local
        private IHapticsAware _haptics;
        private readonly List<InputAction> _inputActions = new List<InputAction>();
        private readonly bool _isRoot;
        private ObjectController _objectController;
        private CollisionController _collisionController;
        private readonly GameObject _gameObject;
        public ObjectInteraction.InteractObject InteractObject { get; private set; }
        private PlayerAppearance.InteractControllerAppearance _controllerAppearance;

        private IHighlightAware _highlight;
        private GameObject _highlightOverriderGameObject;
        private Highlighter _highlighter;
        public IHighlightConfig DefaultHighlightConfig;
        private readonly IHighlightConfig _useHighlightConfig = HighlightAdapter.Instance.Configs.UseHighlight;
        private bool _highlightEnabled;

        public bool IsForceGrabbed => InteractObject?.IsForceGrabbed() ?? false;

        private GrabSettings _grabSettings;
        public event Action Initialized;

        public InputController(ObjectController objectController, GameObject gameObject, bool isRoot = false)
        {
            _objectController = objectController;
            _gameObject = gameObject;

            bool isRuntimeInstanced = false;
            InteractObject = InputAdapter.Instance.ObjectInteraction.Object.GetFrom(_gameObject);
            if (InteractObject == null)
            {
                InteractObject = InputAdapter.Instance.ObjectInteraction.Object.AddTo(_gameObject);
            }
            else
            {
                isRuntimeInstanced = true;
            }

            _isRoot = isRoot;
            Init();
            InteractObject.InteractableObjectGrabbed += OnAnyGrabStart;
            InteractObject.InteractableObjectUngrabbed += OnAnyGrabEnd;

            _controllerAppearance = InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(_gameObject);

            if (isRuntimeInstanced)
            {
                GameModeChanged(ProjectData.GameMode);
            }

            Initialized?.Invoke();
        }

        public void ReturnPosition()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction action)
                {
                    action.ReturnPosition();
                }
            }
        }

        public void Destroy()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.Destroy();
            }
            _inputActions.Clear();

            if (InteractObject != null)
            {
                if (InteractObject.IsGrabbed() || InteractObject.IsUsing())
                {
                    InteractObject.ForceStopInteracting();
                }

                InteractObject.InteractableObjectGrabbed -= OnAnyGrabStart;
                InteractObject.InteractableObjectUngrabbed -= OnAnyGrabEnd;
                InteractObject.InteractableObjectUnused -= HighlightObjectOnUseEnd;
                InteractObject = null;
            }

            _objectController = null;
            Object.Destroy(_gameObject);
        }

        private static void RemoveCollisionController(CollisionController controller)
        {
            controller.enabled = false;
            controller.Destroy();
        }

        private bool IsObjectInteractable()
        {
            var behaviour = _gameObject.GetComponent<InteractableObjectBehaviour>();
            
            if (behaviour)
            {
                return behaviour.IsInteractable;
            }

            return _gameObject.GetComponent<IVarwinInputAware>() != null;
        }

        private void Init()
        {
            InitInputActions();

            var highlightAware = _gameObject.GetComponent<IHighlightAware>();
            if (Settings.Instance.HighlightEnabled && highlightAware == null)
            {
                highlightAware = _gameObject.AddComponent<DefaultHighlighter>();
            }

            var highlightOverrider = _gameObject.GetComponent<HighlightOverrider>();
            if (highlightOverrider)
            {
                _highlightOverriderGameObject = highlightOverrider.ObjectToHightlight;
            }

            if (highlightAware != null)
            {
                _highlight = highlightAware;
                AddHighLighter(highlightAware);
            }

            if (Settings.Instance.HighlightEnabled && IsObjectInteractable())
            {
                EnableHighlight();
            }

            var hapticsAware = _gameObject.GetComponent<IHapticsAware>();

            bool settingsInteractable = Settings.Instance.TouchHapticsEnabled || Settings.Instance.GrabHapticsEnabled || Settings.Instance.UseHapticsEnabled;
            if (hapticsAware == null && settingsInteractable)
            {
                hapticsAware = _gameObject.AddComponent<DefaultHaptics>();
            }

            if (hapticsAware != null)
            {
                AddHaptics(hapticsAware);
                _haptics = hapticsAware;
            }
        }

        private void InitInputActions()
        {
            if (_inputActions.Count > 0)
            {
                foreach (InputAction inputAction in _inputActions)
                {
                    inputAction.Destroy();
                }
                _inputActions.Clear();
            }

            try
            {
                _inputActions.Add(new UseAction(_objectController,
                    _gameObject,
                    InteractObject,
                    this));

                _inputActions.Add(new GrabAction(_objectController,
                    _gameObject,
                    InteractObject,
                    this));

                _inputActions.Add(new TouchAction(_objectController,
                    _gameObject,
                    InteractObject,
                    this));

                _inputActions.Add(new PointerAction(_objectController,
                    _gameObject,
                    InteractObject,
                    this));
                
                _inputActions.Add(new ARTrackingAction(_objectController,
                    _gameObject,
                    InteractObject,
                    this));


                _grabSettings = _gameObject.GetComponent<GrabSettings>();
            }
            catch (Exception e)
            {
                Debug.LogError($"Can't create input actions for {_gameObject.name} Error: {e.Message}");
            }
        }

        private void OnAnyGrabStart(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (!_isRoot)
            {
                return;
            }
            
            var isDeleting = _gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true;
            if (isDeleting)
            {
                return;
            }

            _collisionController = _gameObject.GetComponents<CollisionController>().FirstOrDefault(a => !a.IsDestroying);

            if (!_collisionController || _collisionController.IsDestroying)
            {
                _collisionController = _gameObject.AddComponent<CollisionController>();
                _collisionController.InitializeController(this);
            }

            var grabbingController = InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(e.Hand);
            var grabbingObject = grabbingController.Controller.GetGrabbedObject();

            if (grabbingObject)
            {
                grabbingController.Controller.SetGrabColliders(grabbingObject.GetComponentsInChildren<Collider>());
            }

            GameObject vioGrabbingObject = InteractObject.GetGrabbingObject();

            if (vioGrabbingObject)
            {
                ControllerEvents = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(InteractObject.GetGrabbingObject());
            }
        }

        private void OnAnyGrabEnd(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            if (!_isRoot)
            {
                return;
            }

            if (_collisionController) //if there is a jointBehaviour on this go it'll manage the collisionController on its own 
            {
                RemoveCollisionController(_collisionController);
            } 

            InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(e.Hand).Controller.SetGrabColliders(null);

            if (_highlightEnabled && InteractObject.IsTouching())
            {
                SetupHighlightWithConfig(true, DefaultHighlightConfig);
            }
        }

        private void EnableHighlight()
        {
            if (_highlight == null)
            {
                return;
            }

            InteractObject.InteractableObjectTouched += HighlightObject;
            InteractObject.InteractableObjectUntouched += UnhighlightObject;
            InteractObject.InteractableObjectGrabbed += UnhighlightObject;
            //Wrong behavior of the highlight
            //Now we use default highlight from steam
            //For Collision controller use varwin highlight effect
            InteractObject.InteractableObjectUsed += HighlightObjectOnUseStart;
            InteractObject.InteractableObjectUnused += HighlightObjectOnUseEnd;

            _highlightEnabled = true;
        }

        // ReSharper disable once UnusedMember.Local
        private void DisableHighlight()
        {
            if (_highlight == null)
            {
                return;
            }

            UnhighlightObject(null, new ObjectInteraction.InteractableObjectEventArgs());
            InteractObject.InteractableObjectTouched -= HighlightObject;
            InteractObject.InteractableObjectUntouched -= UnhighlightObject;
            InteractObject.InteractableObjectGrabbed -= UnhighlightObject; 
            //Wrong behavior of the highlight
            //Now we use default highlight from steam
            //For Collision controller use varwin highlight effect
            InteractObject.InteractableObjectUsed -= HighlightObjectOnUseStart;
            InteractObject.InteractableObjectUnused -= HighlightObjectOnUseEnd;

            _highlightEnabled = false;
        }

        public void EnableDrop()
        {
            if (InteractObject != null)
            {
                InteractObject.ValidDrop = ObjectInteraction.InteractObject.ValidDropTypes.DropAnywhere;
            }
        }

        public void DisableDrop()
        {
            InteractObject.ValidDrop = ObjectInteraction.InteractObject.ValidDropTypes.NoDrop;
        }

        public bool IsDropEnabled() => InteractObject.ValidDrop == ObjectInteraction.InteractObject.ValidDropTypes.DropAnywhere;

        public void ForceDropIfNeeded()
        {
            if (ProjectData.PlatformMode != PlatformMode.Vr && ProjectData.PlatformMode != PlatformMode.NettleDesk)
            {
                return;    
            }

            if (ControllerEvents == null || InteractObject == null|| InteractObject.IsForceGrabbed())
            {
                return;
            }

            if (_grabSettings && _grabSettings.GrabType == GrabSettings.GrabTypes.Toggle)
            {
                if (!ControllerEvents.GetBoolInputActionState("GrabToggle"))
                {
                    InteractObject.ForceStopInteracting();
                }
            }
            else if (!ControllerEvents.IsButtonPressed(ControllerInput.ButtonAlias.GripPress))
            {
                InteractObject.ForceStopInteracting();
            }
        }

        public void DropGrabbedObject()
        {
            InteractObject.ForceStopInteracting();
        }

        public bool IsGrabbed() => InteractObject.IsGrabbed();

        public void DropGrabbedObjectAndDeactivate()
        {
            InteractObject.DropGrabbedObjectAndDeactivate();
        }

        private void AddHighLighter(IHighlightAware highlight)
        {
            DefaultHighlightConfig = highlight.HighlightConfig();

            _highlighter = _highlightOverriderGameObject
                ? _highlightOverriderGameObject.GetComponent<Highlighter>()
                : _gameObject.GetComponent<Highlighter>();

            if (!_highlighter)
            {
                _highlighter = _highlightOverriderGameObject
                    ? HighlightAdapter.Instance.AddHighlighter(_highlightOverriderGameObject)
                    : HighlightAdapter.Instance.AddHighlighter(_gameObject);
            }

            try
            {
                _highlighter.SetConfig(DefaultHighlightConfig);
            }
            catch (Exception)
            {
                Debug.LogError($"Can not add highlight to game object = {_gameObject}");
            }
        }

        private void AddHaptics(IHapticsAware haptics)
        {
            HapticsConfig onUse = haptics.HapticsOnUse();
            HapticsConfig onTouch = haptics.HapticsOnTouch();
            HapticsConfig onGrab = haptics.HapticsOnGrab();

            ObjectInteraction.InteractHaptics interactHaptic =
                InputAdapter.Instance.ObjectInteraction.Haptics.GetFrom(_gameObject)
                ?? InputAdapter.Instance.ObjectInteraction.Haptics.AddTo(_gameObject);

            if (onUse != null)
            {
                interactHaptic.StrengthOnUse = onUse.Strength;
                interactHaptic.IntervalOnUse = onUse.Interval;
                interactHaptic.DurationOnUse = onUse.Duration;
            }
            else
            {
                interactHaptic.StrengthOnUse = 0;
                interactHaptic.DurationOnUse = 0;
            }

            if (onTouch != null)
            {
                interactHaptic.StrengthOnTouch = onTouch.Strength;
                interactHaptic.IntervalOnTouch = onTouch.Interval;
                interactHaptic.DurationOnTouch = onTouch.Duration;
            }
            else
            {
                interactHaptic.StrengthOnTouch = 0;
                interactHaptic.DurationOnTouch = 0;
            }

            if (onGrab != null)
            {
                interactHaptic.StrengthOnGrab = onGrab.Strength;
                interactHaptic.IntervalOnGrab = onGrab.Interval;
                interactHaptic.DurationOnGrab = onGrab.Duration;
            }
            else
            {
                interactHaptic.StrengthOnGrab = 0;
                interactHaptic.DurationOnGrab = 0;
            }
        }

        public void Vibrate(GameObject controllerObject, float strength, float duration, float interval)
        {
            if (!InteractObject.IsGrabbed() && !InteractObject.IsUsing())
            {
                Debug.LogWarning("Can't vibrate with object not grabbed or in use");
                return;
            }

            PlayerController.PlayerNodes.ControllerNode playerController =
                InputAdapter.Instance.PlayerController.Nodes.GetControllerReference(controllerObject);

            if (playerController == null)
            {
                Debug.LogWarning("Can't vibrate: " + controllerObject + " is not a controller");
                return;
            }

            playerController.Controller.TriggerHapticPulse(strength, duration, interval);
        }

        private void UnhighlightObject(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            SetupHighlightWithConfig(false, DefaultHighlightConfig);
        }

        private void HighlightObject(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            SetupHighlightWithConfig(true, DefaultHighlightConfig);
        }

        private void HighlightObjectOnUseStart(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            SetupHighlightWithConfig(true, _useHighlightConfig);
        }
        
        private void HighlightObjectOnUseEnd(object sender, ObjectInteraction.InteractableObjectEventArgs e)
        {
            var highlightEnabled = InteractObject.IsTouching() && !InteractObject.IsGrabbed();
            SetupHighlightWithConfig(highlightEnabled, DefaultHighlightConfig);
        }

        public void SetupHighlightWithConfig(bool isEnabled, IHighlightConfig config)
        {
            if (!_gameObject || !_highlighter)
            {
                return;
            }

            _highlighter.IsEnabled = isEnabled;
            
            if (isEnabled)
            {
                _highlighter.SetConfig(config);
            }
        }

        public TransformDto GetTransform()
        {
            if (!_gameObject)
            {
                return null;
            }
            
            var objectId = _gameObject.GetComponent<ObjectId>();
            
            if (!objectId)
            {
                return null;
            }

            Transform transform = _objectController.gameObject == _gameObject
                ? _objectController.GetAffectedTransform()
                : _gameObject.transform;

            return new TransformDto {Id = objectId.Id, Transform = transform.ToTransformDT()};
        }

        public bool IsConnectedToGameObject(GameObject go) => go == _gameObject;

        public void DisableViewInput()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.DisableViewInput();
            }
        }

        public void EnableViewInput()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.EnableViewInput();
            }
        }

        public void GameModeChanged(GameMode newGameMode)
        {
            if (InteractObject.IsGrabbed() || InteractObject.IsUsing())
            {
                InteractObject.ForceStopInteracting();
            }

            foreach (InputAction inputAction in _inputActions)
            {
                inputAction.GameModeChanged(newGameMode);
            }
            
            if (!_highlightEnabled && IsObjectInteractable())
            {
                EnableHighlight();
            }
            else if (_highlightEnabled && !IsObjectInteractable())
            {
                DisableHighlight();
            }
        }

        public void PlatformModeChanged(PlatformMode newPlatformMode)
        {
            if (InteractObject.IsGrabbed() || InteractObject.IsUsing())
            {
                InteractObject.ForceStopInteracting();
            }

            if (_highlightEnabled)
            {
                SetupHighlightWithConfig(false, null);
            }

            var oldState = _inputActions.ToDictionary(inputAction => inputAction.GetType(), inputAction => inputAction.IsEnabled);

            InteractObject.DestroyComponent();
            _controllerAppearance?.DestroyComponent();
            InteractObject = InputAdapter.Instance.ObjectInteraction.Object.AddTo(_gameObject);
            Init();
            InteractObject.InteractableObjectGrabbed += OnAnyGrabStart;
            InteractObject.InteractableObjectUngrabbed += OnAnyGrabEnd;
            
            _controllerAppearance = InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(_gameObject);
            
            foreach (var inputAction in _inputActions)
            {
                inputAction.PlatformModeChanged(newPlatformMode);

                if (!oldState.TryGetValue(inputAction.GetType(), out var enabled))
                {
                    continue;
                }
                
                if (enabled)
                {
                    inputAction.EnableViewInput();
                }
                else
                {
                    inputAction.DisableViewInput();
                }
            }
            
            Initialized?.Invoke();
        }

        public void EnableViewUsing()
        {
            if (!_highlightEnabled)
            {
                EnableHighlight();
            }

            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is UseAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewUsing()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is UseAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }

        public void EnableViewGrab()
        {
            if (!_highlightEnabled)
            {
                EnableHighlight();
            }
            
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewGrab()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }

        public void EnableViewTouch()
        {
            if (!_highlightEnabled)
            {
                EnableHighlight();
            }
            
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is TouchAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewTouch()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is TouchAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }
        
        public void EnableViewPointer()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is PointerAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableViewPointer()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is PointerAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }
        
        public void EnableARTracking()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is ARTrackingAction)
                {
                    inputAction.EnableViewInput();
                }
            }
        }

        public void DisableARTracking()
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is ARTrackingAction)
                {
                    inputAction.DisableViewInput();
                }
            }
        }

        public void UseStart(ControllerInteraction.ControllerHand hand)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is UseAction action)
                {
                    action.UseStart(hand);
                }
            }
        }

        public void UseEnd(ControllerInteraction.ControllerHand hand)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is UseAction action)
                {
                    action.UseEnd(hand);
                }
            }
        }

        public void GrabStart(ControllerInteraction.ControllerHand hand)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction action)
                {
                    action.GrabStart(hand);
                }
            }
        }
        
        public void GrabEnd(ControllerInteraction.ControllerHand hand)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is GrabAction action)
                {
                    action.GrabEnd(hand);
                }
            }
        }
        
        public void TouchStart(ControllerInteraction.ControllerHand controllerHand)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is TouchAction action)
                {
                    action.TouchStart(controllerHand);
                }
            }
        }
        
        public void TouchEnd(ControllerInteraction.ControllerHand controllerHand)
        {
            foreach (InputAction inputAction in _inputActions)
            {
                if (inputAction is TouchAction action)
                {
                    action.TouchEnd(controllerHand);
                }
            }
        }
        
        public void SetKinematicsOn()
        {
            _objectController.SaveKinematics();
            _objectController.SetKinematicsOn();
        }
        
        public void SetKinematicsDefaults()
        {
            _objectController.SetKinematicsDefaults();
        }

        public void ForceUpdateCollisionController()
        {
            _collisionController = _gameObject.GetComponent<CollisionController>();
        }
    }
}

public class TransformDto
{
    public int Id;
    public TransformDT Transform;
}

using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin
{
    public class ARTrackingAction : InputAction
    {
        private readonly bool _isInitialized = false;
        private readonly IARTargetBase _targetBase;
        private readonly ARTrackingController.TrackedObject _trackedObject;
        private readonly IARTargetFoundAware[] _foundAwares;
        private readonly IARTargetLostAware[] _lostAwares;
        private readonly IARTargetPreRenderAware[] _preRenderAwares;
        private readonly IARTargetPostRenderAware[] _postRenderAwares;

        private bool _isEnabled = true;

        public override bool IsEnabled => _trackedObject?.IsTrackable ?? _isEnabled;

        public ARTrackingAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject interactObject, InputController inputController) : base(objectController, gameObject, interactObject, inputController)
        {
            _isInitialized = InputAdapter.Instance.ARTracking?.Image != null;

            if (!_isInitialized)
            {
                return;
            }

            _targetBase = gameObject.GetComponent<IARTargetBase>();
            if (_targetBase == null)
            {
                _isInitialized = false;
                return;
            }

            if (_targetBase is not IARImageTargetAware)
            {
                return;
            }
            
            _trackedObject = InputAdapter.Instance.ARTracking.Image.GetFrom(gameObject);
            if (_trackedObject == null)
            {
                _trackedObject = InputAdapter.Instance.ARTracking.Image.AddTo(gameObject);
            }

            _trackedObject.TargetFound += OnTargetFound;
            _trackedObject.TargetLost += OnTargetLost;
            _trackedObject.TargetPreRender += OnTargetPreRender;
            _trackedObject.TargetPostRender += OnTargetPostRender;

            _foundAwares = gameObject.GetComponents<IARTargetFoundAware>();
            _lostAwares = gameObject.GetComponents<IARTargetLostAware>();
            _preRenderAwares = gameObject.GetComponents<IARTargetPreRenderAware>();
            _postRenderAwares = gameObject.GetComponents<IARTargetPostRenderAware>();
        }
        
        private void OnTargetPreRender(object sender)
        {
            foreach (var preRender in _preRenderAwares)
            { 
                preRender?.OnTargetPreRender();
            }
        }
        
        private void OnTargetPostRender(object sender)
        {
            foreach (var postRender in _postRenderAwares)
            {
                postRender?.OnTargetPostRender();
            }
        }

        private void OnTargetFound(object sender)
        {
            foreach (var foundAware in _foundAwares)
            {
                foundAware?.OnTargetFound();
            }
        }

        private void OnTargetLost(object sender)
        {
            foreach (var lostAware in _lostAwares)
            {
                lostAware?.OnTargetLost();
            }
        }

        public override void EnableViewInput()
        {
            _isEnabled = true;
            if (!_isInitialized)
            {
                return;
            }
            
            _trackedObject.IsTrackable = true;
        }

        public override void DisableViewInput()
        {
            _isEnabled = false; 
            if (!_isInitialized)
            {
                return;
            }

            _trackedObject.IsTrackable = false;
        }

        protected override void OnDestroy()
        {
            if (!_isInitialized)
            {
                return;
            }
            
            _trackedObject.DestroyComponent();
        }
    }
}
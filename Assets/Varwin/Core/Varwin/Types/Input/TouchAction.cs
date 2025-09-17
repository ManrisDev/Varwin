using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.PlatformAdapter;

namespace Varwin
{
    public class TouchAction : InputAction
    {
        private readonly List<ITouchStartAware> _touchStartList;
        private readonly List<ITouchEndAware> _touchEndList;

        private readonly List<ITouchStartInteractionAware> _touchStartInteractionsList;
        private readonly List<ITouchEndInteractionAware> _touchEndInteractionsList;

        private bool HasTouchStartAwares => _touchStartList.Count > 0 || _touchStartInteractionsList.Count > 0;
        private bool HasTouchEndAwares => _touchEndList.Count > 0 || _touchEndInteractionsList.Count > 0;
        
        public override bool IsEnabled => InteractObject?.isTouchable ?? false;

        public TouchAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject interactObject, InputController inputController) : base(
            objectController, gameObject, interactObject, inputController)
        {
            _touchStartList = GameObject.GetComponents<ITouchStartAware>().ToList();
            _touchEndList = GameObject.GetComponents<ITouchEndAware>().ToList();

            _touchStartInteractionsList = GameObject.GetComponents<ITouchStartInteractionAware>().ToList();
            _touchEndInteractionsList = GameObject.GetComponents<ITouchEndInteractionAware>().ToList();

            if (HasTouchStartAwares)
            {
                AddTouchStartAction();
            }

            if (HasTouchEndAwares)
            {
                AddTouchEndAction();
            }
        }

        #region OVERRIDES

        public override void DisableViewInput()
        {
            InteractObject.isTouchable = false;
        }

        public override void EnableViewInput()
        {
            var interactableObjectBehaviour = GameObject.GetComponent<InteractableObjectBehaviour>();

            bool canTouch;

            if (interactableObjectBehaviour)
            {
                canTouch = interactableObjectBehaviour.IsTouchable;
            }
            else
            {
                canTouch = HasTouchStartAwares || HasTouchEndAwares;
            }

            InteractObject.isTouchable = canTouch;
        }

        protected override void DisableEditorInput()
        {
        }

        protected override void EnableEditorInput()
        {
        }

        #endregion

        private void AddTouchStartAction()
        {
            InteractObject.isTouchable = true;
            InteractObject.InteractableObjectTouched += OnTouchStartVoid;
        }

        private void AddTouchEndAction()
        {
            InteractObject.isTouchable = true;
            InteractObject.InteractableObjectUntouched += OnTouchEndVoid;
        }

        private void OnTouchStartVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            TouchStart(interactableObjectEventArgs.Hand);
        }

        private void OnTouchEndVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            TouchEnd(interactableObjectEventArgs.Hand);
        }

        public void TouchEnd(ControllerInteraction.ControllerHand hand)
        {
            var handGameObject = GetHandGameObject(hand);
            var touchContext = new TouchInteractionContext(handGameObject, hand);

            _touchEndInteractionsList.ForEach(aware => aware?.OnTouchEnd(touchContext));
            _touchEndList.ForEach(legacyAware => legacyAware?.OnTouchEnd());
        }

        public void TouchStart(ControllerInteraction.ControllerHand hand)
        {
            var handGameObject = GetHandGameObject(hand);
            var touchContext = new TouchInteractionContext(handGameObject, hand);

            _touchStartInteractionsList.ForEach(aware => aware?.OnTouchStart(touchContext));
            _touchStartList.ForEach(aware => aware?.OnTouchStart());
        }
    }
}
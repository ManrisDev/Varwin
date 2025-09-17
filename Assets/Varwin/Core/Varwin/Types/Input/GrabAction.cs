using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Data.ServerData;
using Varwin.Models.Data;
using Varwin.Public;
using Varwin.PlatformAdapter;

namespace Varwin
{
    public class GrabAction : InputAction
    {
        private readonly List<IGrabStartAware> _grabStartList;
        private readonly List<IGrabEndAware> _grabEndList;

        private readonly List<IGrabStartInteractionAware> _grabStartInteractionsList;
        private readonly List<IGrabEndInteractionAware> _grabEndInteractionsList;
        private readonly IGrabPointAware _grabPoint;

        private bool HasGrabStartAwares => _grabStartList.Count > 0 || _grabStartInteractionsList.Count > 0;
        private bool HasGrabEndAwares => _grabEndList.Count > 0 || _grabEndInteractionsList.Count > 0;
        
        private Action _onEditorGrabStart = delegate { };
        private Action _onEditorGrabEnd = delegate { };
        private TransformDT _saveTransform;
        private TransformDT _onInitTransform;
        private GameObject _gameObject;
        private PlayerAppearance.InteractControllerAppearance _grab;
        
        public override bool IsEnabled => InteractObject?.isGrabbable ?? false;

        public GrabAction(
            ObjectController objectController,
            GameObject gameObject, ObjectInteraction.InteractObject interactObject, InputController inputController) :
            base(objectController, gameObject, interactObject, inputController)
        {
            _grabStartList = GameObject.GetComponents<IGrabStartAware>().ToList();
            _grabStartInteractionsList = GameObject.GetComponents<IGrabStartInteractionAware>().ToList();

            if (HasGrabStartAwares)
            {
                AddGrabStartBehaviour();
            }

            _grabEndList = GameObject.GetComponents<IGrabEndAware>().ToList();
            _grabEndInteractionsList = GameObject.GetComponents<IGrabEndInteractionAware>().ToList();

            if (HasGrabEndAwares)
            {
                AddGrabEndBehaviour();
            }

            _grabPoint = GameObject.GetComponent<IGrabPointAware>();
            _gameObject = gameObject;

            ProjectData.PlatformModeChanging += _ => { _grab?.DestroyComponent(); };

            if (_grabPoint == null)
            {
                return;
            }

            if (!_gameObject.GetComponent<GrabSettings>())
            {
                _gameObject.AddComponent<GrabSettings>()
                    .Init(_grabPoint.GetLeftGrabPoint(), _grabPoint.GetRightGrabPoint());
            }
        }

        #region OVERRIDES

        public override void DisableViewInput()
        {
            InteractObject.isGrabbable = false;
        }

        public override void EnableViewInput()
        {
            var interactableObjectBehaviour = GameObject.GetComponent<InteractableObjectBehaviour>();

            bool canGrab;

            if (interactableObjectBehaviour)
            {
                canGrab = interactableObjectBehaviour.IsGrabbable;
            }
            else
            {
                canGrab = HasGrabStartAwares || HasGrabEndAwares;
            }

            InteractObject.isGrabbable = canGrab;
        }

        protected override void EnableEditorInput()
        {
            if (!IsRootGameObject)
            {
                return;
            }

            InteractObject.isGrabbable = true;

            _grab = InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(GameObject)
                   ?? InputAdapter.Instance.PlayerAppearance.ControllerAppearance.AddTo(GameObject);

            _grab.HideControllerOnGrab = true;

            InteractObject.SwapControllersFlag = true;

            InteractObject.grabOverrideButton = ControllerInput.ButtonAlias.GripPress;
            InteractObject.InteractableObjectGrabbed += EditorGrabbed;
            InteractObject.InteractableObjectUngrabbed += EditorUngrabbed;
        }

        protected override void DisableEditorInput()
        {
            if (HasGrabStartAwares || HasGrabEndAwares)
            {
                _grab = InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(GameObject);
                _grab?.DestroyComponent();
            }

            InteractObject.isGrabbable = false;
            InteractObject.InteractableObjectGrabbed -= EditorGrabbed;
            InteractObject.InteractableObjectUngrabbed -= EditorUngrabbed;
        }

        #endregion

        private void AddGrabStartBehaviour()
        {
            InitGrabAction();
            InteractObject.grabOverrideButton = ControllerInput.ButtonAlias.GripPress;
            InteractObject.InteractableObjectGrabbed += OnGrabStartVoid;
        }

        private void AddGrabEndBehaviour()
        {
            InitGrabAction();
            InteractObject.grabOverrideButton = ControllerInput.ButtonAlias.GripPress;
            InteractObject.InteractableObjectUngrabbed += OnGrabEndVoid;
        }

        private void InitGrabAction()
        {
            InteractObject.isGrabbable = true;

            PlayerAppearance.InteractControllerAppearance grab =
                InputAdapter.Instance.PlayerAppearance.ControllerAppearance.GetFrom(GameObject)
                ?? InputAdapter.Instance.PlayerAppearance.ControllerAppearance.AddTo(GameObject);

            grab.HideControllerOnGrab = true;

            InteractObject.SwapControllersFlag = true;
        }

        private void OnGrabStartVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            GrabStart(interactableObjectEventArgs.Hand);
        }

        private void OnGrabEndVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            GrabEnd(interactableObjectEventArgs.Hand);
        }

        private void EditorGrabbed(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            _onEditorGrabStart();
        }

        private void EditorUngrabbed(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            _onEditorGrabEnd();
        }

        public void ReturnPosition()
        {
            _onInitTransform?.ToTransformUnity(GameObject.transform);
        }

        public void GrabStart(ControllerInteraction.ControllerHand hand)
        {
            var handGameObject = GetHandGameObject(hand);

            ObjectController?.OnGrabStart();

            var interactionContext = new GrabInteractionContext(handGameObject, hand);
            _grabStartInteractionsList.ForEach(aware => aware?.OnGrabStart(interactionContext));

            var hasLegacyInterfaces = _grabStartList.Count > 0; 
            if (hasLegacyInterfaces)
            {
                var legacyContext = interactionContext.GetLegacyGrabbingContext();
                _grabStartList.ForEach(legacyAware => legacyAware.OnGrabStart(legacyContext));
            }
        }

        public void GrabEnd(ControllerInteraction.ControllerHand hand)
        {
            var handGameObject = GetHandGameObject(hand);
            ObjectController?.OnGrabEnd();

            var interactionContext = new GrabInteractionContext(handGameObject, hand);
            _grabEndInteractionsList.ForEach(aware => aware?.OnGrabEnd(interactionContext));
            _grabEndList.ForEach(legacyAware => legacyAware?.OnGrabEnd());
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.PlatformAdapter;

namespace Varwin
{
    public class UseAction : InputAction
    {
        [Obsolete] private readonly List<IUseStartAware> _useStartList;
        [Obsolete] private readonly List<IUseEndAware> _useEndList;

        private readonly List<IUseStartInteractionAware> _useStartInteractionsList;
        private readonly List<IUseEndInteractionAware> _useEndInteractionsList;

        private bool HasUseStartAwares => _useStartList.Count > 0 || _useStartInteractionsList.Count > 0;
        private bool HasUseEndAwares => _useEndList.Count > 0 || _useEndInteractionsList.Count > 0;
        
        public override bool IsEnabled => InteractObject?.isUsable ?? false;

        public UseAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject interactObject, InputController inputController) :
            base(objectController, gameObject, interactObject, inputController)
        {
            _useStartList = GameObject.GetComponents<IUseStartAware>().ToList();
            _useStartInteractionsList = GameObject.GetComponents<IUseStartInteractionAware>().ToList();

            _useEndList = GameObject.GetComponents<IUseEndAware>().ToList();
            _useEndInteractionsList = GameObject.GetComponents<IUseEndInteractionAware>().ToList();

            if (HasUseStartAwares)
            {
                AddUseStartBehaviour();
            }

            if (HasUseEndAwares)
            {
                AddUseEndBehaviour();
            }
        }

        #region OVERRIDES

        public override void DisableViewInput()
        {
            InteractObject.isUsable = false;
        }

        public override void EnableViewInput()
        {
            var interactableObjectBehaviour = GameObject.GetComponent<InteractableObjectBehaviour>();

            bool canUse;

            if (interactableObjectBehaviour)
            {
                canUse = interactableObjectBehaviour.IsUsable;
            }
            else
            {
                canUse = HasUseStartAwares || HasUseEndAwares;
            }

            InteractObject.isUsable = canUse;
        }

        protected override void EnableEditorInput()
        {
        }

        protected override void DisableEditorInput()
        {
        }

        #endregion


        private void AddUseStartBehaviour()
        {
            InteractObject.isUsable = true;
            InteractObject.useOverrideButton = ControllerInput.ButtonAlias.TriggerPress;
            InteractObject.InteractableObjectUsed += OnUseStartVoid;
        }

        private void AddUseEndBehaviour()
        {
            InteractObject.isUsable = true;
            InteractObject.useOverrideButton = ControllerInput.ButtonAlias.TriggerPress;
            InteractObject.InteractableObjectUnused += OnUseEndVoid;
        }

        private void OnUseStartVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            UseStart(interactableObjectEventArgs.Hand);
        }

        private void OnUseEndVoid(
            object sender,
            ObjectInteraction.InteractableObjectEventArgs interactableObjectEventArgs)
        {
            UseEnd(interactableObjectEventArgs.Hand);
        }

        public void UseEnd(ControllerInteraction.ControllerHand hand)
        {
            var handGameObject = GetHandGameObject(hand);
            var useContext = new UseInteractionContext(handGameObject, hand);

            _useEndInteractionsList.ForEach(aware => aware?.OnUseEnd(useContext));
            _useEndList.ForEach(legacyAware => legacyAware?.OnUseEnd());
        }

        public void UseStart(ControllerInteraction.ControllerHand hand)
        {
            var handGameObject = GetHandGameObject(hand);
            var useContext = new UseInteractionContext(handGameObject, hand);

            _useStartInteractionsList.ForEach(aware => aware?.OnUseStart(useContext));

            bool hasLegacyInterfaces = _useStartList.Count > 0;
            if (hasLegacyInterfaces)
            {
                var legacyContext = useContext.GetLegacyUsingContext();
                _useStartList.ForEach(legacyAware => legacyAware?.OnUseStart(legacyContext));
            }
        }
    }
}
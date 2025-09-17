using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using Varwin.PlatformAdapter;
using Object = UnityEngine.Object;

namespace Varwin
{
    public class PointerAction : InputAction
    {
        [Obsolete] private readonly List<IPointerClickAware> _pointerClick;
        [Obsolete] private readonly List<IPointerInAware> _pointerIn;
        [Obsolete] private readonly List<IPointerOutAware>_pointerOut;
        [Obsolete] private readonly List<IPointerDownAware> _pointerDown;
        [Obsolete] private readonly List<IPointerUpAware> _pointerUp;

        private readonly List<IPointerClickInteractionAware> _pointerClickInteractions;
        private readonly List<IPointerInInteractionAware> _pointerInInteractions;
        private readonly List<IPointerOutInteractionAware> _pointerOutInteractions;
        private readonly List<IPointerDownInteractionAware> _pointerDownInteractions;
        private readonly List<IPointerUpInteractionAware> _pointerUpInteractions;

        public Action<ControllerInteraction.ControllerHand> OnPointerClick = delegate { };
        public Action<ControllerInteraction.ControllerHand> OnPointerIn = delegate { };
        public Action<ControllerInteraction.ControllerHand> OnPointerOut = delegate { };
        public Action<ControllerInteraction.ControllerHand> OnPointerDown = delegate { };
        public Action<ControllerInteraction.ControllerHand> OnPointerUp = delegate { };
        
        private readonly GameObject _gameObject;
        private bool _isEnabled;
        
        public override bool IsEnabled => _isEnabled;
        
        #region OVERRIDES
        
        public override void DisableViewInput()
        {
            RemovePointerBehaviours();
        }

        public override void EnableViewInput()
        {
            bool hasAnyInteraction = _pointerClick.Any() || _pointerIn.Any() || _pointerOut.Any()
                || _pointerClickInteractions.Any() || _pointerInInteractions.Any() || _pointerOutInteractions.Any();

            if (hasAnyInteraction)
            {
                AddPointerBehaviour();
            }
        }

        protected override void DisableEditorInput()
        {
             
        }

        protected override void EnableEditorInput()
        {
             
        }

        #endregion

        public PointerAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject interactObject, InputController inputController) : base(objectController, gameObject, interactObject, inputController)
        {
            _gameObject = gameObject;

            _pointerClick = _gameObject.GetComponents<IPointerClickAware>().ToList();
            _pointerIn = _gameObject.GetComponents<IPointerInAware>().ToList();
            _pointerOut = _gameObject.GetComponents<IPointerOutAware>().ToList();
            _pointerDown= _gameObject.GetComponents<IPointerDownAware>().ToList();
            _pointerUp= _gameObject.GetComponents<IPointerUpAware>().ToList();

            _pointerClickInteractions = _gameObject.GetComponents<IPointerClickInteractionAware>().ToList();
            _pointerInInteractions = _gameObject.GetComponents<IPointerInInteractionAware>().ToList();
            _pointerOutInteractions = _gameObject.GetComponents<IPointerOutInteractionAware>().ToList();
            _pointerDownInteractions = _gameObject.GetComponents<IPointerDownInteractionAware>().ToList();
            _pointerUpInteractions = _gameObject.GetComponents<IPointerUpInteractionAware>().ToList();

            if (_pointerClick.Any() || _pointerClickInteractions.Any())
            {
                AddPointerClickAction();
            }

            if (_pointerIn.Any() || _pointerInInteractions.Any())
            {
                AddPointerInAction();
            }

            if (_pointerOut.Any() || _pointerOutInteractions.Any())
            {
                AddPointerOutAction();
            }

            if (_pointerDown.Any() || _pointerDownInteractions.Any())
            {
                AddPointerDownAction();
            }

            if (_pointerUp.Any() || _pointerUpInteractions.Any())
            {
                AddPointerUpAction();
            }

            if (_pointerClick.Any() || _pointerIn.Any() || _pointerOut.Any() || _pointerDown.Any() || _pointerUp.Any()
                || _pointerClickInteractions.Any() || _pointerInInteractions.Any() || _pointerOutInteractions.Any()
                || _pointerDownInteractions.Any() || _pointerUpInteractions.Any())
            {
                AddPointerBehaviour();
            }
        }

        private void AddPointerBehaviour()
        {
            ObjectPointerBehaviour pointerBehaviour = _gameObject.GetComponent<ObjectPointerBehaviour>();

            if (pointerBehaviour)
            {
                RemovePointerBehaviours();
            }

            pointerBehaviour = _gameObject.AddComponent<ObjectPointerBehaviour>();
            pointerBehaviour.Init(this);
            _isEnabled = true;
        }
        
        private void RemovePointerBehaviours()
        {
            if (_gameObject)
            {
                var pointerBehaviour = _gameObject.GetComponents<ObjectPointerBehaviour>();

                foreach (var behaviour in pointerBehaviour)
                {
                    Object.Destroy(behaviour);
                }
            }
            
            _isEnabled = false;
        }

        private void AddPointerClickAction()
        {
            OnPointerClick = hand =>
            {
                var context = GetContext(hand);

                _pointerClickInteractions.ForEach(aware => aware?.OnPointerClick(context));
                _pointerClick.ForEach(aware => aware?.OnPointerClick());
            };
        }

        private void AddPointerInAction()
        {
            OnPointerIn = hand =>
            {
                var context = GetContext(hand);

                _pointerInInteractions.ForEach(aware => aware?.OnPointerIn(context));
                _pointerIn.ForEach(aware => aware?.OnPointerIn());
            };
        }

        private void AddPointerOutAction()
        {
            OnPointerOut = hand =>
            {
                var context = GetContext(hand);

                _pointerOutInteractions.ForEach(aware => aware.OnPointerOut(context));
                _pointerOut.ForEach(aware => aware.OnPointerOut());
            };
        }
        
        private void AddPointerDownAction()
        {
            OnPointerDown = hand =>
            {
                var context = GetContext(hand);

                _pointerDownInteractions.ForEach(aware => aware.OnPointerDown(context));
                _pointerDown.ForEach(aware => aware.OnPointerDown());
            };
        }
        
        private void AddPointerUpAction()
        {
            OnPointerUp = hand =>
            {
                var context = GetContext(hand);

                _pointerUpInteractions.ForEach(aware => aware.OnPointerUp(context));
                _pointerUp.ForEach(aware => aware.OnPointerUp());
            };
        }

        private PointerInteractionContext GetContext(ControllerInteraction.ControllerHand hand) => new(GetHandGameObject(hand), hand);
    }
}
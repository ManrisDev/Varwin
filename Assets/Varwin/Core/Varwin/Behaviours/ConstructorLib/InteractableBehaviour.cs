using System;
using Varwin.Public;
using UnityEngine;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    [Obsolete]
    public class InteractableBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            return true;
        }
    }

    [Obsolete]
    [RequireComponentInChildren(typeof(Rigidbody))]
    [RequireComponent(typeof(InteractableObjectBehaviour))]
    [VarwinComponent(English: "Interaction", Russian: "Взаимодействие")]
    public class InteractableBehaviour : VarwinBehaviour
    {
        #region PHYSICS PARAMETERS
        
        private InteractableObjectBehaviour _interactableObject;
        
        [Obsolete]
        public InteractableObjectBehaviour InteractableObject
        {
            get
            {
                if (_interactableObject)
                {
                    return _interactableObject;
                }
                
                _interactableObject = gameObject.GetComponent<InteractableObjectBehaviour>();
                return _interactableObject;
            }
        }
        
        [Obsolete]
        [Variable(English: "Is static", Russian: "Статичный")]
        [VarwinInspector(English: "Is static", Russian: "Статичный")]
        public bool IsStatic { get; set; }
        
        [Obsolete]
        [Variable(English:"Use gravity", Russian:"Гравитация")]
        [VarwinInspector(English: "Use gravity", Russian: "Гравитация")]
        public bool Gravity { get; set; }

        [Obsolete]
        [Variable(English:"Teleport area", Russian:"Зона телепорта")]
        [VarwinInspector(English: "Teleport area", Russian: "Зона телепорта")]
        public bool Teleportable { get; set; }
        
        [Obsolete]
        [Variable(English:"Is obstacle", Russian:"Препятствие")]
        [VarwinInspector(English: "Is obstacle", Russian: "Препятствие")]
        public bool Collision { get; set; }

        [Obsolete]
        [Variable(English:"Mass", Russian:"Масса")]
        [VarwinInspector(English: "Mass", Russian: "Масса")]
        public float Mass { get; set; }

        [Obsolete]
        [Variable(English: "Bounciness", Russian: "Пружинистость")]
        [VarwinInspector(English: "Bounciness", Russian: "Пружинистость")]
        public float Bounciness { get; set; }
        
        #endregion //PHYSICS PARAMETERS
        
        [Obsolete]
        [Variable(English:"Grabbable", Russian:"Можно брать в руку")]
        [VarwinInspector(English: "Grabbable", Russian: "Можно брать в руку")]
        public bool Grabbable { get; set; }
        
        [Obsolete]
        [Variable(English:"Usable", Russian:"Можно использовать")]
        [VarwinInspector(English: "Usable", Russian: "Можно использовать")]
        public bool Usable { get; set; }

        [Obsolete]
        [Variable("Touchable", Russian:"Можно дотронуться")]
        [VarwinInspector(English: "Touchable", Russian: "Можно дотронуться")]
        
        public bool Touchable { get; set; }
        [Obsolete]
        [EventGroup("InteractionEvents")]
        [Event(English:"on grab start", Russian: "объект взят в руку")]
        public event Action GrabStart;
        
        [Obsolete]
        [EventGroup("InteractionEvents")]
        [Event(English: "on grab end", Russian: "объект отпущен из руки")]
        public event Action GrabEnd;
        
        [Obsolete]
        [EventGroup("InteractionEvents")]
        [Event(English:"on use start", Russian: "объект начали использовать")]
        public event Action UseStart;
        
        [Obsolete]
        [EventGroup("InteractionEvents")]
        [Event(English: "on use end", Russian: "объект закончили использовать")]
        public event Action UseEnd;
        
        [Obsolete]
        [EventGroup("InteractionEvents")]
        [Event(English:"on touch start", Russian: "до объекта дотронулись")]
        public event Action TouchStart;
        
        [Obsolete]
        [EventGroup("InteractionEvents")]
        [Event(English: "on touch end", Russian: "объект прекратили трогать")]
        public event Action TouchEnd;

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            
            InteractableObject.OnUseStarted.AddListener(UseStartFire);
            InteractableObject.OnUseEnded.AddListener(UseEndFire);
            InteractableObject.OnGrabStarted.AddListener(GrabStartFire);
            InteractableObject.OnGrabEnded.AddListener(GrabEndFire);
            InteractableObject.OnTouchStarted.AddListener(TouchStartFire);
            InteractableObject.OnTouchEnded.AddListener(TouchEndFire);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            InteractableObject.OnUseStarted.RemoveListener(UseStartFire);
            InteractableObject.OnUseEnded.RemoveListener(UseEndFire);
            InteractableObject.OnGrabStarted.RemoveListener(GrabStartFire);
            InteractableObject.OnGrabEnded.RemoveListener(GrabEndFire);
            InteractableObject.OnTouchStarted.RemoveListener(TouchStartFire);
            InteractableObject.OnTouchEnded.RemoveListener(TouchEndFire);
        }

        private void UseStartFire()
        {
            UseStart?.Invoke();
        }

        private void UseEndFire()
        {
            UseEnd?.Invoke();
        }

        private void GrabStartFire()
        {
            GrabStart?.Invoke();
        }

        private void GrabEndFire()
        {
            GrabEnd?.Invoke();
        }

        private void TouchStartFire()
        {
            TouchStart?.Invoke();
        }

        private void TouchEndFire()
        {
            TouchEnd?.Invoke();
        }
        
        public void SetCollision(bool collision)
        {
            Collision = collision;
        }
        
        public void SetGravity(bool gravity)
        {
            Gravity = gravity;
        }
        
        public void SetKinematic(bool isKinematic)
        {
            IsStatic = isKinematic;
        }
        
        public void SetMass(float mass)
        {
            Mass = mass;
        }
        
        public void SetBounciness(float bounciness)
        {
            Bounciness = bounciness;
        }
    }
}

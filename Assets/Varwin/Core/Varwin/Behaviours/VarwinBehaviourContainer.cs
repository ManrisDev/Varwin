using System;
using UnityEngine;

namespace Varwin.Core.Behaviours
{
    public class VarwinBehaviourContainer
    {
        public readonly Type BehaviourType;
        private readonly VarwinBehaviourHelper _behaviourHelper;

        public VarwinBehaviourContainer(Type behaviourType) : this(behaviourType, new VarwinBehaviourHelper())
        {
        }
        
        public VarwinBehaviourContainer(Type behaviourType, VarwinBehaviourHelper behaviourHelper)
        {
            BehaviourType = behaviourType;
            _behaviourHelper = behaviourHelper;
        }

        public bool CanAddBehaviour(GameObject gameObject)
        {
            return _behaviourHelper.CanAddBehaviour(gameObject, BehaviourType);
        }
    }
}

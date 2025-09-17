using System;

namespace Varwin.Public
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DisableDefaultBehaviourAttribute : Attribute
    {
        public BehaviourType BehaviourType { get; private set; }
        
        public DisableDefaultBehaviourAttribute(BehaviourType type)
        {
            BehaviourType = type;
        }
    }
}
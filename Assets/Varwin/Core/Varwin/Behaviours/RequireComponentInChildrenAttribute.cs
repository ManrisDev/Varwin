using System;

namespace Varwin.Core.Behaviours
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponentInChildrenAttribute : Attribute
    {
        public readonly Type RequiredComponent;

        public RequireComponentInChildrenAttribute(Type requiredComponent)
        {
            RequiredComponent = requiredComponent;
        }
    }
}
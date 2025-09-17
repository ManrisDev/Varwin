using System;

namespace Varwin.Public
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ProtectComponentAttribute : Attribute
    {
        public readonly Type ProtectedComponent;

        public ProtectComponentAttribute(Type protectedComponent)
        {
            ProtectedComponent = protectedComponent;
        }
    }
}

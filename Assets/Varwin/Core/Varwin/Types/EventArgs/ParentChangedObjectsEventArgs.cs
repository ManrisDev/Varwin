using System;
using System.Collections.Generic;

namespace Varwin
{
    public sealed class ParentChangedObjectsEventArgs : EventArgs
    {
        public List<ObjectController> ObjectControllers;
    }
}
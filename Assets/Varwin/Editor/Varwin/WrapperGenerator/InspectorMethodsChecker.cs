using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Varwin.Editor
{
    public static class InspectorMethodsChecker
    {
        public static bool CheckInspectorMethodsSignature(Component component, out List<MethodInfo> wrongMethods)
        {
            var inspectorMethods = component.GetType().GetMethods()
                .Where(x => x.GetCustomAttribute<VarwinInspectorAttribute>() != null);

            wrongMethods = new();
            foreach (var method in inspectorMethods)
            {
                if(method.GetParameters().Length > 0)
                    wrongMethods.Add(method);
            }

            return wrongMethods.Count == 0;
        }
    }
}
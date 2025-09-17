using System;
using System.Collections.Generic;
using System.Linq;

namespace Varwin.Public
{
    [Serializable]
    public class MethodSignature : Signature
    {
        public string ReturnType;
        public List<string> Parameters;

        public bool ParametersAreEquals(MethodSignature other)
        {
            return other.Parameters.SequenceEqual(Parameters); 
        }

        public override string ToString()
        {
            string parametersList = Parameters != null && Parameters.Count > 0 ? Parameters.Aggregate((current, param) => current + ", " + param) : "";
            return $"{ReturnType} {Name} ({parametersList})";
        }
    }
}
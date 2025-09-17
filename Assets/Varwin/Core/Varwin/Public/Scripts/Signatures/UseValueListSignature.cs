using System;
using System.Collections.Generic;
using System.Linq;

namespace Varwin.Public
{
    [Serializable]
    public class UseValueListSignature : Signature
    {
        public List<string> Members;
        
        public bool ContainsAllMembersOf(UseValueListSignature other)
        {
            return other.Members.All(member => Members.Contains(member));
        }
    }
}
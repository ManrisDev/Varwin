using System;
using System.Collections.Generic;
using System.Linq;

namespace Varwin.Public
{
    [Serializable]
    public class GroupSignature : Signature
    {
        public List<string> Members;

        public bool ContainsAllMembersOf(GroupSignature other)
        {
            return other.Members.All(member => Members.Contains(member));
        }
    }
}
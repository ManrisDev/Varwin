using System.Collections.Generic;
using System.Dynamic;

namespace Varwin
{
    public class DynamicWrapperCollection : DynamicCollection<Wrapper>
    {
        public DynamicWrapperCollection(List<Wrapper> collection): base(collection) { }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var success = base.TryInvokeMember(binder, args, out result);

            if (success && binder.Name == "GetBehaviour")
            {
                result = new DynamicCollection<dynamic>(result as List<dynamic>);
            }

            return success;
        }
    }
}
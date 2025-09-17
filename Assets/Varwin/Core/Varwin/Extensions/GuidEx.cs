using System;

namespace Varwin
{
    public static class GuidEx
    {
        public static string Clear(this Guid guid)
        {
            return guid.ToString().Replace("-", "");
        }
    }
}
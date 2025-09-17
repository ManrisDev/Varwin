using System;
using System.Collections.Generic;
using System.Linq;

namespace Varwin.Editor
{
    public static class SdkIgnoredScripts
    {
        private static readonly List<string> IgnoredScripts = new List<string>
        {
            "OVRLipSync"
        };

        public static bool ContainsType(Type type)
        {
            return IgnoredScripts.Any(ignoredScript => type.Name.StartsWith(ignoredScript));
        }
    }
}
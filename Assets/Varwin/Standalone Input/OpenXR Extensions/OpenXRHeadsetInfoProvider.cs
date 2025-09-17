#if VARWIN_OPENXR

using Varwin.OpenXR;
using Varwin.XR;

namespace OpenXR.Extensions
{
    public class OpenXRHeadsetInfoProvider : IHeadsetInfoProvider
    {
        public string GetHeadsetName()
        {
            var namePtr = VarwinOpenXRProvider.XrGetHeadsetName();
            var headsetName = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(namePtr);
            return headsetName;
        }
    }
}

#endif
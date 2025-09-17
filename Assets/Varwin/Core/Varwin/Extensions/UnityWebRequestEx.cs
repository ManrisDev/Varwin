using UnityEngine.Networking;

namespace Varwin
{
    public static class UnityWebRequestEx
    {
        public static bool HasError(this UnityWebRequest webRequest)
        {
            return webRequest.result.IsError();
        }

        public static bool IsError(this UnityWebRequest.Result result)
        {
            return result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError or UnityWebRequest.Result.DataProcessingError;
        }
    }
}
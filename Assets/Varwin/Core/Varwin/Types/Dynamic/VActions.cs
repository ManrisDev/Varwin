using Unity.Netcode;
using UnityEngine;
#if UNITY_ANDROID
using Varwin.Data;
#endif

namespace Varwin
{
    public static class VActions
    {
        public static void ExitProject()
        {
#if VARWINCLIENT && UNITY_ANDROID
            GameStateData.ClearObjects();
            GameStateData.ClearLogic();

            ProjectData.ProjectStructure = null;

            try
            {
                ProjectData.UpdateSubscribes(new RequiredProjectArguments() {GameMode = GameMode.View, PlatformMode = PlatformMode.Vr});
            }
            catch
            {
            }
            finally
            {
                ProjectDataListener.Instance.LoadScene("MobileLauncher");
            }

            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.Shutdown();
            }
#else
            Application.Quit();
#endif
        }
    }
}
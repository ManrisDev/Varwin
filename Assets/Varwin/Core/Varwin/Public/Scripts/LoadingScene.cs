using UnityEngine;

namespace Varwin.Core
{
    public class LoadingScene : MonoBehaviour
    {
        public delegate void LoadingEventHandler(float percent, string feedBack);

        public event LoadingEventHandler ProgressChanged;

        public bool IsStatusVisible
        {
#if VARWINCLIENT
            get => ProjectLoadSystem.LoadingScreenInfo?.IsStatusVisible ?? true;
#else
            get => true;
#endif
        }

        public Texture2D CustomLogo
        {
#if VARWINCLIENT
            get => ProjectLoadSystem.LoadingScreenInfo?.CustomLogo;
#else
            get => null;
#endif
        }

#if VARWINCLIENT
        private void Awake()
        {
            ProjectLoadSystem.ProgressChanged += OnProgressChanged;
        }

        private void OnDestroy()
        {
            ProjectLoadSystem.ProgressChanged -= OnProgressChanged;
        }
#endif

        private void OnProgressChanged(float percent, string feedback)
        {
            ProgressChanged?.Invoke(percent, feedback);
        }
    }
}
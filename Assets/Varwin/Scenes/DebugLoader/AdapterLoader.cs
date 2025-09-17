#if !UNITY_ANDROID
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Management;
using Varwin.PlatformAdapter;
using Varwin.DesktopInput;
using Varwin.Models.Data;
using Varwin.NettleDesk;
#if VARWIN_OPENXR
using Varwin.XR;
#endif

namespace Varwin
{
    public class AdapterLoader : MonoBehaviour
    {
        public static AdapterLoader Instance { get; private set; }
        public event Action VarwinXRStartLoading;
        public event Action VarwinXRCantInitialize;

        private PlatformMode _previousPlatformMode = PlatformMode.Undefined;

        private TransformDT _initialSpawnPoint;

#if VARWIN_OPENXR        
        public PlatformMode PlatformMode;
#endif
        
        public event Action ChangeCurrentAdapterReady;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }

            DontDestroyOnLoad(this);

            ProjectData.PlatformModeChanging += UpdateOldMode;
            ProjectData.PlatformModeChanged += OnPlatformModeChanged;

#if !VARWIN_OPENXR
            ProjectData.PlatformMode = PlatformMode.Desktop;
#endif
        }
        
#if VARWIN_OPENXR
        private void Update()
        {
            if (ProjectData.PlatformMode != PlatformMode)
            {
                ProjectData.PlatformMode = PlatformMode;
            }
        }
#endif        

        private void OnDestroy()
        {
            ProjectData.PlatformModeChanging -= UpdateOldMode;
            ProjectData.PlatformModeChanged -= OnPlatformModeChanged;
        }

        private void UpdateOldMode(PlatformMode oldMode)
        {
            _previousPlatformMode = oldMode;
        }

        private void OnPlatformModeChanged(PlatformMode platformMode)
        {
            if (InputAdapter.Instance != null)
            {
                SaveRigTransform(platformMode);
                ChangeCurrentAdapterReady += DestroyCurrentAdapterPlayer;
            }

            StartCoroutine(SwitchInputAdapter(platformMode));
        }

        private void SaveRigTransform(PlatformMode newMode)
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            Transform spawnPoint = PlayerAnchorManager.SpawnPoint;

            if (!spawnPoint)
            {
                return;
            }
            
            _initialSpawnPoint = spawnPoint.ToTransformDT();
            
            Transform rigTransform = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;

            spawnPoint.position = rigTransform.position;

            if (newMode == PlatformMode.Vr)
            {
                spawnPoint.rotation = rigTransform.rotation;
            }
            else
            {
                Vector3 oldRotation = InputAdapter.Instance.PlayerController.Nodes.Head.Transform.eulerAngles;
                spawnPoint.rotation = Quaternion.Euler(new Vector3(0, oldRotation.y, 0));
            }
        }

        private IEnumerator RestoreInitialSpawnPoint()
        {
            yield return null;
            if (PlayerAnchorManager.SpawnPoint && _initialSpawnPoint != null)
            {
                PlayerAnchorManager.SpawnPoint.position = _initialSpawnPoint.PositionDT.ToUnityVector();
                PlayerAnchorManager.SpawnPoint.rotation = _initialSpawnPoint.RotationDT.ToUnityQuaternion();
            }
            _initialSpawnPoint = null;

            if (!ProjectData.IsPlayMode)
            {
                PlayerManager.Respawn();
            }
        }

        private IEnumerator SwitchInputAdapter(PlatformMode platformMode)
        {
            if (_previousPlatformMode == platformMode)
            {
                yield break;
            }
            
#if VARWIN_OPENXR
            switch (platformMode)
            {
                case PlatformMode.Desktop:
                    DisableVR();

                    yield return new WaitForEndOfFrame();

                    ChangeCurrentAdapterReady?.Invoke();
                    InputAdapter.ChangeCurrentAdapter(new DesktopAdapter());
                    break;
                case PlatformMode.Spectator:
                    DisableVR();

                    yield return new WaitForEndOfFrame();

                    ChangeCurrentAdapterReady?.Invoke();
                    InputAdapter.ChangeCurrentAdapter(new SpectatorAdapter());
                    break;
                case PlatformMode.NettleDesk:
                    DisableVR();

                    yield return new WaitForEndOfFrame();

                    ChangeCurrentAdapterReady?.Invoke();
                    InputAdapter.ChangeCurrentAdapter(new NettleDeskAdapter());
                    break;
                default:
                {
                    while (!XRGeneralSettings.Instance || !XRGeneralSettings.Instance.Manager)
                    {
                        yield return null;
                    }

                    if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
                    {
                        VarwinXRStartLoading?.Invoke();
                        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

                        var xrLoader = XRGeneralSettings.Instance.Manager.activeLoader;

                        if (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
                        {
                            Debug.LogError("Unity can't load XR Loader. Check your OpenXR subsystem works properly");
                            VarwinXRCantInitialize?.Invoke();
                            yield break;
                        }

                        xrLoader.Start();
                        VarwinXRStartLoading?.Invoke();
                        yield return new WaitForEndOfFrame();
                    }

                    ChangeCurrentAdapterReady?.Invoke();
                    InputAdapter.ChangeCurrentAdapter(new VarwinXRAdapter());
                    break;
                }
            }
#else
            DisableVR();

            yield return new WaitForEndOfFrame();

            ChangeCurrentAdapterReady?.Invoke();
            InputAdapter.ChangeCurrentAdapter(new DesktopAdapter());
#endif

            while (InputAdapter.Instance == null)
            {
                yield return null;
            }

            for (int i = 0; i < 10; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            if (InputAdapter.Instance.PlayerController.Nodes.Rig == null || !InputAdapter.Instance.PlayerController.Nodes.Rig.GameObject)
            {
                GameObject playerRig = Instantiate(InputAdapter.Instance.PlayerController.RigInitializer.InitializeRig());

                InputAdapter.Instance.PlayerController.Init(playerRig);

                playerRig.transform.position = PlayerAnchorManager.SpawnPoint ? PlayerAnchorManager.SpawnPoint.position : transform.position;

                var currentCamera = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform.GetComponentInChildren<Camera>();
                if (ProjectData.PlatformMode == PlatformMode.Desktop)
                {
                    CameraManager.DesktopPlayerCamera = currentCamera;
                }
                else
                {
                    CameraManager.VrCamera = currentCamera;
                }
            }

            GameStateData.PlatformModeChanged(platformMode, _previousPlatformMode);
            
            StartCoroutine(RestoreInitialSpawnPoint());
        }

        private void DestroyCurrentAdapterPlayer()
        {
            ChangeCurrentAdapterReady -= DestroyCurrentAdapterPlayer;
            
            if (InputAdapter.Instance?.PlayerController?.Nodes?.Rig?.GameObject)
            {
                Destroy(InputAdapter.Instance.PlayerController.Nodes.Rig.GameObject);
            }
        }

        private void OnApplicationQuit()
        {
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }

        private static void DisableVR()
        {
            if (!XRGeneralSettings.Instance || !XRGeneralSettings.Instance.Manager)
            {
                return;
            }
            
            if (XRGeneralSettings.Instance.Manager.activeLoader)
            {
                XRGeneralSettings.Instance.Manager.activeLoader.Stop();
            }

            if (XRGeneralSettings.Instance.Manager.isInitializationComplete)
            {
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
        }
    }
}
#endif
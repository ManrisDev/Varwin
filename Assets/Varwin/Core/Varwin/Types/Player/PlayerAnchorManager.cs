using System;
using System.Collections;
using System.Linq;
using Varwin;
using UnityEngine;
using Varwin.Data.ServerData;
using Varwin.PlatformAdapter;
using Varwin.Public;
using Scene = Varwin.Data.ServerData.Scene;

public class PlayerAnchorManager : MonoBehaviour
{
    public static event Action SpawnPointInitialized;
    public static event Action<Scene> SceneLoaded;
    public static event Action PlayerPositionInitialized;
    
    private static Transform _spawnPoint;
    public static Transform SpawnPoint
    {
        get => _spawnPoint;
        set
        {
            if (value == _spawnPoint)
            {
                return;
            }

            _spawnPoint = value;

            SpawnPointInitialized?.Invoke();
        }
    }

    private bool _trackingInitialized;
    private Transform _headTransform;
    private GameObject _player;

    private void Awake()
    {
        Varwin.Public.SpawnPoint.DefaultSpawnPointSpawned += OnDefaultSpawnPointSpawned;
        ProjectData.GameModeChanged += RespawnPlayerOnModeChange; 
        InitializePlayer();
    }

    private void OnDestroy()
    {
        Varwin.Public.SpawnPoint.DefaultSpawnPointSpawned -= OnDefaultSpawnPointSpawned;
        ProjectData.GameModeChanged -= RespawnPlayerOnModeChange;
    }
    
    private void RespawnPlayerOnModeChange(GameMode mode)
    {
        StopAllCoroutines();
        SetPlayerPosition();
    }

    private void OnDefaultSpawnPointSpawned(SpawnPoint spawnPoint)
    {
        var scene = ProjectData.ProjectStructure.Scenes.GetProjectScene(ProjectData.SceneId);
        var spawnPointTransform = scene.SceneObjects?.Find(a => a.InstanceId == spawnPoint.gameObject.GetWrapper().GetObjectController().Id)?.Data?.Transform;
        if (spawnPointTransform == null)
        {
            spawnPoint.transform.position = transform.position;
            spawnPoint.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        SpawnPoint = spawnPoint.transform;

        SetPlayerPosition();
    }

    public void InitializePlayer()
    {
        if (!SpawnPoint)
        {
            SpawnPoint = transform;
        }

        if (InputAdapter.Instance == null)
        {
            return;
        }

        if (InputAdapter.Instance.PlayerController.Nodes.Head.GameObject)
        {
            return;
        }

        GameObject playerRig = InputAdapter.Instance.PlayerController.RigInitializer.InitializeRig();

        if (!playerRig)
        {
            return;
        }

        _player = Instantiate(playerRig, SpawnPoint.position, Quaternion.identity);
        InputAdapter.Instance.PlayerController.Init(_player);
        _headTransform = InputAdapter.Instance.PlayerController.Nodes.Head.Transform;
        _trackingInitialized = false;

        var cameras = _player.GetComponentsInChildren<Camera>();
        var vrCamera = cameras.FirstOrDefault(x => x.enabled);

        if (ProjectData.PlatformMode == PlatformMode.Vr)
        {
            CameraManager.VrCamera = vrCamera ? vrCamera : _player.GetComponentInChildren<Camera>(true);
        }
        else
        {
            CameraManager.DesktopPlayerCamera = vrCamera ? vrCamera : _player.GetComponentInChildren<Camera>(true);
        }

        StartCoroutine(MovePlayerToSpawnCoroutine());
    }

    public static void StoreSettingsOnSceneLoad()
    {
        int currentSceneId = ProjectData.SceneId;
        Scene scene = ProjectData.ProjectStructure?.Scenes?.GetProjectScene(currentSceneId);
        
        if (scene != null)
        {
            SceneLoaded?.Invoke(scene);
        }
    }

    private IEnumerator ResetPlayer()
    {
        Debug.Log("turning player off (disabled)");
        
        yield return new WaitForEndOfFrame();
        
        StartCoroutine(InvokeOnLoadSceneTemplate());

        yield return true;
    }

    private static IEnumerator InvokeOnLoadSceneTemplate()
    {
        while (!ProjectData.ObjectsAreLoaded)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        yield return true;
    }

    private IEnumerator WaitForInputAdapterInstance()
    {
        if (!SpawnPoint)
        {
            SpawnPoint = transform;
        }
        
        while (InputAdapter.Instance == null)
        {
            yield return null;
        }
        
        while (InputAdapter.Instance.PlayerController.Nodes.Rig == null)
        {
            yield return null;
        }

        GameObject newPlayer = InputAdapter.Instance.PlayerController.Nodes.Rig.GameObject;
        if (newPlayer)
        {
            if (PlayerManager.CurrentRig != null)
            {
                if (PlayerManager.CurrentRig is MonoBehaviour monoBehaviour && monoBehaviour || PlayerManager.CurrentRig is not MonoBehaviour)
                {
                    PlayerManager.CurrentRig.Position = SpawnPoint.position;
                    PlayerManager.CurrentRig.Rotation = SpawnPoint.rotation;
                }
            }
            
            _player = newPlayer;
            _player.transform.position = SpawnPoint.position;

            var playerCamera = _player.transform.GetComponentInChildren<Camera>();
            if (playerCamera)
            {
                _headTransform = playerCamera.transform;
            }
            _trackingInitialized = false;
        }
    }

    private IEnumerator MovePlayerToSpawnCoroutine()
    {
        PlayerManager.GravityFreeze = true;
        
        yield return WaitForInputAdapterInstance();

        while (!_headTransform || !_player)
        {
            yield return null;
        }
        
        while (!_trackingInitialized)
        {
            Vector3 diff = _headTransform.position - _player.transform.position;

            if (diff != Vector3.zero)
            {
                _trackingInitialized = true;

                Vector3 newRot = _player.transform.eulerAngles;

                newRot.x = 0;
                newRot.y -= _headTransform.transform.eulerAngles.y;

                if (SpawnPoint)
                {
                    newRot.y += SpawnPoint.eulerAngles.y;
                }
                newRot.z = 0;

                _player.transform.rotation = Quaternion.Euler(newRot);

                Vector3 newPos = _player.transform.position;
                diff = _headTransform.position - newPos;
                newPos.x -= diff.x;
                newPos.z -= diff.z;

                _player.transform.position = newPos;

                PlayerManager.FallingTime = 0;

                Debug.Log("Player position initialized");
            }

            yield return null;
        }
        PlayerManager.GravityFreeze = false;
        PlayerPositionInitialized?.Invoke();
    }

    public void RestartPlayer()
    {
        StartCoroutine(RestartPlayerCoroutine());
    }

    private IEnumerator RestartPlayerCoroutine()
    {
        yield return MovePlayerToSpawnCoroutine();
        yield return ResetPlayer();

        yield return true;
    }

    public void SetPlayerPosition()
    {
        StartCoroutine(MovePlayerToSpawnCoroutine());
    }
}

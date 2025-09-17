using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Varwin.Log;
using Varwin;
using Varwin.Data;
using UnityEngine;
using Varwin.Public;
using Varwin.PlatformAdapter;

public class DebugLauncher : MonoBehaviour
{
    public Transform PlayerSpawnPoint;
    public GameMode GameMode;

    private void Update()
    {
        if (ProjectData.GameMode != GameMode)
        {
            ProjectData.GameMode = GameMode;
        }
    }

    private void Awake()
    {
        if (!PlayerSpawnPoint)
        {
            var worldDescriptor = FindObjectOfType<WorldDescriptor>();
            if (worldDescriptor && worldDescriptor.PlayerSpawnPoint)
            {
                PlayerSpawnPoint = worldDescriptor.PlayerSpawnPoint;
            }

            if (!PlayerSpawnPoint)
            {
                PlayerSpawnPoint = transform;
            }
        }
        
        Application.logMessageReceived += ErrorHelper.ErrorHandler;
        Settings.CreateDebugSettings("");
    }

    private IEnumerator Start()
    {
        while (InputAdapter.Instance == null)
        {
            yield return null;
        }
        
        GameObject playerRig = Instantiate(InputAdapter.Instance.PlayerController.RigInitializer.InitializeRig());
        
        InputAdapter.Instance.PlayerController.Init(playerRig);
       
        playerRig.transform.position = PlayerSpawnPoint ? PlayerSpawnPoint.position : Vector3.zero;
        
        ProjectData.GameMode = GameMode;
       
        InitObjectsOnScene();

        // TODO: Когда всплывет баг с переключением режимов в SDK и CameraManager.CurrentCamera — переделать
        var currentCamera = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform.GetComponentInChildren<Camera>();
        if (ProjectData.PlatformMode == PlatformMode.Desktop)
        {
            CameraManager.DesktopPlayerCamera = currentCamera;
        }
        else
        {
            CameraManager.VrCamera = currentCamera;
        }
        
#if VARWIN_CLIENTCORE        
        UIFadeInOutController.Instance.FadeOut();
#endif
    }

    private void InitObjectsOnScene()
    {
        var sceneObjects = GetSceneObjects();

        foreach (var sceneObject in sceneObjects)
        {
            var spawn = new SpawnInitParams
            {
                Name = sceneObject.Value,
                IdScene = 1,
                IdInstance = 0,
                IdObject = 0,
                IdServer = 0
            };
            
            InitObject(0, spawn, sceneObject.Key, null);
        }
    }
    
    public void InitObject(int idObject, SpawnInitParams spawnInitParams, GameObject spawnedGameObject, I18n localizedNames, bool internalSpawn = false)
    {
        GameObject gameObjectLink = spawnedGameObject;
        int idScene = spawnInitParams.IdScene;
        int idServer = spawnInitParams.IdServer;
        int idInstance = spawnInitParams.IdInstance;
        bool embedded = spawnInitParams.Embedded;
        string name = spawnInitParams.Name;
        var parentId = spawnInitParams.ParentId;
#if VARWINCLIENT
        var resources = spawnInitParams.InspectorPropertiesData;
#endif
        bool lockChildren = spawnInitParams.LockChildren;
        bool disableSelectabilityFromScene = spawnInitParams.DisableSelectabilityInEditor;
        bool disableSceneLogic = spawnInitParams.DisableSceneLogic;
        bool isDisabled = spawnInitParams.IsDisabled;
        bool isDisabledInHierarchy = spawnInitParams.IsDisabledInHierarchy;
        int index = spawnInitParams.Index;
        bool sceneTemplateObject = spawnInitParams.SceneTemplateObject;

        ObjectController parent = null;

        if (parentId != null)
        {
            parent = GameStateData.GetObjectControllerInSceneById(parentId.Value);
        }

        if (parent != null && spawnInitParams.VirtualObjectParentId != null)
        {
            var virtualParent = parent.GetVirtualObject(spawnInitParams.VirtualObjectParentId.Value);
            if (virtualParent != null)
            {
                parent = virtualParent;    
            }
        }

        WrappersCollection wrappersCollection = null;

        if (idScene != 0)
        {
            wrappersCollection = GameStateData.GetWrapperCollection();
        }
        
        InitObjectParams initObjectParams = new InitObjectParams
        {
            Id = idInstance,
            IdObject = idObject,
            IdScene = idScene,
            IdServer = idServer,
            Asset = gameObjectLink,
            LocalTransform = spawnInitParams.LocalTransform,
            Name = name,
            RootGameObject = spawnedGameObject,
            WrappersCollection = wrappersCollection,
            Parent = parent,
            Embedded = embedded,
            LocalizedNames = localizedNames,
#if VARWINCLIENT
            ResourcesPropertyData = resources,
#endif
            LockChildren = lockChildren,
            DisableSelectabilityInEditor = disableSelectabilityFromScene,
            DisableSceneLogic = disableSceneLogic,
            IsDisabled = isDisabled,
            IsDisabledInHierarchy = isDisabledInHierarchy,
            Index = index,
            SceneTemplateObject = sceneTemplateObject,
            VirtualObjectsData = spawnInitParams.VirtualObjectInfos
        };

        var rootObjectId = spawnedGameObject.GetComponent<ObjectId>();
        var transforms = spawnInitParams.Transforms;
        if (transforms != null && rootObjectId && transforms.ContainsKey(rootObjectId.Id))
        {
            initObjectParams.WorldTransform = transforms[rootObjectId.Id];
        }

        var newController = new ObjectController(initObjectParams);

        try
        {
            ProjectData.OnObjectSpawned(newController, internalSpawn, spawnInitParams.Duplicated, spawnInitParams.SpawnedByHierarchy);
        }
        catch (Exception e)
        {
            Debug.LogError("Can not invoke method on spawn object in " + newController.Name);
            Debug.LogError(e.Message + e.StackTrace);
        }
    }

    private Dictionary<GameObject, string> GetSceneObjects()
    {
        var sceneObjects = new Dictionary<GameObject, string>();
        
        var descriptors = FindObjectsOfType<VarwinObjectDescriptor>();
        foreach (VarwinObjectDescriptor descriptor in descriptors)
        {
            if (!sceneObjects.ContainsKey(descriptor.gameObject))
            {
                sceneObjects.Add(descriptor.gameObject, descriptor.Name);
            }
        }
        
        var monoBehaviours = FindObjectsOfType<MonoBehaviour>().Where(x => x is IVarwinInputAware);
        foreach (MonoBehaviour monoBehaviour in monoBehaviours)
        {
            if (monoBehaviour.GetComponentInParent<VarwinObjectDescriptor>())
            {
                continue;
            }
            
            if (!sceneObjects.ContainsKey(monoBehaviour.gameObject))
            {
                sceneObjects.Add(monoBehaviour.gameObject, monoBehaviour.name);
            }
        }

        return sceneObjects;
    }
}

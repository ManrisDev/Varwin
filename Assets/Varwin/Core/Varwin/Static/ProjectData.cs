using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Varwin.Core;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.PlatformAdapter;
#if VARWINCLIENT
using Varwin.UI;
using Varwin.WWW;
#endif
namespace Varwin
{
    public static class ProjectData
    {
        [Obsolete]
        public static bool IsMultiplayerAndMasterClient => false;
        
        //public static Dictionary<string, string> LaunchArguments { get; } = new();
        public static List<string> LaunchArguments { get; set; } = new();

        /// <summary>
        /// Current project id
        /// </summary>
        public static int ProjectId { get; private set; }

        /// <summary>
        /// Current scene id
        /// </summary>
        public static int SceneId { get; private set; }

        public static bool IsRichSceneTemplate { get; set; }

        /// <summary>
        /// Current scene template id
        /// </summary>
        public static int SceneTemplateId => ProjectStructure.Scenes.GetSceneTemplateId(SceneId);

        /// <summary>
        /// Current project configuration id
        /// </summary>
        public static int ProjectConfigurationId { get; set; }

        /// <summary>
        /// Object Id in hand
        /// </summary>
        public static int SelectedObjectIdToSpawn { get; set; }

        private static bool _interactionWithObjectsLocked;

        /// <summary>
        /// If it possible to player to interact with objects
        /// </summary>
        public static bool InteractionWithObjectsLocked
        {
            get => _interactionWithObjectsLocked;
            set
            {
                if (_interactionWithObjectsLocked == value)
                {
                    return;
                }

                _interactionWithObjectsLocked = value;

                if (_interactionWithObjectsLocked)
                {
                    var leftHand = InputAdapter.Instance?.PlayerController?.Nodes?.GetControllerReference(ControllerInteraction.ControllerHand.Left);
                    leftHand?.Controller?.StopInteraction();

                    var rightHand = InputAdapter.Instance?.PlayerController?.Nodes?.GetControllerReference(ControllerInteraction.ControllerHand.Right);
                    rightHand?.Controller?.StopInteraction();
                }

                InteractionWithObjectsLockedChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Invoked when InteractionWithObjectsLocked was changed
        /// </summary>
        public static event Action<bool> InteractionWithObjectsLockedChanged;

        /// <summary>
        /// All objects are loaded
        /// </summary>
        public static bool ObjectsAreLoaded { get; set; }

        /// <summary>
        /// Amount of objects in library
        /// </summary>
        public static int ObjectsCount { get; set; }

        /// <summary>
        /// All resources are loaded
        /// </summary>
        public static bool ResourcesAreLoaded { get; set; }

        /// <summary>
        /// Called when scene was changed in Editor (or Edit Mode)
        /// </summary>
        public static event Action SceneChanged;

        /// <summary>
        /// Called when scene was renamed
        /// </summary>
        public static event Action SceneRenamed;

        /// <summary>
        /// Called when project meta was changed
        /// </summary>
        public static event Action ProjectStructureChanged;

        /// <summary>
        /// User modify something?
        /// </summary>
        [Obsolete("The property is obsolete and will be removed. Use the SceneObjectsChanged event instead.")]
        public static bool ObjectsAreChanged { get; set; }

        /// <summary>
        /// Loading scenes with project structure key
        /// </summary>
        public static Dictionary<int, string> LoadingScenePaths { get; set; } = new Dictionary<int, string>();

        /// <summary>
        /// Current project structure data
        /// </summary>
        private static ProjectStructure _projectStructure;

        public static ProjectStructure ProjectStructure
        {
            get => _projectStructure;
            set
            {
                _projectStructure = value;
                ProjectStructureChanged?.Invoke();
            }
        }

        public delegate void ObjectsLoadedHandler();

        /// <summary>
        /// Action when objects are loaded
        /// </summary>
        public static event ObjectsLoadedHandler ObjectsLoaded;

        public delegate void ResourcesLoadedHandler();

        /// <summary>
        /// Action when resources are loaded
        /// </summary>
        public static event ResourcesLoadedHandler ResourcesLoaded;

        public delegate void SceneLoadedHandler();

        /// <summary>
        /// Action when scene is loaded
        /// </summary>
        public static event SceneLoadedHandler SceneLoaded;

        public delegate void SceneClearedHandler();

        /// <summary>
        /// Action when objects are loaded
        /// </summary>
        public static event SceneClearedHandler SceneCleared;

        /// <summary>
        /// Action when user save data
        /// </summary>
        public static Action<bool> OnSave { get; set; }

        public static Action OnPreSave { get; set; }

        public delegate void ParentChangedObjectsHandler(ParentChangedObjectsEventArgs args);

        public static event ParentChangedObjectsHandler ParentChangedObjects;

        public delegate void DeleteObjectHandler(DeleteObjectEventArgs args);

        public static event DeleteObjectHandler DeletingObject;

        public delegate void ObjectSpawnedHandler(ObjectSpawnedEventArgs args);

        public static event ObjectSpawnedHandler ObjectSpawned;

        public static event Action ObjectSpawnProcessCompleted;
        public static event Action ObjectInitialSpawnProcessCompleted;

        public delegate void GameModeChangeHandler(GameMode newGameMode);

        public delegate void PlatformModeChangedHandler(PlatformMode newPlatformMode);

        public static event GameModeChangeHandler GameModeChanging;
        public static event GameModeChangeHandler GameModeChanged;

        public static event PlatformModeChangedHandler PlatformModeChanging;
        public static event PlatformModeChangedHandler PlatformModeChanged;

        public static event Action ProjectRemoved;
        public static event Action CurrentSceneRemoved;
        public static event Action CurrentSceneTemplateChanged;
        public static event Action CurrentSceneObjectsChanged;
        public static event Action LibraryItemsReplaced;
        public static event Action MobileReadyChanged;
        public static event Action TerminateSignalReceived;
        public static event Action LaunchSignalReceived;
        public static event Action ProjectReloaded;
        public static event Action SceneObjectsChanged;
        public static event Action<bool> UIOverlapStatusChaned;
        public static event Action SceneIsBackedUp;
        public static event Func<bool> ProjectLaunchConfirmed;
        
        private static ExecutionMode _executionMode = ExecutionMode.Undefined;
        private static GameMode _gm = GameMode.Undefined;
        private static PlatformMode _platform = PlatformMode.Undefined;

        public static bool ModeChangingEnabled { get; set; } = true;

        public static bool IsLobbySceneActive => SceneManager.GetActiveScene().buildIndex == 2;
        public static bool IsEnterHostIpSceneActive => SceneManager.GetActiveScene().buildIndex == 3;
        public static bool IsMultiplayerSceneActive => IsLobbySceneActive || IsEnterHostIpSceneActive;

        public static GameMode GameMode
        {
            get => _gm;
            set => GameModeChange(value);
        }
        
        public static PlatformMode PlatformMode
        {
            get => _platform;
            set => PlatformModeChange(value);
        }

        public static ExecutionMode ExecutionMode
        {
            get => _executionMode;
            set => ExecutionModeChange(value);
        }

        public static MultiplayerMode MultiplayerMode
        {
            get
            {
                if (!NetworkManager.Singleton)
                {
                    return MultiplayerMode.Single;
                }
                
                if (NetworkManager.Singleton && NetworkManager.Singleton.IsClient && NetworkManager.Singleton.IsServer)
                {
                    return MultiplayerMode.Host;
                }
                
                if (NetworkManager.Singleton && NetworkManager.Singleton.IsClient)
                {
                    return MultiplayerMode.Client;
                }

                return MultiplayerMode.Undefined;
            }
        }

        public static bool IsPlayMode => GameMode == GameMode.Preview || GameMode == GameMode.View;

        public static bool IsDesktopEditor => PlatformMode == PlatformMode.Desktop && GameMode == GameMode.Edit;

        public static bool IsMobileVr()
        {
#if UNITY_EDITOR || !UNITY_ANDROID
            return false;
#else
            return IsMobileClient && PlatformMode == PlatformMode.Vr;
#endif
        }

        public static bool IsLinux()
        {
#if UNITY_STANDALONE_LINUX
            return true;
#endif
            return false;
        }

        /// <summary>
        /// Returns whether current platform is Mobile
        /// </summary>
        public static bool IsMobileClient
        {
            get
            {
#if UNITY_EDITOR || !UNITY_ANDROID
                return false;
#else
                return true;
#endif
            }
        }

        public static Data.ServerData.Scene CurrentScene => ProjectStructure.Scenes.FirstOrDefault(x => x.Id == SceneId);

#if VARWINCLIENT
        public static void UpdateSubscribes(RequiredProjectArguments arguments)
        {
            ProjectId = arguments.ProjectId;
            SceneId = arguments.SceneId;
            ProjectConfigurationId = arguments.ProjectConfigurationId;
            GameMode = arguments.GameMode;
            PlatformMode = arguments.PlatformMode;

            if (_executionMode != ExecutionMode.EXE || GameMode != GameMode.View)
            {
                var lockedObjects = GetLockedSceneObjects(CurrentScene.SceneObjects);
                GameStateData.UpdateSceneObjectLockedIds(lockedObjects);
            }

            Debug.Log($"Project data was updated: {arguments}");
        }
#endif
        
        private static List<GameStateData.LockedSceneObject> GetLockedSceneObjects(IEnumerable<SceneObjectDto> sceneObjects)
        {
            var lockedObjects = new List<GameStateData.LockedSceneObject>();

            if (sceneObjects == null)
            {
                return lockedObjects;
            }
            
            foreach (var sceneObject in sceneObjects)
            {
                if (sceneObject.UsedInSceneLogicInternal)
                {
                    lockedObjects.Add(new GameStateData.LockedSceneObject
                    {
                        Id = sceneObject.Id ?? 0,
                        UsedInSceneLogic = true
                    });
                }
                
                lockedObjects.AddRange(GetLockedSceneObjects(sceneObject.SceneObjects));
            }

            return lockedObjects;
        }

        private static void PlatformModeChange(PlatformMode newValue)
        {
            if (newValue == _platform)
            {
                return;
            }

#if VARWINCLIENT && UNITY_STANDALONE
            if (newValue == PlatformMode.Vr && !FeatureManager.IsAvailable(Data.Feature.VR))
            {
                var title = I18next.Format("PLATFORM_MODE_IS_NOT_AVAILABLE_TITLE".Localize(), new KeyValuePair<string, object>("platform_mode", "MULTIPLAYER_VR".Localize()));
                var message = I18next.Format("PLATFORM_MODE_IS_NOT_AVAILABLE_UNDER_LICENSE".Localize(), new KeyValuePair<string, object>("platform_mode", "MULTIPLAYER_VR".Localize()));
                PlatformModeUsageErrorWindow.ShowError(title, message, PlatformMode.Vr);
                throw new ArgumentException(message);
            }
            if (newValue == PlatformMode.NettleDesk && !FeatureManager.IsAvailable(Data.Feature.NettleDesk))
            {
                var title = I18next.Format("PLATFORM_MODE_IS_NOT_AVAILABLE_TITLE".Localize(), new KeyValuePair<string, object>("platform_mode", "MULTIPLAYER_NETTLEDESK".Localize()));
                var message = I18next.Format("PLATFORM_MODE_IS_NOT_AVAILABLE_UNDER_LICENSE".Localize(), new KeyValuePair<string, object>("platform_mode", "MULTIPLAYER_NETTLEDESK".Localize()));
                PlatformModeUsageErrorWindow.ShowError(title, message, PlatformMode.NettleDesk);
                throw new ArgumentException(message);
            }
#endif
            
            PlatformMode oldValue = _platform;
            _platform = newValue;

            PlatformModeChanging?.Invoke(oldValue);
            PlatformModeChanged?.Invoke(_platform);
        }

        private static void GameModeChange(GameMode newValue)
        {
            if (newValue == _gm)
            {
                return;
            }

            InteractionWithObjectsLocked = false;

            GameMode oldGm = _gm;
            _gm = newValue;
#if VARWINCLIENT
            Helper.HideUi();
#endif

            GameModeChanging?.Invoke(_gm);
            GameStateData.GameModeChanged(_gm, oldGm);
            GameModeChanged?.Invoke(_gm);
            Debug.Log($"<Color=Yellow>Game mode changed to {_gm.ToString()}</Color>");
        }

        private static void ExecutionModeChange(ExecutionMode newValue)
        {
            if (newValue == _executionMode)
            {
                return;
            }

            if (_executionMode == ExecutionMode.Undefined)
            {
                _executionMode = newValue;
            }
        }

        public static void ObjectsWasLoaded()
        {
            ObjectsLoaded?.Invoke();
        }

        public static void SceneWasLoaded()
        {
            SceneLoaded?.Invoke();
#if VARWINCLIENT
            ProjectDataListener.ReadyToGetNewMessages();
#endif
        }

        public static void OnSceneCleared()
        {
            SceneCleared?.Invoke();
        }

        public static void OnObjectSpawned(ObjectController objectController, bool internalSpawn, bool duplicated = false, bool spawnedByHierarchy = false)
        {
            ObjectSpawned?.Invoke(new ObjectSpawnedEventArgs
            {
                ObjectController = objectController,
                InternalSpawn = internalSpawn,
                Duplicated = duplicated,
                SpawnedByHierarchy = spawnedByHierarchy
            });
        }

        public static void OnObjectSpawnProcessCompleted()
        {
            ObjectSpawnProcessCompleted?.Invoke();
        }

        /// <summary>
        /// Вызывается, когда заспаунятся все объекты при старте DEsktopEditor
        /// </summary>
        public static void OnObjectInitialSpawnProcessCompleted()
        {
            ObjectInitialSpawnProcessCompleted?.Invoke();
        }

        public static void ResourcesWasLoaded()
        {
            ResourcesLoaded?.Invoke();
        }

        public static void OnParentChangedObjects(List<ObjectController> objectControllers)
        {
            ParentChangedObjects?.Invoke(new ParentChangedObjectsEventArgs {ObjectControllers = objectControllers});
        }

        public static void OnDeletingObject(ObjectController objectController)
        {
            DeletingObject?.Invoke(new DeleteObjectEventArgs {ObjectController = objectController});
        }

        public static void UpdateSceneData(CustomSceneData sceneData)
        {
            Data.ServerData.Scene currentScene = ProjectStructure.Scenes.GetProjectScene(SceneId);
            currentScene.Data = sceneData;
        }

        [Obsolete]
        public static void UpdateSceneLogic(int sceneId)
        {
        }

        public static void OnSceneChanged()
        {
            ObjectsAreChanged = true;
            SceneChanged?.Invoke();
            SceneObjectsChanged?.Invoke();
        }

        public static void RenameCurrentScene(string name)
        {
            CurrentScene.Name = name;
            SceneRenamed?.Invoke();
        }

        public static void OnSceneCreated()
        {
#if VARWINCLIENT
            AsyncRunner.Execute(GraphQLApi.GetProjectMeta(ProjectId), projectStructure => ProjectStructure = projectStructure);
#endif
        }

        public static void OnProjectRemoved() => ProjectRemoved?.Invoke();

        public static void OnCurrentSceneRemoved() => CurrentSceneRemoved?.Invoke();

        public static void OnCurrentSceneTemplateChanged() => CurrentSceneTemplateChanged?.Invoke();

        public static void OnMobileReadyChanged() => MobileReadyChanged?.Invoke();

        public static void OnCurrentSceneObjectsChanged() => CurrentSceneObjectsChanged?.Invoke();

        public static void OnLibraryItemsReplaced() => LibraryItemsReplaced?.Invoke();

        public static void OnLaunchSignal() => LaunchSignalReceived?.Invoke();

        public static void OnTerminateSignal() => TerminateSignalReceived?.Invoke();

        public static void OnProjectReload() => ProjectReloaded?.Invoke();

        public static void OnUIOverlapStatusChanged(bool isOverlapActive) => UIOverlapStatusChaned?.Invoke(isOverlapActive);

        public static void OnSceneObjectsChanged() => SceneObjectsChanged?.Invoke();

        public static void OnSceneBackedUp() => SceneIsBackedUp?.Invoke();

        public static bool IsProjectLoadingConfirmed()
        {
            return ProjectLaunchConfirmed?.Invoke() ?? false;
        }
    }
}
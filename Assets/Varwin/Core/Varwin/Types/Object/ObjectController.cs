using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using Varwin.Core.Behaviours;
using Varwin.Core.Behaviours.ConstructorLib;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.Models.Data;
using Varwin.Public;
using Varwin.PlatformAdapter;
using Object = UnityEngine.Object;
#if VARWIN_CLIENTCORE

#endif

#pragma warning disable 618

namespace Varwin
{
    /// <summary>
    /// Base class for all objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ObjectController
    {
        #region PRIVATE VARS

        private static readonly HashSet<string> IgnoredBehaviours = new HashSet<string>
        {
            "Interactable",
            "Detachable",
            "VelocityEstimator",
            "SteamVRBooleanAction",
            "SteamVRInteractableObject",

            "HighlightEffect",
            "VarwinHighlightEffect",
            "DefaultHighlighter",
        };

#if VARWIN_CLIENTCORE
        private HierarchyController _hierarchyController;
#endif

        private readonly List<string> _objectBehaviours = new List<string>();

#if VARWIN_CLIENTCORE
        private ComponentsData _componentsData;
#endif
        private Dictionary<int, InputController> _inputControllers = new Dictionary<int, InputController>();
        private Dictionary<Transform, bool> _rigidBodyKinematicsDefaults = new Dictionary<Transform, bool>();

        private List<GameModeSwitchController> _gameModeSwitchControllers = new List<GameModeSwitchController>();
        private List<PlatformModeSwitchController> _platformModeSwitchControllers = new List<PlatformModeSwitchController>();

        private List<ColliderController> _colliderControllers = new List<ColliderController>();
        private List<Rigidbody> _rigidbodies = new List<Rigidbody>();
        private List<ObjectController> _virtualObjects = new();

        private I18n _localizedNames;

        public Vector3 LocalPosition
        {
#if VARWIN_CLIENTCORE
            get => _hierarchyController.GetLocalPosition();
            set => _hierarchyController.SetLocalPosition(value);
#else
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
#endif
        }
        
        public Vector3 LocalEulerAngles
        {
#if VARWIN_CLIENTCORE
            get => _hierarchyController.GetLocalEulerAngles();
            set => _hierarchyController.SetLocalEulerAngles(value);
#else
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
#endif
        }

        public Vector3 LocalScale
        {
#if VARWIN_CLIENTCORE
            get => _hierarchyController.GetLocalScale();
            set => _hierarchyController.SetLocalScale(value);
#else
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
#endif
        }

        private bool _activeInHierarchy;
        private bool _activeSelf = true;

        public bool IsDeleted { get; private set; }

        #endregion

        #region PUBLIC VARS

        public readonly VarwinObjectDescriptor VarwinObjectDescriptor;

#if VARWIN_CLIENTCORE
        /// <summary>
        /// Methods with VarwinInspector
        /// </summary>
        [Obsolete]
        public Dictionary<string, InspectorMethod> InspectorMethods => null;
#endif

#if VARWIN_CLIENTCORE
        [Obsolete]
        public HashSet<Type> InspectorComponentsTypes => null;
#endif

        /// <summary>
        /// Event after changed property value
        /// </summary>
        public event Action<string, object> PropertyValueChanged;

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Link to gameObject with IWrapperAware
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// Link to root Rigidbody
        /// </summary>
        public Rigidbody RigidBody { get; private set; }

        /// <summary>
        /// Link to root gameObject
        /// </summary>
        public GameObject RootGameObject { get; private set; }

        /// <summary>
        /// Object Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Id inside group
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Scene Id 
        /// </summary>
        public int IdScene { get; private set; }

        /// <summary>
        /// Id object inside server
        /// </summary>
        public int IdServer { get; set; }

        /// <summary>
        /// Object Type Id. Used to save.
        /// </summary>
        public int IdObject { get; private set; }

        public bool IsVirtualObject { get; private set; }

        public bool IsEmbedded { get; private set; }

        /// <summary>
        /// Is Scene Template Object
        /// </summary>
        public bool IsSceneTemplateObject { get; private set; }

        /// <summary>
        /// Multiplayer object option
        /// </summary>
        public Collaboration Collaboration { get; private set; }

        public bool IsPlayerObject { get; private set; }

        public WrappersCollection WrappersCollection { get; private set; }

        public PrefabObject PrefabObject => GameStateData.GetPrefabData(IdObject);

        public event Action<ObjectController, bool> ActivityChanged;

        public event Action InspectorRefreshed;

        public bool ActiveInHierarchy
        {
            get => _activeInHierarchy;
            private set
            {
                _activeInHierarchy = value;

                RootGameObject.SetActive(_activeInHierarchy);

                if (_activeInHierarchy)
                {
                    foreach (InputController controller in _inputControllers.Values)
                    {
                        controller.GameModeChanged(ProjectData.GameMode);
                    }
                }

                UpdateActiveStatusParenting();

                ActivityChanged?.Invoke(this, _activeInHierarchy);
            }
        }

        public bool ActiveSelf
        {
            get => _activeSelf;
            private set
            {
                _activeSelf = value;

                if (!LockChildren && ProjectData.IsPlayMode)
                {
                    ActiveInHierarchy = _activeSelf;
                    return;
                }

                if (LockParent == this || Parent && Parent.ActiveInHierarchy)
                {
                    ActiveInHierarchy = _activeSelf;

                    foreach (var child in Descendants)
                    {
                        if (ActiveInHierarchy)
                        {
                            child.ActiveInHierarchy = child.Parent.ActiveInHierarchy && child.ActiveSelf;
                        }
                        else
                        {
                            child.ActiveInHierarchy = ActiveInHierarchy;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lock children transforms
        /// </summary>
        public bool LockChildren
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().LockChildren;
            set => GetHierarchyController().LockChildren = value;
#else
            get => false;
            set {}
#endif
        }

        /// <summary>
        /// Enabled this allows to select objects 
        /// </summary>
        public bool SelectableInEditor { get; set; }

        /// <summary>
        /// Enable this object instance in Blockly Editor
        /// </summary>
        public bool EnableInLogicEditor { get; set; }

        /// <summary>
        /// Sort order index
        /// </summary>
        public int Index
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().Index;
            set => GetHierarchyController().Index = value;
#else
            get => -1;
            set {}
#endif
        }

        public ObjectController Parent
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().Parent?.ObjectController;
#else
            get => null;
#endif
        }

        public ObjectController LockParent
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().LockParent.ObjectController;
#else
            get => null;
#endif
        }

        public List<ObjectController> Children
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().Children.Select(x => x.ObjectController).ToList();
#else
            get => null;
#endif
        }

        public List<ObjectController> Descendants
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().Descendants.Select(x => x.ObjectController).ToList();
#else
            get => null;
#endif
        }

        public int ParentId
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().ParentId;
#else
            get => -1;
#endif
        }

        public bool HierarchyExpandedState
        {
#if VARWIN_CLIENTCORE
            get => GetHierarchyController().TreeExpandedState;
            set => GetHierarchyController().TreeExpandedState = value;
#else
            get => false;
            set {}
#endif
        }

        public event Action ParentChanged;

        public bool IsSelectedInEditor { get; set; }

        #endregion
        
        public ObjectController(InitObjectParams initObjectParams)
        {
            _localizedNames = initObjectParams.LocalizedNames;
            IsEmbedded = initObjectParams.Embedded;
            IsSceneTemplateObject = initObjectParams.SceneTemplateObject;
            IsVirtualObject = initObjectParams.IsVirtualObject;

            RootGameObject = initObjectParams.RootGameObject;
            gameObject = initObjectParams.Asset;

            VarwinObjectDescriptor = gameObject.GetComponent<VarwinObjectDescriptor>();
            VarwinObjectDescriptor.InitObjectController(this);
            
            RigidBody = gameObject.GetComponent<Rigidbody>();

#if VARWIN_CLIENTCORE
            GetHierarchyController();
#endif
            
            Id = initObjectParams.Id;
#if VARWIN_SDK
            Id = gameObject.GetInstanceID();      
#endif
            Index = initObjectParams.Index;
            IdObject = initObjectParams.IdObject;
            IdScene = initObjectParams.IdScene;
            IdServer = initObjectParams.IdServer;
            Name = initObjectParams.Name;
            WrappersCollection = initObjectParams.WrappersCollection;
            LockChildren = initObjectParams.LockChildren;
            SelectableInEditor = !initObjectParams.DisableSelectabilityInEditor;
            EnableInLogicEditor = !initObjectParams.DisableSceneLogic;
            ActiveSelf = !initObjectParams.IsDisabled;
            ActiveInHierarchy = !initObjectParams.IsDisabledInHierarchy;

#if VARWIN_CLIENTCORE
            _hierarchyController.SetParentWithoutNotify(initObjectParams.Parent?._hierarchyController, Index, true);
#endif
            SetTransform(initObjectParams);
            InvokeParentChangedEvent();

            int instanceId = Id;
            this.RegisterMeInScene(ref instanceId, Name);

            Id = instanceId;
            SetName(Name);

            RootGameObject.AddComponent<ObjectBehaviourWrapper>().OwdObjectController = this;

            AddWrapper();
            AddBehaviours();
#if VARWIN_CLIENTCORE
            _componentsData = new ComponentsData(this, initObjectParams.ResourcesPropertyData);
#endif
            AddRequiredComponents();

            SaveKinematics();
            InitCollaboration();
            InitVirtualObjects(initObjectParams);
            ApplyGameMode(ProjectData.GameMode, ProjectData.GameMode);
            
            Create();

            ParentChanged += UpdateActivityOnParentChanged;
            
            foreach (var varwinObject in gameObject.GetComponentsInChildren<VarwinObject>(true))
            {
                varwinObject.OnObjectInitialized();
            }
            
            ExecuteSwitchGameModeOnObject(ProjectData.GameMode, ProjectData.GameMode);
            ExecuteSwitchPlatformModeOnObject(ProjectData.PlatformMode, ProjectData.PlatformMode);
        }

        private void SetTransform(InitObjectParams initObjectParams)
        {
#if VARWIN_CLIENTCORE
            if (initObjectParams.LocalTransform != null)
            {
                _hierarchyController.SetLocalPosition(initObjectParams.LocalTransform.PositionDT);
                _hierarchyController.SetLocalEulerAngles(initObjectParams.LocalTransform.EulerAnglesDT);
                _hierarchyController.SetLocalScale(initObjectParams.LocalTransform.ScaleDT);
            }
            else if (initObjectParams.WorldTransform != null)
            {
                _hierarchyController.GetAffectedTransform().position = initObjectParams.WorldTransform.PositionDT;
                _hierarchyController.GetAffectedTransform().rotation = initObjectParams.WorldTransform.RotationDT;
                _hierarchyController.SetLocalScale(initObjectParams.WorldTransform.ScaleDT);
            }
#endif
        }

#if VARWIN_CLIENTCORE
        public HierarchyController GetHierarchyController()
        {
            return _hierarchyController ??= new HierarchyController(this);
        }
#endif
        
        #region Activity

        public void SetActive(bool isActive)
        {
            ActiveSelf = isActive;
        }

        private void UpdateActivityOnParentChanged()
        {
            if (Parent == null)
            {
                ActiveInHierarchy = ActiveSelf;
                foreach (var child in Children)
                {
                    child.UpdateActivityOnParentChanged();
                }

                return;
            }

            if (Parent.ActiveInHierarchy)
            {
                // set active self to the same value to update descendants activity if needed
                ActiveSelf = _activeSelf;
                return;
            }

            ActiveInHierarchy = Parent.ActiveInHierarchy;
            foreach (var child in Descendants)
            {
                child.ActiveInHierarchy = Parent.ActiveInHierarchy;
            }
        }

        private void UpdateActiveStatusParenting()
        {
            if (ProjectData.IsPlayMode)
            {
                if (IsVirtualObject)
                {
                    return;
                }
                
                RootGameObject.transform.parent = ActiveInHierarchy ? null : Parent?.RootGameObject.transform;
            }
        }

        #endregion Activity

        #region Init

        private void InitCollaboration()
        {
            IPlayerObject varwinObject = gameObject.GetComponent<IPlayerObject>();

            if (varwinObject == null)
            {
                return;
            }

            var nodes = new PlayerNodes
            {
                Head = InputAdapter.Instance?.PlayerController?.Nodes?.Head?.Transform,
                LeftHand = InputAdapter.Instance?.PlayerController?.Nodes?.LeftHand?.Transform,
                RightHand = InputAdapter.Instance?.PlayerController?.Nodes?.RightHand?.Transform
            };
            
            varwinObject.SetNodes(nodes);
            IsPlayerObject = true;

            GameStateData.SetPlayerObject(PrefabObject);
        }

        private void InitVirtualObjects(InitObjectParams initObjectParams)
        {
            if (IsVirtualObject)
            {
                return;
            }
            
            _virtualObjects.Clear();
            foreach (var virtualObject in gameObject.GetComponentsInChildren<VirtualObject>())
            {
                if (virtualObject.gameObject == gameObject)
                {
                    continue;
                }

                var virtualObjectInfo = initObjectParams.VirtualObjectsData?.Find(a=>a.Id == virtualObject.IdObject);
                
                virtualObject.Initialize(this, virtualObjectInfo);
                _virtualObjects.Add(virtualObject.ObjectController);
                
                if (virtualObjectInfo != null)
                {
                    ProjectData.CurrentScene?.Data?.HierarchyExpandStates?.TryAdd(virtualObject.ObjectController.Id, virtualObjectInfo.HierarchyExpandedState);
                    virtualObject.ObjectController.HierarchyExpandedState = virtualObjectInfo.HierarchyExpandedState;
                }
            }
        }

        private void Create()
        {
            var monoBehaviours = RootGameObject.GetComponentsInChildren<MonoBehaviour>().ToList();
            monoBehaviours.RemoveAll(x => !x || IgnoredBehaviours.Contains(x.GetType().Name));

            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                if (!monoBehaviour)
                {
                    continue;
                }

                MethodInfo method = monoBehaviour.GetType()
                    .GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (method == null || method.GetParameters().Length != 0)
                {
                    continue;
                }

                try
                {
                    method.Invoke(monoBehaviour, null);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Caught error when trying to invoke Create method in object {Name}:{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void SetServerId(int id)
        {
            IdServer = id;
        }

        public void SetName(string newName)
        {
            Name = newName;
        }

        #endregion

        #region Add

        /// <summary>
        /// Добавление стандартных поведений на объект.
        /// </summary>
        private void AddBehaviours()
        {
            _objectBehaviours.AddRange(BehavioursCollection.GetBehaviours(this));
            if (VarwinObjectDescriptor is { AddBehavioursAtRuntime: true })
            {
                _objectBehaviours.AddRange(BehavioursCollection.AddBehaviours(this));
                VarwinObjectDescriptor.Components.SetupRuntimeBehaviours(RootGameObject);
            }
        }
        
        /// <summary>
        /// Добавление компонентов, необходимых для корректной работы объекта.
        /// </summary>
        private void AddRequiredComponents()
        {
            var behaviours = RootGameObject.GetComponentsInChildren<MonoBehaviour>(true);
            bool haveRootInputControl = false;

            foreach (var behaviour in behaviours)
            {
                if (!behaviour)
                {
                    continue;
                }

                var haveInputControlInObject = false;
                var idComponent = behaviour.gameObject.GetComponent<ObjectId>();

                if (idComponent)
                {
                    AddInputControl(behaviour, idComponent, out haveInputControlInObject);
                }
                
                if (haveInputControlInObject)
                {
                    haveRootInputControl = true;
                }

                AddGameModeSwitchControls(behaviour);
                AddCollidersAware(behaviour);
                AddPlatformModeSwitchControls(behaviour);
            }

            if (!haveRootInputControl)
            {
                _inputControllers.Add(-1, new InputController(this, RootGameObject, true));
            }
        }

        public void AddWrapper()
        {
            Wrapper wrapper;

            if (IsSceneTemplateObject)
            {
                wrapper = new NullWrapper(RootGameObject);
            }
            else
            {
                IWrapperAware wrapperAware = RootGameObject.GetComponentInChildren<IWrapperAware>();

                if (wrapperAware == null)
                {
                    return;
                }

                wrapper = wrapperAware.Wrapper();
            }

            WrappersCollection.Add(Id, wrapper);
            wrapper.InitObjectController(this);
        }

        private void AddCollidersAware(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<IColliderAware>())
            {
                return;
            }

            var colliderAware = child as IColliderAware;
            var colliderController = child.gameObject.AddComponent<ColliderController>();
            colliderController.SetColliderAware(colliderAware);
            _colliderControllers.Add(colliderController);
        }

        private void AddGameModeSwitchControls(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<ISwitchModeSubscriber>())
            {
                return;
            }

            var switchMode = (ISwitchModeSubscriber)child;
            _gameModeSwitchControllers.Add(new GameModeSwitchController(switchMode));
        }

        private void AddPlatformModeSwitchControls(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<ISwitchPlatformModeSubscriber>())
            {
                return;
            }

            var switchMode = (ISwitchPlatformModeSubscriber)child;
            _platformModeSwitchControllers.Add(new PlatformModeSwitchController(switchMode));
        }

        public void AddInputControl(MonoBehaviour behaviour, ObjectId idComponent, out bool haveInputControlInObject)
        {
            haveInputControlInObject = false;

            if (!behaviour.GetType().ImplementsInterface<IVarwinInputAware>())
            {
                return;
            }

            int id = idComponent.Id;
            GameObject go = behaviour.gameObject;
            bool root = go == RootGameObject;

            if (go == gameObject)
            {
                haveInputControlInObject = true;
            }

            if (!_inputControllers.ContainsKey(id))
            {
                _inputControllers.Add(id, new InputController(this, go, root));
            }
        }

        public IEnumerable<InputController> GetInputControllers()
        {
            return _inputControllers?.Values;
        }
        
        public Dictionary<int, InputController> GetInputControllersWithIds()
        {
            return _inputControllers;
        }
        
        public InputController GetInputController(int id)
        {
            return _inputControllers.TryGetValue(id, out var value) ? value : null;
        }


        #endregion

        #region Get

#if VARWIN_CLIENTCORE
        public static List<HierarchyController> GetRootObjectsInScene()
        {
            return GameStateData.GetRootObjectsScene().Select(x => x._hierarchyController).ToList();
        }
#endif

        public Dictionary<int, TransformDT> GetTransforms()
        {
            var transforms = new Dictionary<int, TransformDT>();

            foreach (InputController controllersValue in _inputControllers.Values)
            {
                if (!controllersValue.IsRoot)
                {
                    continue;
                }
                
                TransformDto transform = controllersValue.GetTransform();

                if (transform == null)
                {
                    continue;
                }

                if (!transforms.Keys.Contains(transform.Id))
                {
                    transforms.Add(transform.Id, transform.Transform);
                }
            }

            return transforms;
        }
        
        public ObjectController GetVirtualObject(int id)
        {
            return _virtualObjects.FirstOrDefault(virtualObject => virtualObject.IdObject == id);
        }

        public List<ObjectController> GetVirtualObjects()
        {
            return _virtualObjects;
        }

#if VARWIN_CLIENTCORE
        public SpawnInitParams GetSpawnInitParams()
        {
            var transforms = new Dictionary<int, TransformDT>();
            var inspectorPropertiesData = _componentsData.GetInspectorPropertiesData();
            var objectsIds = RootGameObject.GetComponentsInChildren<ObjectId>();

            foreach (ObjectId objectId in objectsIds)
            {
                if (transforms.ContainsKey(objectId.Id))
                {
                    continue;
                }

                TransformDT transformDt = objectId.gameObject == gameObject
                    ? GetAffectedTransform().ToTransformDT()
                    : objectId.gameObject.transform.ToTransformDT();

                transforms.Add(objectId.Id, transformDt);
            }

            var parent = Parent?.IsVirtualObject ?? false ? Parent.GetRootParent() : Parent;
            
            var spawn = new SpawnInitParams
            {
                ParentId = parent?.Id ?? 0,
                IdScene = ProjectData.SceneId,
                IdInstance = Id,
                IdObject = IdObject,
                IdServer = IdServer,
                RootTransform = gameObject.transform.ToTransformDT(),
                LocalTransform = ToLocalTransformDT(),
                Name = Name,
                Transforms = transforms,
                InspectorPropertiesData = inspectorPropertiesData,
                LockChildren = LockChildren,
                DisableSelectabilityInEditor = !SelectableInEditor,
                DisableSceneLogic = !EnableInLogicEditor,
                IsDisabled = !ActiveSelf,
                IsDisabledInHierarchy = !ActiveInHierarchy,
                Index = Index,
                VirtualObjectInfos = _virtualObjects?.Select(a => new VirtualObjectInfo
                {
                    Id = a.IdObject,
                    InstanceId = a.Id,
                    HierarchyExpandedState = a.HierarchyExpandedState,
                    LockChildren = a.LockChildren
                }).ToList(),
                VirtualObjectParentId = Parent?.IsVirtualObject ?? false ? Parent.IdObject : null
            };

            return spawn;
        }


        public List<SpawnInitParams> GetDescendedSpawnInitParams()
        {
            var result = new List<SpawnInitParams> { GetSpawnInitParams() };

            foreach (var child in Descendants)
            {
                if (child.IsVirtualObject)
                {
                    continue;
                }
                
                var spawnParams = child.GetSpawnInitParams();
                spawnParams.SpawnedByHierarchy = true;
                result.Add(spawnParams);
            }

            return result;
        }
#endif
        public string GetLocalizedName()
        {
            if (_localizedNames == null)
            {
                return "name unknown";
            }

#if VARWIN_CLIENTCORE
            return Settings.Instance.Language switch
            {
                "ru" => !string.IsNullOrEmpty(_localizedNames.ru) ? _localizedNames.ru : _localizedNames.en,
                "cn" => !string.IsNullOrEmpty(_localizedNames.cn) ? _localizedNames.cn : _localizedNames.en,
                "ko" => !string.IsNullOrEmpty(_localizedNames.ko) ? _localizedNames.ko : _localizedNames.en,
                "kk" => !string.IsNullOrEmpty(_localizedNames.kk) ? _localizedNames.kk : _localizedNames.en,
                "en" => _localizedNames.en,
                _ => _localizedNames.en
            };
#else
            return Application.systemLanguage switch
            {
                SystemLanguage.Russian => !string.IsNullOrEmpty(_localizedNames.ru) ? _localizedNames.ru : _localizedNames.en,
                SystemLanguage.ChineseSimplified => !string.IsNullOrEmpty(_localizedNames.cn) ? _localizedNames.cn : _localizedNames.en,
                SystemLanguage.Korean => !string.IsNullOrEmpty(_localizedNames.ko) ? _localizedNames.ko : _localizedNames.en,
                SystemLanguage.English => _localizedNames.en,
                _ => _localizedNames?.en ?? "name unknown"
            };
#endif
        }

        public I18n GetLocalizedNames()
        {
            return _localizedNames ?? null;
        }

        public Transform GetAffectedTransform()
        {
#if VARWIN_CLIENTCORE
            return _hierarchyController.GetAffectedTransform();
#else
            return gameObject?.transform;
#endif
        }

        public void UpdateTransforms()
        {
#if VARWIN_CLIENTCORE
            if (ActiveInHierarchy)
            {
                return;
            }

            _hierarchyController.UpdateTransformManually();
#endif
        }

        #endregion

        #region Switch Mode

        public void ApplyGameMode(GameMode newMode, GameMode oldMode)
        {
            if (!gameObject)
            {
                return;
            }

            foreach (InputController controller in _inputControllers.Values)
            {
                controller.GameModeChanged(newMode);
            }

            if (newMode == GameMode.Edit)
            {
                SetKinematicsOn();
            }
            else if ((newMode == GameMode.Preview || newMode == GameMode.View) &&
                     (oldMode == GameMode.Edit || oldMode == GameMode.Undefined || oldMode == newMode))
            {
                PhysicsBehaviour physicsBehaviour = null;
                if (ActiveInHierarchy && gameObject.TryGetComponent(out physicsBehaviour))
                {
                    physicsBehaviour.TrySetPhysicsSettings();
                }

#if VARWIN_CLIENTCORE
                _hierarchyController.UpdateConstraintsForPlayMode();
#endif

                if (physicsBehaviour)
                {
                    SaveKinematics();
                }

                SetKinematicsDefaults();

                UpdateActiveStatusParenting();
            }
        }

        public void ApplyPlatformMode(PlatformMode newMode, PlatformMode oldMode)
        {
            InitCollaboration();

            foreach (var controller in _inputControllers.Values)
            {
                controller.PlatformModeChanged(newMode);
            }
        }

        public void ExecuteSwitchGameModeOnObject(GameMode newMode, GameMode oldMode)
        {
            if (!gameObject || !gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (var gameModeSwitchController in _gameModeSwitchControllers)
            {
                try
                {
                    gameModeSwitchController.SwitchGameMode(newMode, oldMode);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Can not apply game mode for {Name}.\n{e}");
                }
            }
        }

        public void ExecuteSwitchPlatformModeOnObject(PlatformMode newMode, PlatformMode oldMode)
        {
            if (!gameObject || !gameObject.activeInHierarchy)
            {
                return;
            }

            foreach (var platformModeSwitchController in _platformModeSwitchControllers)
            {
                try
                {
                    platformModeSwitchController.SwitchPlatformMode(newMode, oldMode);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Can not apply platform mode for {Name}.\n{e}");
                }
            }
        }

        #endregion

        public void SaveKinematics()
        {
            var rigidBodies = RootGameObject.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rigidbody in rigidBodies)
            {
                if (!_rigidBodyKinematicsDefaults.ContainsKey(rigidbody.transform))
                {
                    _rigidBodyKinematicsDefaults.Add(rigidbody.transform, rigidbody.isKinematic);
                }
                else
                {
                    _rigidBodyKinematicsDefaults[rigidbody.transform] = rigidbody.isKinematic;
                }
            }
        }

        public void SetKinematicsDefaults()
        {
            foreach (var rigidbody in _rigidBodyKinematicsDefaults)
            {
                rigidbody.Key.GetComponent<Rigidbody>().isKinematic = rigidbody.Value;
            }

            foreach (Rigidbody rigidbody in _rigidbodies)
            {
                rigidbody.useGravity = true;
            }

            _rigidbodies.Clear();
        }

        public void SetKinematicsOn()
        {
            foreach (Transform transform in _rigidBodyKinematicsDefaults.Keys)
            {
                if (!transform)
                {
                    Debug.LogError("Transform was lost");
                    continue;
                }

                Rigidbody body = transform.GetComponent<Rigidbody>();

                if (!body)
                {
                    continue;
                }

                body.isKinematic = true;
            }
        }

        public void CopyKinematicsFrom(Rigidbody rigidbodyToCopy)
        {
            foreach (Transform transform in _rigidBodyKinematicsDefaults.Keys)
            {
                if (!transform)
                {
                    Debug.LogError("Transform was lost");
                    continue;
                }

                Rigidbody body = transform.GetComponent<Rigidbody>();

                if (!body)
                {
                    continue;
                }

                body.isKinematic = rigidbodyToCopy.isKinematic;
                body.useGravity = rigidbodyToCopy.useGravity;
            }

            SaveKinematics();
        }
        
        public bool TryDestroyInputControl(MonoBehaviour behaviour, ObjectId idComponent)
        {
            if (!behaviour.GetType().ImplementsInterface<IVarwinInputAware>())
            {
                return false;
            }

            int id = idComponent.Id;
            GameObject go = behaviour.gameObject;
            bool root = go == RootGameObject;

            if (!_inputControllers.ContainsKey(id))
            {
                return false;
            }

            var inputController = _inputControllers[id];
            inputController.Destroy();
            _inputControllers.Remove(id);
            return true;
        }
        
        public void Delete()
        {
            if (IsDeleted)
            {
                return;
            }

            IsDeleted = true;
            
            this.OnDeletingObject();

            RemoveParent();

            WrappersCollection.Remove(Id);
            
#if VARWIN_CLIENTCORE
            _hierarchyController.DestroyGhostObject();
#endif

            Object.Destroy(RootGameObject);

            foreach (ColliderController colliderController in _colliderControllers)
            {
                Object.Destroy(colliderController);
            }
            
            _colliderControllers.Clear();
            _colliderControllers = null;

            foreach (InputController inputController in _inputControllers.Values)
            {
                inputController.Destroy();
            }
            _inputControllers.Clear();
            _inputControllers = null;

            _rigidBodyKinematicsDefaults.Clear();
            _rigidBodyKinematicsDefaults = null;

            _gameModeSwitchControllers.Clear();
            _gameModeSwitchControllers = null;

            _platformModeSwitchControllers.Clear();
            _platformModeSwitchControllers = null;

            _rigidbodies.Clear();
            _rigidbodies = null;

#if VARWIN_CLIENTCORE
            _hierarchyController.Destroy();
            _hierarchyController = null;
            
            _componentsData.Dispose();
            _componentsData = null;
#endif
            
            _virtualObjects.Clear();

            this.Dispose();
        }

        #region Inspector Property

        public void OnPropertyValueChanged(string componentPropertyName, object value)
        {
            PropertyValueChanged?.Invoke(componentPropertyName, value);
        }

        public List<string> GetObjectBehaviours()
        {
#if VARWIN_CLIENTCORE
            return _objectBehaviours;
#endif
            return null;
        }

        public List<ResourceDto> GetUsingResourcesData()
        {
#if VARWIN_CLIENTCORE
            return _componentsData.GetUsingResources();
#endif
            return null;
        }

        public bool IsResourceProperty(string propertyName)
        {
#if VARWIN_CLIENTCORE
            return _componentsData.IsResourceField(propertyName);
#endif
            return false;
        }

        public List<string> GetOnDemandedResourceGuids()
        {
#if VARWIN_CLIENTCORE
            return _componentsData.GetOnDemandedResourcesGuid();
#endif
            return null;
        }

#if VARWIN_CLIENTCORE
        public List<InspectorPropertyData> GetInspectorPropertiesData()
        {
            return _componentsData.GetInspectorPropertiesData();
        }
#else
        public List<object> GetInspectorPropertiesData() => null;
#endif

        // ReSharper disable once UnusedMember.Global
        public void SetSerializablePropertyValue(string propertyName, object value)
        {
#if VARWIN_CLIENTCORE
            if (string.IsNullOrEmpty(propertyName))
            {
                return;
            }
            _componentsData.SetPropertyFromInspector(propertyName, value);
            OnPropertyValueChanged(propertyName, value);
#endif
        }

#if VARWIN_CLIENTCORE

        public void SetInspectorPropertyValue(string propertyName, object value, bool isResource, bool isArrayMember = false, int index = -1)
        {
            if (isArrayMember)
            {
                _componentsData.SetArrayFromInspector(propertyName, value, index);
            }
            else
            {
                _componentsData.SetPropertyFromInspector(propertyName, value);
            }

            OnPropertyValueChanged(propertyName, GetRawValue(propertyName));
        }

        /// <inheritdoc cref="Varwin.ComponentsData.SetResourceProperty"/>
        public void SetResource(string propertyName, string guid)
        {
            _componentsData.SetResourceProperty(propertyName, guid);
            OnPropertyValueChanged(propertyName, guid);
        }

        public void SetArrayFromInspector(string propertyName, object value, int index)
        {
            _componentsData.SetArrayFromInspector(propertyName, value, index);
            OnPropertyValueChanged(propertyName, GetRawValue(propertyName));
        }

        ///<inheritdoc cref="Varwin.ComponentsData.AddItemToCollection"/>
        public IEnumerable AddItemToCollection(string propertyName, object value = null, int index = -1)
        {
            var collection = _componentsData.AddItemToCollection(propertyName, value, index);
            OnPropertyValueChanged(propertyName, collection);

            return collection;
        }

        ///<inheritdoc cref="ComponentsData.RemoveItemFromCollection"/>
        public IEnumerable RemoveItemFromCollection(string propertyName, int index)
        {
            var collection = _componentsData.RemoveItemFromCollection(propertyName, index);
            OnPropertyValueChanged(propertyName, collection);

            return collection;
        }

        public int GetCollectionSize(string propertyName)
        {
            return _componentsData.GetCollectionSize(propertyName);
        }
#endif

        #endregion

        public void OnEditorSelect()
        {
            IsSelectedInEditor = true;
        }

        public void OnEditorUnselect()
        {
            IsSelectedInEditor = false;
        }

        [Obsolete]
        public void RefreshInspector() => RefreshInspector(false);

        [Obsolete]
        public void RefreshInspector(bool force = false)
        {
#if VARWIN_CLIENTCORE
            InspectorRefreshed?.Invoke();
#endif
        }

        #region Hierarchy Controller Wrappers

        public void SetParent(ObjectController parent, int index, bool keepOriginalScale = false)
        {
#if VARWIN_CLIENTCORE
            _hierarchyController.SetParent(parent?._hierarchyController, index, keepOriginalScale);
#endif
        }

        public void SetParent(ObjectController parent, bool keepOriginalScale = false)
        {
            if (parent == null)
            {
                SetParent(null, 0, keepOriginalScale);
            }
            else
            {
                SetParent(parent, parent.Children.Count - 1, keepOriginalScale);    
            }
        }

        public void RemoveParent()
        {
#if VARWIN_CLIENTCORE
            _hierarchyController.RemoveParent();
#endif
        }

        public void OnGrabStart()
        {
#if VARWIN_CLIENTCORE
            _hierarchyController.OnGrabStart();
#endif
        }

        public void OnGrabEnd()
        {
#if VARWIN_CLIENTCORE
            _hierarchyController.OnGrabEnd();
#endif
        }

        public ObjectController GetRootParent()
        {
#if VARWIN_CLIENTCORE
            return _hierarchyController.GetRootParent().ObjectController;
#endif
            return null;
        }

        public void InvokeParentChangedEvent()
        {
            ParentChanged?.Invoke();
        }

        #endregion

        public static implicit operator bool(ObjectController objectController) => objectController != null;

        public override string ToString()
        {
            return $"ObjectController ({Name})";
        }

        public TransformDT ToLocalTransformDT()
        {
            TransformDT transformDT = new();
            transformDT.PositionDT = LocalPosition;
            transformDT.EulerAnglesDT = LocalEulerAngles;
            transformDT.ScaleDT = LocalScale;
            return transformDT;
        }

        public object GetPropertyValue(string componentPropertyName)
        {
#if VARWIN_CLIENTCORE
            return _componentsData.GetValueFor(componentPropertyName);
#endif
            
            return null;
        }

        public object GetRawValue(string componentPropertyName)
        {
#if VARWIN_CLIENTCORE
            return _componentsData.GetRawValueFor(componentPropertyName);
#endif
            return null;
        }

        public object GetArrayRawValue(string componentPropertyName, int index)
        {
#if VARWIN_CLIENTCORE
            return _componentsData.GetArrayRawValue(componentPropertyName, index);
#endif
            return null;
        }
    }
}
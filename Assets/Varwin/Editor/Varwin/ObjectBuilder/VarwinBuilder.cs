using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Data;
using Varwin.Public;

namespace Varwin.Editor
{
    public class VarwinBuilder : IJsonSerializable
    {
        public enum BuilderType
        {
            Default,
            LogicOnly
        }

        public const string TempStateFilename = "ObjectBuilding.json";
        public const bool DeleteTempStateFile = false;
        public const bool DeleteTempFolder = true;
        public const bool DeleteWrappers = true;

        public List<VarwinPackageInfo> PackageInfos;
        public List<ObjectBuildDescription> ObjectsToBuild;
        public int CurrentBuildingSceneIndex;
        public List<SceneTemplateBuildDescription> ScenesToBuild;
        public List<ResourceBuildDescription> ResourcesToBuild;
        public virtual BuilderType BuildType { get; set; } = BuilderType.Default;

        protected Queue<IBuildingState> States;

        private float _displayProgress;
        private IBuildingState _currentState;
        [JsonProperty("CompletedStateCount")] private int _completedStateCount;
        [JsonProperty("IsStatesInitialized")] private bool _isStatesInitialized = false;

        public VarwinBuildingData Data { get; } = new VarwinBuildingData();

        public float Progress
        {
            get
            {
                if (StateCount == 0)
                {
                    return 0;
                }

                if (IsFinished)
                {
                    return 1f;
                }
                
                if (_currentState != null)
                {
                    return Mathf.Clamp01((CompletedStateCount + _currentState.Progress) / StateCount);
                }

                return Mathf.Clamp01((float)CompletedStateCount / StateCount);
            }
        }

        public string Label => _currentState?.Label ?? SdkTexts.CompilingScriptsStep;

        public float DisplayProgress
        {
            get => ObjectsToBuild.Count < 10 ? _displayProgress : Progress;
            set => _displayProgress = value;
        }
        
        public bool IsFinished { get; private set; }
        public bool IsStopped { get; private set; }
        public int StateCount { get; protected set; }
        [JsonIgnore] public int CompletedStateCount => _completedStateCount;

        public void Initialize()
        {
            InitializeStates();
            IsFinished = false;
            IsStopped = false;
            VarwinBuildStepNotifier.UpdateBuildObjects(ObjectsToBuild);
            Serialize();
        }

        protected virtual void InitializeStates()
        { 
            States = new();

            States.Enqueue(new ObjectsBuildValidationState(this));
            if (SdkSettings.Features.Changelog && !Application.isBatchMode)
            {
                States.Enqueue(new EditChangelogState(this));
            }

            if (ObjectsToBuild != null)
            {
                States.Enqueue(new PreparationState(this));

                if (SdkSettings.Features.DynamicVersioning)
                {
                    States.Enqueue(new AsmdefReferencesCollectingState(this));
                    States.Enqueue(new RenameAssembliesToNewNamesState(this));
                }

                States.Enqueue(new WrapperGenerationState(this));
                States.Enqueue(new IconGenerationState(this));
                States.Enqueue(new PreviewGenerationState(this));
                States.Enqueue(new AssembliesCollectingState(this));

                if (SdkSettings.Features.DynamicVersioning)
                {
                    States.Enqueue(new SetVersionSuffixToDescriptorState(this));
                }

                States.Enqueue(new InstallJsonGenerationState(this));
                States.Enqueue(new BundleJsonGenerationState(this));
                States.Enqueue(new AssetBundleBuildingState(this));
                States.Enqueue(new SourcePackagingState(this));
                States.Enqueue(new ZippingFilesState(this));
            }

            if (ScenesToBuild != null)
            {
                States.Enqueue(new SceneTemplateBuildingState(this));
            }
            
            if (PackageInfos != null)
            {
                States.Enqueue(new PackageCreationState(this));
            }

            if (ObjectsToBuild != null)
            {
                if (SdkSettings.Features.DynamicVersioning)
                {
                    States.Enqueue(new RenameAssembliesToOldNamesState(this));
                }
            }

            if (!_isStatesInitialized)
            {
                foreach (var state in States)
                {
                    state.Initialize();
                }

                _isStatesInitialized = true;
            }
            
            StateCount = States.Count;
        }

        public void Update()
        {
            DisplayProgress = Mathf.Lerp(DisplayProgress, Progress, 0.1f);

            if (Mathf.Abs(DisplayProgress - Progress) > 0.01f)
            {
                return;
            }
            
            if (IsFinished)
            {
                return;
            }
            
            _currentState?.Update();

            if (_currentState == null || _currentState.IsFinished)
            {
                if (States != null && States.Count > 0)
                {
                    if (_currentState != null)
                    {
                        _completedStateCount++;
                    }
                    
                    _currentState = States.Dequeue();
                }
                else
                {
                    IsFinished = true;
                }
            }
        }
        
        public void Build(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            Build(new [] {varwinObjectDescriptor});
        }
        
        public void Build(IEnumerable<VarwinObjectDescriptor> varwinObjectDescriptors)
        {
            PackageInfos = null;
            Build(new VarwinBuildDescriptor
            {
                Objects = varwinObjectDescriptors.ToArray()
            });
        }

        public void Build(VarwinPackageDescriptor varwinPackageDescriptor)
        {
            Build(new List<VarwinPackageDescriptor>(){ varwinPackageDescriptor });
        }

        public void Build(IEnumerable<VarwinPackageDescriptor> varwinPackageDescriptors)
        {
            PackageInfos = new List<VarwinPackageInfo>();
            var varwinObjectDescriptors = new List<VarwinObjectDescriptor>();
            var varwinSceneTemplates = new List<string>();
            var varwinResources = new List<VarwinResource>();
            
            foreach (var varwinPackageDescriptor in varwinPackageDescriptors)
            {
                varwinPackageDescriptor.Guid = Guid.NewGuid().ToString();
            
                if (varwinPackageDescriptor.Objects != null)
                {
                    foreach (var gameObject in varwinPackageDescriptor.Objects)
                    {
                        var varwinObjectDescriptor = gameObject.GetComponent<VarwinObjectDescriptor>();
                        if (varwinPackageDescriptor)
                        {
                            varwinObjectDescriptors.Add(varwinObjectDescriptor);
                        }
                    }
                }

                if (varwinPackageDescriptor.SceneTemplates != null)
                {
                    foreach (var varwinSceneTemplate in varwinPackageDescriptor.SceneTemplates)
                    {
                        var scenePath = AssetDatabase.GetAssetPath(varwinSceneTemplate.Value);
                        if (!string.IsNullOrEmpty(scenePath))
                        {
                            varwinSceneTemplates.Add(scenePath);
                        }
                    }
                }
                
                if (varwinPackageDescriptor.Resources != null)
                {
                    foreach (var varwinResource in varwinPackageDescriptor.Resources)
                    {
                        if (varwinResource)
                        {
                            varwinResources.Add(varwinResource);
                        }
                    }
                }

                PackageInfos.Add(new VarwinPackageInfo(varwinPackageDescriptor));
            }

            var descriptor = new VarwinBuildDescriptor
            {
                Objects = varwinObjectDescriptors.ToArray(),
                Scenes = varwinSceneTemplates.ToArray(),
                Resources = varwinResources.ToArray()
            };

            Build(descriptor);
        }
        
        public void Skip()
        {
            if (States is { Count: > 0 })
            {
                _currentState = States.Dequeue();
            }
        }

        public void Stop()
        {
            IsStopped = true;
        }

        public static VarwinBuilder Deserialize()
        {
            if (!File.Exists(TempStateFilename))
            {
                return null;
            }
            
            string json = File.ReadAllText(TempStateFilename);
            var objectBuilder = JsonConvert.DeserializeObject<VarwinBuilder>(json);
            if (objectBuilder.BuildType == BuilderType.LogicOnly)
                objectBuilder = JsonConvert.DeserializeObject<VarwinLogicOnlyBuilder>(json);
            
            objectBuilder.Initialize();
            
            objectBuilder.Skip();
            for (int i = 0; i < objectBuilder.CompletedStateCount; ++i)
            {
                objectBuilder.Skip();
            }

            objectBuilder.DisplayProgress = objectBuilder.Progress;

            return objectBuilder;
        }

        public virtual void Serialize()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            string jsonModels = JsonConvert.SerializeObject(this, DeleteTempStateFile ? Formatting.None : Formatting.Indented, jsonSerializerSettings);

            File.WriteAllText(TempStateFilename, jsonModels);
        }

        private void Build(VarwinBuildDescriptor descriptor)
        {
            try
            {
                if (descriptor.Objects != null)
                {
                    ObjectsToBuild = new List<ObjectBuildDescription>();
                    
                    foreach (VarwinObjectDescriptor varwinObjectDescriptor in descriptor.Objects)
                    {
                        if (!varwinObjectDescriptor)
                        {
                            continue;
                        }

                        VarwinObjectDescriptor currentVarwinObjectDescriptor = varwinObjectDescriptor;

                        if (ObjectsToBuild.Any(x => x.ObjectGuid == currentVarwinObjectDescriptor.RootGuid))
                        {
                            continue;
                        }

                        GameObject prefab = CreateObjectUtils.GetPrefabObject(currentVarwinObjectDescriptor.gameObject);
                        string varwinObjectPath = CreateObjectUtils.GetPrefabPath(prefab ? prefab : currentVarwinObjectDescriptor.gameObject);

                        if (!CreateObjectUtils.IsPrefabAsset(currentVarwinObjectDescriptor.gameObject) && prefab)
                        {
                            PrefabUtility.ApplyPrefabInstance(currentVarwinObjectDescriptor.gameObject, InteractionMode.AutomatedAction);
                        }

                        CreateObjectUtils.SetupComponentReferences(currentVarwinObjectDescriptor);

                        VarwinObjectDescriptor prefabVarwinObjectDescriptor = prefab ? prefab.GetComponent<VarwinObjectDescriptor>() : currentVarwinObjectDescriptor;

                        if (prefab)
                        {
                            GameObject tempPrefab = PrefabUtility.LoadPrefabContents(varwinObjectPath);
                            var tempPrefabVarwinObjectDescriptor = tempPrefab.GetComponent<VarwinObjectDescriptor>();
                            CreateObjectUtils.SetupComponentReferences(tempPrefabVarwinObjectDescriptor);
                            CreateObjectUtils.SetupObjectIds(tempPrefab);
                            prefab = PrefabUtility.SaveAsPrefabAsset(tempPrefab, varwinObjectPath);

                            if (!currentVarwinObjectDescriptor)
                            {
                                currentVarwinObjectDescriptor = prefab.GetComponent<VarwinObjectDescriptor>();
                                prefabVarwinObjectDescriptor = currentVarwinObjectDescriptor;
                            }

                            if (!CreateObjectUtils.IsPrefabAsset(currentVarwinObjectDescriptor.gameObject))
                            {
                                CreateObjectUtils.ApplyPrefabInstanceChanges(currentVarwinObjectDescriptor.gameObject);
                            }

                            var prefabIsChanged = false;

                            if (!string.Equals(prefabVarwinObjectDescriptor.Prefab, varwinObjectPath, StringComparison.Ordinal))
                            {
                                prefabVarwinObjectDescriptor.Prefab = varwinObjectPath;
                                prefabIsChanged = true;
                            }

                            string prefabGuid = AssetDatabase.AssetPathToGUID(varwinObjectPath);
                            if (!string.Equals(prefabVarwinObjectDescriptor.PrefabGuid, prefabGuid, StringComparison.Ordinal))
                            {
                                prefabVarwinObjectDescriptor.PrefabGuid = prefabGuid;
                                prefabIsChanged = true;
                            }

                            if (prefabIsChanged)
                            {
                                CreateObjectUtils.ApplyPrefabInstanceChanges(prefabVarwinObjectDescriptor.gameObject);
                            }
                        }
                        else
                        {
                            CreateObjectUtils.SetupObjectIds(currentVarwinObjectDescriptor.gameObject);
                        }

                        string folderPath = null;
                        if (!string.IsNullOrEmpty(varwinObjectPath))
                        {
                            folderPath = Path.GetDirectoryName(varwinObjectPath)?.Replace("\\", "/");
                        }

                        var objectBuildDescription = new ObjectBuildDescription()
                        {
                            ObjectName = prefabVarwinObjectDescriptor.Name,
                            ObjectGuid = prefabVarwinObjectDescriptor.RootGuid,
                            PrefabPath = varwinObjectPath,
                            FolderPath = folderPath,
                            ContainedObjectDescriptor = prefabVarwinObjectDescriptor
                        };

                        if (CreateObjectUtils.CheckNeedBuildWithVarwinTemp(currentVarwinObjectDescriptor))
                        {
                            string folder = $"Assets/VarwinTemp/{prefabVarwinObjectDescriptor.Name}";
                            if (!Directory.Exists(folder))
                            {
                                Directory.CreateDirectory(folder);
                            }

                            string prefabPath = $"{folder}/{prefabVarwinObjectDescriptor.Name}.prefab";

                            GameObject prefabTempInstance;
                            if (prefab)
                            {
                                prefabTempInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                            }
                            else
                            {
                                prefabTempInstance = UnityEngine.Object.Instantiate(currentVarwinObjectDescriptor.gameObject);
                            }

                            if (!prefabTempInstance)
                            {
                                Debug.LogError($"Can't load prefab at path {prefabPath}. Try to build an object via prefab instance on the scene");
                                continue;
                            }

                            if (!prefabTempInstance.GetComponent<VarwinObject>())
                            {
                                prefabTempInstance.AddComponent<VarwinObject>();
                            }

                            prefab = PrefabUtility.SaveAsPrefabAsset(prefabTempInstance, prefabPath);
                            prefabVarwinObjectDescriptor = prefab.GetComponent<VarwinObjectDescriptor>();
                            UnityEngine.Object.DestroyImmediate(prefabTempInstance);

                            prefabVarwinObjectDescriptor.Prefab = prefabPath;
                            prefabVarwinObjectDescriptor.PrefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);

                            objectBuildDescription.ObjectName = prefabVarwinObjectDescriptor.Name;
                            objectBuildDescription.ObjectGuid = prefabVarwinObjectDescriptor.RootGuid;
                            objectBuildDescription.ContainedObjectDescriptor = prefabVarwinObjectDescriptor;
                            objectBuildDescription.PrefabPath = prefabPath;
                            objectBuildDescription.FolderPath = folder;

                            var asmdefData = new AssemblyDefinitionData
                            {
                                name = prefabVarwinObjectDescriptor.Namespace,
                                references = new[] { "VarwinCore" }
                            };

                            asmdefData.Save($"{folder}/{prefabVarwinObjectDescriptor.Name}.asmdef");

                            CreateObjectUtils.SetupAsmdef(prefabVarwinObjectDescriptor);
                        }

                        if (!prefab.GetComponent<VarwinObject>())
                        {
                            prefab.AddComponent<VarwinObject>();
                        }

                        ObjectsToBuild.Add(objectBuildDescription);
                    }
                }

                if (descriptor.Scenes != null)
                {
                    ScenesToBuild = new List<SceneTemplateBuildDescription>();
                    
                    foreach (var scene in descriptor.Scenes)
                    {
                        var sceneDescriptor = new SceneTemplateBuildDescription
                        {
                            Path = scene
                        };

                        ScenesToBuild.Add(sceneDescriptor);
                    }
                }
                
                if (descriptor.Resources != null)
                {
                    ResourcesToBuild = new List<ResourceBuildDescription>();
                    
                    foreach (var resource in descriptor.Resources)
                    {
                        var resourceBuildDescription = new ResourceBuildDescription
                        {
                            Name = resource.name,
                            ResourcePath = AssetDatabase.GetAssetPath(resource),
                        };

                        ResourcesToBuild.Add(resourceBuildDescription);
                    }
                }

                if (SdkSettings.Settings.MobileReady)
                {
                    if (ObjectsToBuild.Any(x => x.ContainedObjectDescriptor.MobileReady) && !AndroidPlatformHelper.IsAndroidPlatformInstalled)
                    {
                        throw new Exception("Android platform not installed. Please install it before building mobile ready object.");
                    }
                }

                Initialize();
            }
            catch (Exception e)
            {
                var message = string.Format(SdkTexts.ProblemWhenRunBuildAllObjectsFormat, e.Message);
                EditorUtility.DisplayDialog("Error!", message, "OK");
                Debug.LogException(e);
                
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }
    }
}
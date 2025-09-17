using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Varwin.Core;
using Varwin.Core.Behaviours;
using Varwin.Data;
using Varwin.Data.ServerData;
using Varwin.Public;
using Logger = Varwin.Core.Logger;
using Object = UnityEngine.Object;
#if VARWINCLIENT

using Request3dModel = Varwin.WWW.Request3dModel;
using Varwin.Types;

#endif

namespace Varwin
{
    public static class GameStateData
    {
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        
#if VARWINCLIENT        
        private static readonly List<ObjectInfo> ObjectPrefabs = new();
        private static readonly List<SceneInfo> SceneTemplatePrefabs = new();
#endif

        private static readonly Dictionary<ObjectController, int> ObjectsIds = new Dictionary<ObjectController, int>();
        private static readonly Dictionary<int, Sprite> ObjectIcons = new Dictionary<int, Sprite>();

        private static readonly List<ResourceObject> Resources = new List<ResourceObject>();

        private static readonly List<int> EmbeddedObjects = new List<int>();
        private static readonly WrappersCollection WrappersCollection = new WrappersCollection();
#if VARWINCLIENT
        private static LogicInstance _logicInstance = null;
#endif
        private static PrefabObject _playerObject;
        private static List<int> _selectedObjectsIds = new List<int>();
        private static HashSet<int> _sceneObjectLockedIds = new HashSet<int>();

        public static HashSet<Hash128> LoadedAssetBundleParts = new HashSet<Hash128>();
        public static readonly HashSet<string> LoadedResources = new HashSet<string>();

        /// <summary>
        /// Список Id отсутствующий в RMS и заспавненных на активной сцене объектов.
        /// </summary>
        public static HashSet<int> MissingInRmsSpawnedObjects { get; } = new();

        /// <summary>
        /// Количество объектов, использующих отсутствующие (в библиотеке RMS) ресурсы.   
        /// </summary>
        public static int CountOfObjectsUsesMissingResources { get; set;  }

        /// <summary>
        /// Флаг, показывающих наличие в сцене отсутствующих (в библиотеке RMS) объектов или ресурсов.
        /// </summary>
        public static bool IsSceneContainsMissingResourcesOrObjects => MissingInRmsSpawnedObjects.Count > 0 || CountOfObjectsUsesMissingResources > 0;

        /// <summary>
        /// Список guid'ов ресурсов, которые были удалены на протяжении работы со сценой.
        /// </summary>
        public static HashSet<string> MissingResourcesGuids { get; } = new();

        public delegate void ObjectRenameHandler(Wrapper wrapper, string oldName, string newName);

        public static event ObjectRenameHandler OnObjectWasRenamed;

        public static void ClearAllData()
        {
            ClearLogic();
            
            ObjectsIds.Clear();
            
            ClearObjectIcons();
            ClearResources();
            
            UnloadAllObjectsPrefabs();
            UnloadAllScenePrefabs(true);
            
            EmbeddedObjects.Clear();
            WrappersCollection.Clear();
            _selectedObjectsIds.Clear();
            _sceneObjectLockedIds.Clear();

            LoadedAssetBundleParts.Clear();
            LoadedResources.Clear();

            MissingInRmsSpawnedObjects.Clear();
            MissingResourcesGuids.Clear();
            CountOfObjectsUsesMissingResources = 0;
            VarwinBehaviourHelper.ClearCache();
        }

        public static void ClearSceneData()
        {
            ClearLogic();
            
            ObjectsIds.Clear();
            
            ClearObjectIcons();
            ClearResources();
            UnloadAllObjectsPrefabs();
            UnloadAllScenePrefabs();
            
            EmbeddedObjects.Clear();
            WrappersCollection.Clear();
            _selectedObjectsIds.Clear();
            _sceneObjectLockedIds.Clear();

            LoadedAssetBundleParts.Clear();
            LoadedResources.Clear();

            MissingInRmsSpawnedObjects.Clear();
            MissingResourcesGuids.Clear();
            CountOfObjectsUsesMissingResources = 0;
        }
        
        private static void UnloadAllObjectsPrefabs()
        {
#if VARWINCLIENT            
            foreach (var objectInfo in ObjectPrefabs)
            {
                objectInfo.AssetContainer.Unload();
            }
            
            ObjectPrefabs.Clear();
#endif
        }
        
        private static void UnloadAllScenePrefabs(bool includeLoadingScenes = false)
        {
#if VARWINCLIENT            
            for (var index = SceneTemplatePrefabs.Count - 1; index >= 0; index--)
            {
                var sceneInfo = SceneTemplatePrefabs[index];
                if (sceneInfo.IsLoadingScene && !includeLoadingScenes)
                {
                    continue;
                }

                sceneInfo.AssetContainer.Unload();
                SceneTemplatePrefabs.RemoveAt(index);
            }
#endif
        }

        private static void ClearObjectIcons()
        {
            foreach (var icon in ObjectIcons)
            {
                if (icon.Value)
                {
                    Object.Destroy(icon.Value);
                }
            }

            ObjectIcons.Clear();
        }

        private static void ClearResources()
        {
            foreach (var resource in Resources)
            {
                if (resource.Value is Object resourceValue)
                {
                    Object.Destroy(resourceValue);
                }

                resource.Value = null;
            }
            
            Resources.Clear();
#if VARWINCLIENT
            Request3dModelResource.ClearCache();
#endif
        }

        public static bool UnloadResource(ResourceDto resourceDto)
        {
            if (resourceDto == null)
            {
                Logger.LogWarning("Can't unload null reference resource. Skipping...");
                return false;
            }
            
            var resourceObject = Resources.FirstOrDefault(a => a.Data.Guid == resourceDto.Guid);
            if (resourceObject == null)
            {
                Logger.LogWarning($"Can't find resource {resourceDto.GetLocalizedName()}. Skipping...");
                return false;
            }

            if (resourceObject.Value is Object resourceValue)
            {
                Object.Destroy(resourceValue);
            }

            resourceObject.Value = null;
            return true;
        }

        public static void OnDeletingObject(this ObjectController self)
        {
            ProjectData.OnDeletingObject(self);
        }

        public static void Dispose(this ObjectController self)
        {
            if (ObjectsIds.ContainsKey(self))
            {
                ObjectsIds.Remove(self);
            }

#if VARWINCLIENT
            GCManager.Collect();
#endif
        }

        public static void SelectObjects(List<int> newSelection)
        {
            var unselectedObjects = new List<ObjectController>();
            var newSelectedObjects = new List<ObjectController>();

            foreach (int objectId in _selectedObjectsIds)
            {
                if (newSelection.Contains(objectId))
                {
                    continue;
                }

                ObjectController objectController = GetObjectControllerInSceneById(objectId);
                if (objectController == null || objectController.Parent != null && objectController.Parent.IsSelectedInEditor)
                {
                    continue;
                }

                unselectedObjects.Add(objectController);
            }

            foreach (int objectId in newSelection)
            {
                if (!_selectedObjectsIds.Contains(objectId))
                {
                    newSelectedObjects.Add(GetObjectControllerInSceneById(objectId));
                }
            }

            foreach (ObjectController controller in unselectedObjects)
            {
                controller?.OnEditorUnselect();
            }

            foreach (ObjectController controller in newSelectedObjects)
            {
                controller?.OnEditorSelect();
            }

            _selectedObjectsIds = newSelection;
        }

        public static void RegisterMeInScene(this ObjectController self, ref int instanceId, string desiredName)
        {
            if (instanceId == 0)
            {
                int newId;

                if (ObjectsIds.Count == 0)
                {
                    newId = 1;
                }
                else
                {
                    newId = ObjectsIds.Values.ToList().Max() + 1;
                }

                instanceId = newId;

                if (string.IsNullOrEmpty(desiredName))
                {
                    desiredName = self.GetLocalizedName();
                }
            }

            RenameObject(self, desiredName);

            ObjectsIds.Add(self, instanceId);
        }

        public static IEnumerable<int> GetObjectIds()
        {
            return ObjectsIds.Values;
        }

        /// <summary>
        /// Переименование объекта
        /// </summary>
        /// <param name="self"></param>
        /// <param name="desiredName">Желаемое имя</param>
        /// <param name="uniqueName"> Должно ли имя быть уникальным на сцене</param>
        public static void RenameObject(this ObjectController self, string desiredName, bool uniqueName = true)
        {
            if (!ProjectData.IsPlayMode && uniqueName && !IsUniqueName(desiredName, self.ParentId))
            {
                desiredName = GetUniqueName(desiredName, self.ParentId);
            }

            var oldName = self.Name;

            self.SetName(desiredName);
            
            OnObjectWasRenamed?.Invoke(self.gameObject.GetWrapper(), oldName, desiredName);
        }

        private static bool IsUniqueName(string desiredName, int parentId)
        {
            var existingNames = ObjectsIds.Keys
                .Where(x => x.ParentId == parentId)
                .Select(x => x.Name)
                .Where(x => x.IndexOf(desiredName, StringComparison.InvariantCulture) == 0);

            return !existingNames.Contains(desiredName);
        }

        private static string GetUniqueName(string desiredName, int parentId)
        {
            const string regexPattern = @"\((\d*)\)$";
            var match = Regex.Match(desiredName, regexPattern, RegexOptions.IgnoreCase);
            if (match.Success && match.Value != string.Empty)
            {
                desiredName = desiredName.Replace(match.Value, string.Empty);
                desiredName = desiredName.Trim();
            }

            var existingNames = ObjectsIds.Keys
                .Where(x => x.ParentId == parentId)
                .Select(x => x.Name)
                .Where(x => x.IndexOf(desiredName, StringComparison.InvariantCulture) == 0);

            if (!existingNames.Any())
            {
                return desiredName;
            }

            var maxIndex = -1;
            foreach (var item in existingNames)
            {
                match = Regex.Match(item, regexPattern, RegexOptions.IgnoreCase);
                if (match.Success && match.Groups.Count > 1 && match.Groups[1].Value != string.Empty)
                {
                    if (int.TryParse(match.Groups[1].Value, out var index))
                    {
                        if (index > maxIndex)
                        {
                            maxIndex = index;
                        }
                    }
                }
            }

            return $"{desiredName} ({maxIndex + 1})";
        }

        public static ObjectController GetObjectControllerInSceneById(int idObject)
        {
            foreach (ObjectController value in ObjectsIds.Keys)
            {
                if (value.Id == idObject)
                {
                    return value;
                }
            }

            return null;
        }

        public static WrappersCollection GetWrapperCollection() => WrappersCollection;

        public static void ClearObjects()
        {
            var objectControllers = GetObjectsInScene();

            foreach (ObjectController objectController in objectControllers)
            {
                objectController.Delete();
            }

            ObjectsIds.Clear();
            Debug.Log("Scene objects were deleted");
        }

        public static int GetNextObjectIdInScene()
        {
            if (ObjectsIds.Count == 0)
            {
                return 1;
            }

            int newId = ObjectsIds.Values.ToList().Max() + 1;

            return newId;
        }

        public static List<ObjectController> GetObjectsInScene() => ObjectsIds.Keys.ToList();
        public static int SceneObjectsCount => ObjectsIds.Keys.Count;

        public static List<ObjectController> GetRootObjectsScene()
        {
            List<ObjectController> rootObjects = ObjectsIds.Keys.ToList().Where(objectController => objectController.Parent == null).ToList();
            rootObjects = rootObjects.OrderBy(controller => controller.Index).ToList();

            return rootObjects;
        }

#if VARWINCLIENT            
        public static void AddPrefabGameObject(PrefabObject prefabObject, AssetContainer assetContainer)
        {
            ObjectPrefabs.Add(new ObjectInfo(prefabObject, assetContainer));
        }

        public static void AddPrefabScene(SceneTemplatePrefab prefab, AssetContainer assetContainer, bool isLoadingScene)
        {
            SceneTemplatePrefabs.Add(new SceneInfo(prefab, assetContainer, isLoadingScene));
        }
#endif
        
        public static void AddResourceObject(ResourceObject resource)
        {
            var resourceObject = Resources.FirstOrDefault(a => a.Data.Guid == resource.Data.Guid);
            if (resourceObject == null)
            {
                Resources.Add(resource);
            }
        }

        public static void AddObjectIcon(int objectId, Sprite sprite)
        {
            if (!ObjectIcons.ContainsKey(objectId))
            {
                ObjectIcons.Add(objectId, sprite);
            }
        }

        public static void AddToEmbeddedList(int objectId)
        {
            if (!EmbeddedObjects.Contains(objectId))
            {
                EmbeddedObjects.Add(objectId);
            }
        }

#if VARWINCLIENT          
        public static SceneInfo GetSceneInfo(int sceneId)
        {
            return SceneTemplatePrefabs.FirstOrDefault(a => a.SceneTemplatePrefab.Id == sceneId);
        }
#endif

#if VARWINCLIENT  
        public static ObjectInfo GetObjectInfo(int objectId)
        {
            return ObjectPrefabs.FirstOrDefault(a => a.PrefabObject.Id == objectId);
        } 
#endif

        public static GameObject GetPrefabGameObject(int objectId)
        {
#if VARWINCLIENT            
            var result = ObjectPrefabs.FirstOrDefault(a => a.PrefabObject.Id == objectId);
            return result?.AssetContainer.GetMainAsset();
#else
            return null;
#endif
        }

        public static PrefabObject GetPrefabData(int objectId)
        {
#if VARWINCLIENT     
            var result = ObjectPrefabs.FirstOrDefault(a => a.PrefabObject.Id == objectId);
            return result?.PrefabObject;
#else
            return null;
#endif
        }

        public static PrefabObject GetPrefabData(string rootGuid)
        {
#if VARWINCLIENT     
            var result = ObjectPrefabs.FirstOrDefault(a => a.PrefabObject.RootGuid == rootGuid);
            return result?.PrefabObject;
#else
            return null;
#endif
        }

        public static bool ResourceDataIsLoaded(string resourceGuid) => Resources.Any(a => a.Data.Guid == resourceGuid);

        public static ResourceObject GetResource(string resourceGuid) => Resources.FirstOrDefault(a => a.Data.Guid == resourceGuid);

        public static ResourceDto GetResourceDtoById(int id) => Resources.FirstOrDefault(a=>a.Data.Id == id)?.Data;

        public static ResourceDto GetResourceData(string resourceGuid) => Resources.FirstOrDefault(a => a.Data.Guid == resourceGuid)?.Data;
        public static object GetResourceValue(string resourceGuid) => GetResource(resourceGuid)?.Value;

        public static List<PrefabObject> GetPrefabsData()
        {
#if VARWINCLIENT                 
            return ObjectPrefabs.Select(a => a.PrefabObject).ToList();
#else
            return null;
#endif
        }

        public static List<ResourceDto> GetResourcesData() => Resources.Select(a => a.Data).ToList();
        public static Sprite GetObjectIcon(int objectId) => ObjectIcons.ContainsKey(objectId) ? ObjectIcons[objectId] : null;
        public static bool IsEmbedded(int objectId) => EmbeddedObjects.Contains(objectId);

#if VARWINCLIENT
        public static void RefreshLogic(LogicInstance logicInstance, byte[] assemblyBytes)
        {
            ClearLogic();
            WrappersCollection.Clear();
            Debug.Log("Logic was refreshed");
        }
#endif
        
        public static void ClearLogic()
        {
#if VARWINCLIENT
            _logicInstance?.Clear();
            SceneLogicManager.Clear();
#endif
        }

#if VARWINCLIENT
        public static void SetLogic(LogicInstance logicInstance)
        {
            _logicInstance = logicInstance;
        }
#endif
        public static void GameModeChanged(GameMode newMode, GameMode oldMode)
        {
            var objects = GetAllObjects();

            foreach (ObjectController o in objects)
            {
                o.ApplyGameMode(newMode, oldMode);
                o.ExecuteSwitchGameModeOnObject(newMode, oldMode);
            }
        }

        public static void PlatformModeChanged(PlatformMode newMode, PlatformMode oldMode)
        {
            var objects = GetAllObjects();

            foreach (ObjectController o in objects)
            {
                o.ApplyPlatformMode(newMode, oldMode);
                o.ExecuteSwitchPlatformModeOnObject(newMode, oldMode);
            }
        }

        private static List<ObjectController> GetAllObjects() => ObjectsIds.Keys.ToList();

        public static void AddAssembly(string dllName, Assembly assembly)
        {
            if (Assemblies.ContainsKey(dllName))
            {
                return;
            }

            Assemblies.Add(dllName, assembly);
        }

        public static Assembly GetAssembly(string dllName)
        {
            return Assemblies.TryGetValue(dllName, out var assembly) ? assembly : null;
        }

        public static PrefabObject PlayerObject => _playerObject;

        public static void SetPlayerObject(PrefabObject playerObject)
        {
            if (_playerObject == null)
            {
                _playerObject = playerObject;
            }
        }

        public static bool HasObjectOnScene(int spawnParamIdInstance) => WrappersCollection.ContainsKey(spawnParamIdInstance);

        public static bool ObjectIsLocked(ObjectController objectController) => ObjectIsLocked(objectController.IdServer);
        public static bool ObjectIsLocked(int objectControllerIdServer) => _sceneObjectLockedIds.Contains(objectControllerIdServer);

        public static void UpdateSceneObjectLockedIds(IEnumerable<LockedSceneObject> changedObjects)
        {
            foreach (LockedSceneObject lockedObject in changedObjects)
            {
                if (lockedObject.UsedInSceneLogic)
                {
                    _sceneObjectLockedIds.Add(lockedObject.Id);
                }
                else
                {
                    _sceneObjectLockedIds.Remove(lockedObject.Id);
                }
            }
        }

        public class LockedSceneObject
        {
            public int Id;
            public bool UsedInSceneLogic;
        }
    }
}
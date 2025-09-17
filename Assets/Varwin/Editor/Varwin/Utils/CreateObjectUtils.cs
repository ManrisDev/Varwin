using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin.Public;
using Object = UnityEngine.Object;

namespace Varwin.Editor
{
    public static class CreateObjectUtils
    {
        private const string GlobalAssemblyFullName = "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        private static readonly Dictionary<Type, bool> ScriptsGlobalStatus = new Dictionary<Type, bool>();

        public static ObjectId AddObjectId(GameObject go)
        {
            if (!go)
            {
                return null;
            }

            go.hideFlags = HideFlags.None;
            
            var objectIdComponent = go.GetComponent<ObjectId>();
            if (!objectIdComponent)
            {
                objectIdComponent = go.AddComponent<ObjectId>();
                objectIdComponent.Id = go.GetInstanceID();
            }

            return objectIdComponent;
        }
        
        public static void SetupObjectIds(GameObject go)
        {
            var components = go.GetComponentsInChildren<MonoBehaviour>(true);
            
            if (components.Length > 0)
            {
                var objects = new HashSet<GameObject>();
                
                foreach (var component in components)
                {
                    if (component && component.gameObject && !objects.Contains(component.gameObject))
                    {
                        objects.Add(component.gameObject);
                    }
                }
                
                var usedIds = new HashSet<int>();
                
                foreach (var obj in objects)
                {
                    ObjectId objectId = AddObjectId(obj.gameObject);
             
                    if (objectId)
                    {
                        CheckObjectId(usedIds, objectId);
                    }
                }
            }
        }

        private static void CheckObjectId(HashSet<int> ids, ObjectId objectId)
        {
            if (ids.Contains(objectId.Id))
            {
                do
                {
                    objectId.Id++;
                } while (ids.Contains(objectId.Id));
            }
            ids.Add(objectId.Id);
        }
        
        public static GameObject GetPrefabObject(GameObject go)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
            if (prefab)
            {
                return prefab;
            }
            
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(go);
            if (prefabStage != null)
            {
                return prefabStage.prefabContentsRoot;
            }

            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go)))
            {
                return go;
            }
            
            return null;
        }
        
        public static string GetPrefabPath(GameObject go)
        {
            GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
            string prefabPath = null;

            if (prefab)
            {
                prefabPath = AssetDatabase.GetAssetPath(prefab);
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    return prefabPath;
                }
            }
            
            UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(go);
            if (prefabStage != null && !string.IsNullOrEmpty(prefabStage.prefabAssetPath))
            {
                return prefabStage.prefabAssetPath;
            }

            prefabPath = AssetDatabase.GetAssetPath(go);
            if (!string.IsNullOrEmpty(prefabPath))
            {
                return prefabPath;
            }
            
            return null;
        }

        public static bool IsPrefabAsset(GameObject go)
        {
            return GetPrefabObject(go) == go;
        }

        public static void ApplyPrefabInstanceChanges(GameObject go)
        {
            var objectFromSource = PrefabUtility.GetCorrespondingObjectFromSource(go);
            GameObject prefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);

            if (prefabInstanceRoot && objectFromSource)
            {
                try
                {
                    CreateBackupObjectIfNotExists(prefabInstanceRoot, prefabPath);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(prefabInstanceRoot, prefabPath, InteractionMode.AutomatedAction);
                }
                catch
                {
                    Debug.LogWarning($"Can't save object at path \"{prefabPath}\"", go);
                }
            }
            else
            {
                var vod = go.GetComponent<VarwinObjectDescriptor>();
                
                if (vod)
                {
                    try
                    {
                        prefabPath = vod.Prefab;
                        CreateBackupObjectIfNotExists(go, prefabPath);
                        PrefabUtility.SaveAsPrefabAssetAndConnect(go, prefabPath, InteractionMode.AutomatedAction);
                    }
                    catch
                    {
                        Debug.LogWarning($"Can't save object at path \"{prefabPath}\"", go);
                    }
                }
                else
                {
                    throw new Exception("VarwinObjectDescriptor is not found in the prefab");
                }
            }
        }

        private static void CreateBackupObjectIfNotExists(GameObject go, string prefabPath)
        {
            var backupPath = prefabPath.Replace(".prefab", "_backup.prefab");
            var isBackupExists = AssetDatabase.LoadAssetAtPath<Object>(backupPath);
            if (!isBackupExists)
            {
                AssetDatabase.CopyAsset(prefabPath, backupPath);
            }
        }

        public static void RemoveBackupObjectIfExists(string prefabPath)
        {
            var backupPath = prefabPath.Replace(".prefab", "_backup.prefab");
            var isBackupExists = AssetDatabase.LoadAssetAtPath<Object>(backupPath);
            if (isBackupExists)
            {
                AssetDatabase.DeleteAsset(backupPath);
            }
        }

        public static void RestoreObjectFromBackup(ObjectBuildDescription buildDescription)
        {
            if (!buildDescription.ContainedObjectDescriptor)
            {
                return;
            }
            
            var prefabPath = buildDescription.ContainedObjectDescriptor.Prefab;
            var backupPath = prefabPath.Replace(".prefab", "_backup.prefab");
            var isBackupExists = AssetDatabase.LoadAssetAtPath<Object>(backupPath);
            if (!isBackupExists)
            {
                return;
            }

            AssetDatabase.CopyAsset(backupPath, prefabPath);
            AssetDatabase.DeleteAsset(backupPath);
        }

        public static void RevertPrefabInstanceChanges(GameObject go)
        {
            try
            {
                PrefabType prefabType = PrefabUtility.GetPrefabType(go);

                bool flag = prefabType == PrefabType.DisconnectedModelPrefabInstance ||
                            prefabType == PrefabType.DisconnectedPrefabInstance;
                GameObject parentPrefab = GetPrefabObject(go);

                if (parentPrefab)
                {
                    if (flag)
                    {
                        PrefabUtility.ReconnectToLastPrefab(parentPrefab);
                    }

                    PrefabUtility.RevertPrefabInstance(parentPrefab, InteractionMode.AutomatedAction);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Can't revert prefab: {e}", go);
            }
        }

        public static List<ObjectId> GetObjectIdDuplicates(GameObject go)
        {
            var objectIds = go.GetComponentsInChildren<ObjectId>(true);
            var duplicates = new List<ObjectId>();
            foreach (var objectId in objectIds)
            {
                if (objectId)
                {
                    if (objectIds.GroupBy(x => x.Id).Any(g => g.Count() > 1 && g.Key == objectId.Id))
                    {
                        duplicates.Add(objectId);
                    }
                }
            }
            return duplicates;
        }
        
        public static bool ContainsObjectIdDuplicates(GameObject go)
        {
            var objectIds = go.GetComponentsInChildren<ObjectId>(true);
            return objectIds.GroupBy(x => x.Id).Any(g => g.Count() > 1);
        }

        public static void SetupAsmdef(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            string prefabPath = varwinObjectDescriptor.Prefab;
            if (!string.IsNullOrEmpty(varwinObjectDescriptor.PrefabGuid))
            {
                prefabPath = AssetDatabase.GUIDToAssetPath(varwinObjectDescriptor.PrefabGuid);
                varwinObjectDescriptor.Prefab = prefabPath;
            }
            
            var asmdefPath = AsmdefUtils.FindAsmdef(prefabPath)?.FullName;

            if (string.IsNullOrEmpty(asmdefPath))
            {
                return;
            }

            var asmdefData = AsmdefUtils.LoadAsmdefData(asmdefPath);
            var asmdefReferences = asmdefData.references.ToHashSet();

            var referencesIsChanged = false;

            var references = GetAssembliesReferences(varwinObjectDescriptor);
            
            foreach (var reference in references)
            {
                if (string.IsNullOrEmpty(reference))
                {
                    throw new Exception(
                        $"Assembly definition of object \"{varwinObjectDescriptor.Name}\" contains missing reference.\n" +
                        $"Assembly definition path is \"{asmdefPath}\"."
                        );
                }
                
                if (reference == asmdefData.name || asmdefReferences.Contains(reference))
                {
                    continue;
                }

                asmdefReferences.Add(reference);
                referencesIsChanged = true;
            }

            const string unityNetcodeRuntimeAsmdefName = "Unity.Netcode.Runtime";
            if (!references.Contains(unityNetcodeRuntimeAsmdefName))
            {
                asmdefReferences.Add(unityNetcodeRuntimeAsmdefName);
                referencesIsChanged = true;
            }

            if (referencesIsChanged)
            {
                asmdefData.references = asmdefReferences.ToArray();
                asmdefData.Save(asmdefPath);
            }
            
            AsmdefUtils.CollectReferences(asmdefData);
        }

        public static IEnumerable<MonoBehaviour> GetGlobalScripts(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            var scripts = new List<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                string globalScriptPath = GetGlobalScriptPath(monoBehaviour);
                if (!string.IsNullOrEmpty(globalScriptPath))
                {
                    scripts.Add(monoBehaviour);
                }
            }

            return scripts;
        }
        
        public static bool ContainsGlobalScript(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            return monoBehaviours.Any(monoBehaviour => monoBehaviour && IsGlobalScript(monoBehaviour.GetType()));
        }

        public static bool IsGlobalScript(Type type)
        {
            if (!ScriptsGlobalStatus.TryGetValue(type, out bool isGlobal))
            {
                isGlobal = type.Assembly.FullName == GlobalAssemblyFullName;
                ScriptsGlobalStatus.Add(type, isGlobal);
            }
            return isGlobal;
        }

        public static IEnumerable<string> GetGlobalScriptsPaths(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            return GetGlobalScriptsPaths(varwinObjectDescriptor.gameObject.GetComponentsInChildren<MonoBehaviour>(true));
        }

        public static IEnumerable<string> GetGlobalScriptsPaths(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            var scripts = new List<string>();
            
            foreach (MonoBehaviour monoBehaviour in monoBehaviours)
            {
                string globalScriptPath = GetGlobalScriptPath(monoBehaviour);
                if (!string.IsNullOrEmpty(globalScriptPath))
                {
                    scripts.Add(globalScriptPath);
                }
            }
            
            return scripts;
        }

        public static string GetGlobalScriptPath(MonoBehaviour monoBehaviour)
        {
            if (!monoBehaviour)
            {
                return null;
            }

            if (SdkIgnoredScripts.ContainsType(monoBehaviour.GetType()))
            {
                return null;
            }

            string monoBehaviourPath = TypeUtils.FindScript(monoBehaviour.GetType(), false);

            if (monoBehaviourPath == null || monoBehaviourPath.StartsWith("Assets/Varwin/"))
            {
                return null;
            }
                
            string monoBehaviourAsmdefPath = AsmdefUtils.FindAsmdef(monoBehaviourPath)?.GetAssetPath();
            if (monoBehaviourAsmdefPath != null)
            {
                return null;
            }
            
            return monoBehaviourPath.Replace("\\", "/");
        }
        
        public static IEnumerable<string> GetAssembliesReferences(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            return GetAssembliesReferences(varwinObjectDescriptor.gameObject.GetComponentsInChildren<MonoBehaviour>(true));
        }

        public static IEnumerable<string> GetAssembliesReferences(IEnumerable<MonoBehaviour> monoBehaviours)
        {
            var references = new HashSet<string>();
            
            foreach (var monoBehaviour in monoBehaviours)
            {
                if (!monoBehaviour)
                {
                    continue;
                }
                
                var otherAsmdefPath = AsmdefUtils.FindAsmdef(monoBehaviour.GetType())?.FullName;
                if (otherAsmdefPath == null)
                {
                    continue;
                }

                var otherAsmdefData = AsmdefUtils.LoadAsmdefData(otherAsmdefPath);
                if (otherAsmdefData == null)
                {
                    continue;
                }

                if (!references.Contains(otherAsmdefData.name))
                {
                    references.Add(otherAsmdefData.name);
                }
            }

            return references;
        }

        public static void SetupComponentReferences(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            if (varwinObjectDescriptor.Components == null)
            {
                varwinObjectDescriptor.Components = new ComponentReferenceCollection();
            }
            
            varwinObjectDescriptor.Components.Setup(varwinObjectDescriptor.gameObject);
        }

        public static bool CheckNeedBuildWithVarwinTemp(GameObject go)
        {
            string prefabPath = GetPrefabPath(go);
        
            if (string.IsNullOrEmpty(prefabPath))
            {
                return true;
            }
        
            if (AsmdefUtils.FindAsmdef(prefabPath) == null)
            {
                return true;
            }
        
            return false;
        }

        public static bool CheckNeedBuildWithVarwinTemp(Component component)
        {
            return CheckNeedBuildWithVarwinTemp(component.gameObject);
        }
    }
}

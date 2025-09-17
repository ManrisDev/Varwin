using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using Varwin.Models.Data;
using Varwin.Public;

namespace Varwin.Data.ServerData
{
    /// <summary>
    /// Project structure data
    /// </summary>
    public class ProjectStructure : IJsonSerializable
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Guid { get; set; }
        public List<Scene> Scenes { get; set; }
        public List<ProjectConfiguration> ProjectConfigurations { get; set; }
        public List<SceneTemplatePrefab> SceneTemplates { get; set; }
        public List<PrefabObject> Objects { get; set; }
        public List<ResourceDto> Resources { get; set; }
        public bool MobileReady { get; set; }
        public bool Multiplayer { get; set; }
        public string LicenseKey { get; set; }
        public ProjectAuthor Author { get; set; }
        public ProjectLicense License { get; set; }
    }

    public class ProjectSceneWithPrefabObjects : IJsonSerializable
    {
        public Scene Scene;
        public SceneTemplatePrefab SceneTemplate;
    }

    public class Scene : IJsonSerializable
    {
        /// <summary>
        /// Instance id of scene
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Guid of scene in world structure
        /// </summary>
        public string Sid { get; set; }
        
        /// <summary>
        /// Scene Template name
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Scene Template prefab id
        /// </summary>
        public int SceneTemplateId { get; set; }
        
        /// <summary>
        /// Scene Template Guid
        /// </summary>
        public string SceneTemplateGuid { get; set; }
        
        /// <summary>
        /// C# logic Code
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Scene-related additional information
        /// </summary>
        public CustomSceneData Data { get; set; }
        
        /// <summary>
        /// C# logic AssemblyBytes
        /// </summary>
        public byte[] AssemblyBytes { get; set; }
        
        /// <summary>
        /// EditorData (Blockly)
        /// </summary>
        public EditorData EditorData { get; set; }
        
        /// <summary>
        /// Scene Template objects
        /// </summary>
        public List<SceneObjectDto> SceneObjects { get; set; }
        
        /// <summary>
        /// Resources
        /// </summary>
        public string Assets { get; set; }

        /// <summary>
        /// Resource path to scene logic assembly
        /// </summary>
        public string LogicResource => Assets + "/logic_assembly.dll";
    }

    public class ProjectSceneArguments
    {
        public Scene Scene = null;
        public SceneTemplatePrefab SceneTemplate = null;
        public StateProjectScene State;
         
        public enum StateProjectScene
        {
            Added, Deleted, Changed
        }
    }
    
    public class ProjectConfigurationArguments
    {
        public ProjectConfiguration ProjectConfiguration = null;
        public StateConfiguration State;
         
        public enum StateConfiguration
        {
            Added, Deleted, Changed
        }
    }

    public class ProjectConfiguration : IJsonSerializable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sid { get; set; }
        public int ProjectId { get; set; }
        public int? StartSceneId { get; set; }
        public int? LoadingSceneTemplateId { get; set; }
        public int? LoadingLogoResourceId { get; set; }
        public bool HideExtendedLoadingStatus { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string Lang { get; set; }
        [JsonConverter(typeof(StringEnumConverter))] public PlatformMode PlatformMode { get; set; }
        public bool DisablePlatformModeSwitching { get; set; }
    }

    public class SceneTemplatePrefab
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public I18n Name { get; set; }
        public I18n Description { get; set; }
        public bool HasScripts { get; set; }
        public string Assets { get; set; }
        public bool LinuxReady { get; set; }
        public string ConfigResource => Assets + "/bundle.json";
        public string IconResource => Assets + "/bundle.png";
        public string LinuxBundleResource => Assets + "/linux_bundle";
        public string LinuxManifestResource => Assets + "/linux_bundle.manifest";
        public string AndroidBundleResource => Assets + "/android_bundle";
        public string AndroidManifestResource => Assets + "/android_bundle.manifest";
        public string BundleResource => Assets + "/bundle";
        public string ManifestResource => Assets + "/bundle.manifest";

        public string GetLocalizedName()
        {
            return Name.GetCurrentLocale();
        }
    }

    /// <summary>
    /// Custom object data
    /// </summary>
    public class ObjectData : IJsonSerializable
    {
        public TransformDT LocalTransform { get; set; }
        
        [Obsolete("Use LocalTransform instead")]
        public Dictionary<int, TransformDT> Transform { get; set; }

        [Obsolete("Use LocalTransform instead")]
        public TransformDT RootTransform { get; set; }
#if VARWINCLIENT
        public List<InspectorPropertyData> InspectorPropertiesData { get; set; }
#endif
        public bool LockChildren { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDisabledInHierarchy { get; set; }
        public bool DisableSelectabilityInEditor { get; set; }
        public int Index { get; set; }
        public int? VirtualParentObjectId { get; set; }
        public List<VirtualObjectInfo> VirtualObjectsInfos { get; set; }
    }

    public class NewObjectData : IJsonSerializable
    {
        public Dictionary<int, TransformDT> Transform { get; set; }
#if VARWINCLIENT
        public List<InspectorPropertyData> InspectorPropertiesData { get; set; }
#endif
        public bool LockChildren { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsDisabledInHierarchy { get; set; }
        public bool DisableSelectabilityInEditor { get; set; }
        public int Index { get; set; }
    }

    public class EditorData
    {
        public string Blockly { get; set; }
    }

    public class ProjectAuthor : IJsonSerializable
    {
        public string Name;
        public string Company;
        public string Email;
        public string Url;
    }

    public class ProjectLicense : IJsonSerializable
    {
        public int Id;
        public string Code;
        public string Version;
        public string Url;
        public string CreatedAt;
        public string UpdatedAt;
    }


    #region Ex
    public static class ProjectStructureEx
    {
        public static Scene GetProjectScene(this List<Scene> self, int id)
        {
            foreach (Scene scene in self)
            {
                if (scene.Id == id)
                {
                    return scene;
                }
            }

            return null;
        }

        public static int GetSceneTemplateId(this List<Scene> self, int projectSceneId)
        {
            Scene scene = self.GetProjectScene(projectSceneId);
            return scene?.SceneTemplateId ?? 0;
        }

        public static Scene GetProjectScene(this List<Scene> self, string sid)
        {
            Guid guid = new Guid(sid);

            foreach (Scene projectScene in self)
            {
                Guid sidGuid = new Guid(projectScene.Sid);

                if (sidGuid == guid)
                {
                    return projectScene;
                }
            }

            return null;
        }
        
        public static ProjectConfiguration GetProjectConfigurationByConfigurationSid(this List<ProjectConfiguration> self, string sid)
        {
            Guid guid = new Guid(sid);

            foreach (ProjectConfiguration projectConfiguration in self)
            {
                Guid sidGuid = new Guid(projectConfiguration.Sid);

                if (sidGuid == guid)
                {
                    return projectConfiguration;
                }
            }

            return null;
        }

        public static ProjectConfiguration GetProjectConfigurationByProjectScene(this List<ProjectConfiguration> self, int projectSceneId)
        {
            foreach (ProjectConfiguration projectConfiguration in self)
            {
                if (projectConfiguration.Id == projectSceneId)
                {
                    return projectConfiguration;
                }
            }

            return null;
        }

        public static ProjectConfiguration GetProjectConfigurationById(this List<ProjectConfiguration> self, int id)
        {
            return self.Find(x => x.Id == id);
        }

        public static List<string> GetNames(this List<ProjectConfiguration> self)
        {
            List<string> names = new List<string>();
            foreach (ProjectConfiguration worldConfiguration in self)
            {
                names.Add(worldConfiguration.Name);
            }

            return names;
        }

        public static int GetId(this List<ProjectConfiguration> self, string name)
        {
            foreach (ProjectConfiguration worldConfiguration in self)
            {
                if (worldConfiguration.Name == name)
                {
                    return worldConfiguration.Id;
                }
            }

            return 0;
        }

        public static Scene GetProjectScene(this ProjectStructure self, string projectSceneSid)
        {
            Scene scene = self.Scenes.GetProjectScene(projectSceneSid);
            return scene;
        }
        
        public static ProjectConfiguration GetConfiguration(this ProjectStructure self, string configurationId)
        {
            ProjectConfiguration worldConfiguration = self.ProjectConfigurations.GetProjectConfigurationByConfigurationSid(configurationId);
            return worldConfiguration;
        }

        [Obsolete]
        public static void UpdateEntities(List<SceneObjectDto> objects)
        {
        }

        public static void UpdateProjectSceneObjects(this Scene self, List<SceneObjectDto> objects)
        {
            self.SceneObjects = objects;
            ProjectData.ObjectsAreChanged = false;
            Debug.Log($"Project scene objects {ProjectData.SceneId} was updated in structure!");
        }

        public static SceneTemplatePrefab GetProjectScene(this List<SceneTemplatePrefab> self, int id)
        {
            foreach (SceneTemplatePrefab sceneTemplatePrefab in self)
            {
                if (sceneTemplatePrefab.Id == id)
                {
                    return sceneTemplatePrefab;
                }
            }

            return null;
        }

        public static PrefabObject GetById(this List<PrefabObject> self, int id)
        {
            foreach (PrefabObject o in self)
            {
                if (o.Id == id)
                {
                    return o;
                }
            }

            return null;
        }

        public static void RemoveProjectScene(this ProjectStructure self, Scene deletedScene)
        {
            Scene result = null; 
            foreach (Scene scene in self.Scenes)
            {
                    
                if (scene.Id == deletedScene.Id)
                {
                    result = scene;
                }
            }

            if (result != null)
            {
                self.Scenes.Remove(result);
            }
        }
        
        public static void UpdateProjectScene(this ProjectStructure self, Scene changedScene)
        {
            for (int i = 0; i < self.Scenes.Count; i++)
            {
                if (self.Scenes[i].Id == changedScene.Id)
                {
                    self.Scenes[i] = changedScene;
                }
            }
        }
        
        public static void UpdateOrAddSceneTemplatePrefab(this ProjectStructure self, SceneTemplatePrefab changedSceneTemplatePrefab)
        {
            for (int i = 0; i < self.SceneTemplates.Count; i++)
            {
                if (self.SceneTemplates[i].Id == changedSceneTemplatePrefab.Id)
                {
                    self.SceneTemplates[i] = changedSceneTemplatePrefab;
                    return;
                }
            }
            
            self.SceneTemplates.Add(changedSceneTemplatePrefab);
        }
        
        public static void RemoveProjectConfiguration(this ProjectStructure self, ProjectConfiguration deletedConfiguration)
        {
            ProjectConfiguration result = null; 
            foreach (ProjectConfiguration worldConfiguration in self.ProjectConfigurations)
            {
                    
                if (worldConfiguration.Id == deletedConfiguration.Id)
                {
                    result = worldConfiguration;
                }
            }

            if (result != null)
            {
                self.ProjectConfigurations.Remove(result);
            }
        }
        
        public static void UpdateProjectConfiguration(this ProjectStructure self, ProjectConfiguration changedConfiguration)
        {
            for (int i = 0; i < self.ProjectConfigurations.Count; i++)
            {
                if (self.ProjectConfigurations[i].Id == changedConfiguration.Id)
                {
                    self.ProjectConfigurations[i] = changedConfiguration;
                }
            }
        }

        public static bool HasObjectOnServer(this Scene self, int serverId)
        {
            return self.SceneObjects.Any(x => x.HasChildWithId(serverId));
        }
        
        private static bool HasChildWithId(this SceneObjectDto self, int serverId)
        {
            if (self.Id == serverId)
            {
                return true;
            }
            
            return self.SceneObjects != null && self.SceneObjects.Any(x => x.HasChildWithId(serverId));
        }
    }
    #endregion
}

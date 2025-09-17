using System.Collections.Generic;
using Newtonsoft.Json;
using Varwin.Models.Data;

namespace Varwin.Data.ServerData
{
    public class SceneTemplateObjectsDto : IJsonSerializable
    {
        [JsonProperty("id")] public int SceneId { get; set; }
        [JsonProperty("sceneObjects")] public List<SceneObjectDto> SceneObjects { get; set; }
        [JsonProperty("data")] public CustomSceneData SceneData { get; set; }
        [JsonProperty("objectBehaviours")] public List<ObjectBehavioursData> ObjectBehaviours { get; set; }
    }

    public class CustomSceneData : IJsonSerializable
    {
        public Vector3DT CameraSpawnPosition;
        public QuaternionDT CameraSpawnRotation;
        public Dictionary<int, bool> HierarchyExpandStates;
        public HashSet<string> OnDemandedResourceGuids;
    }

    public class SceneObjectDto : IJsonSerializable
    {
        private int _id;

        /// <summary>
        /// Server Id
        /// </summary>
        [JsonProperty("id")] public int? Id;

        [JsonProperty("instanceId")] public int InstanceId { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonIgnore] public int? ParentId { get; set; }
        [JsonProperty("objectId")] public int ObjectId { get; set; }

        [JsonIgnore] public Collaboration Collaboration { get; set; }

        [JsonProperty("resourceIds")] public List<int> ResourceIds { get; set; }

        [JsonProperty("data")] public ObjectData Data { get; set; }

        [JsonProperty("sceneObjects")] public List<SceneObjectDto> SceneObjects { get; set; }

        [JsonProperty("usedInSceneLogic")] public bool UsedInSceneLogic
        {
            set => UsedInSceneLogicInternal = value;
        }

        [JsonIgnore] public bool UsedInSceneLogicInternal { get; private set; }
        
        [JsonIgnore] public ObjectController ObjectController { get; set; }

        [JsonProperty("disableSceneLogic")] public bool DisableSceneLogic { get; set; }
    }

    public class ObjectBehavioursData : IJsonSerializable
    {
        [JsonProperty("objectId")] public int ObjectId { get; set; }

        [JsonProperty("behaviours")] public List<string> Behaviours { get; set; }
    }

    public class ResourceId : IJsonSerializable
    {
        [JsonProperty("id")] public int Id;
    }
}
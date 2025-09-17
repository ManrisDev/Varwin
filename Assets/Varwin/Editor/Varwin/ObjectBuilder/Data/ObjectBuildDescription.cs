using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Data;
using Varwin.Public;

namespace Varwin.Editor
{
    public class ObjectBuildDescription : IJsonSerializable
    {
        public string ObjectName;
        public string ObjectGuid;
        public string PrefabPath;
        public string RootGuid;
        public string NewGuid;

        [JsonIgnore]
        private VarwinObjectDescriptor _containedObjectDescriptor;
        
        [JsonIgnore]
        public VarwinObjectDescriptor ContainedObjectDescriptor
        {
            get
            {
                if (!_containedObjectDescriptor)
                {
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                    if (prefab)
                    {
                        _containedObjectDescriptor = prefab.GetComponent<VarwinObjectDescriptor>();
                    }
                }

                return _containedObjectDescriptor;
            }
            set => _containedObjectDescriptor = value;
        }

        [JsonIgnore]
        public GameObject GameObject => ContainedObjectDescriptor.gameObject;

        [JsonIgnore]
        public string Guid => ObjectGuid.Replace("-", "");

        [JsonIgnore]
        public string BundleName => $"{ObjectName.ToLower()}_{Guid}";

        [JsonIgnore]
        public Dictionary<string, List<string>> AssetBundleParts = null;
        
        public string FolderPath;

        public string ConfigBlockly;
        public string ConfigAssetBundle;

        public DateTime StartTime;
        public bool HasError;

        public string TagsPath => $"{FolderPath}/tags.txt";

        public string IconPath => $"{FolderPath}/icon.png";

        public List<string> Assemblies = new List<string>();

        public ObjectBuildDescription()
        {
            StartTime = DateTime.Now;
        }

        public ObjectBuildDescription(VarwinObjectDescriptor varwinObjectDescriptor) : this()
        {
            ObjectName = varwinObjectDescriptor.Name;
            ObjectGuid = varwinObjectDescriptor.RootGuid;
            PrefabPath = varwinObjectDescriptor.Prefab;

            if (!string.IsNullOrEmpty(varwinObjectDescriptor.PrefabGuid))
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(varwinObjectDescriptor.PrefabGuid);
                if (!string.IsNullOrEmpty(prefabPath) && !string.Equals(PrefabPath, prefabPath))
                {
                    PrefabPath = prefabPath;
                }
            }

            if (!string.IsNullOrEmpty(PrefabPath))
            {
                FolderPath = Path.GetDirectoryName(PrefabPath);
            }

            HasError = false;
            
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab)
            {
                ContainedObjectDescriptor = prefab.GetComponent<VarwinObjectDescriptor>();
            }

            if (ContainedObjectDescriptor)
            {
                ContainedObjectDescriptor = varwinObjectDescriptor;
            }

            ConfigBlockly = varwinObjectDescriptor.ConfigBlockly;
            ConfigAssetBundle = varwinObjectDescriptor.ConfigAssetBundle;
        }
    }
}
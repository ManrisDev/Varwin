
using System.Collections.Generic;
using UnityEngine;
using Varwin.Models.Data;
using Varwin.Public;

namespace Varwin
{
    public class InitObjectParams
    {
        /// <summary>
        /// Instance id in Wrapper Collection
        /// </summary>
        public int Id;
        /// <summary>
        /// Id group
        /// </summary>
        public int IdScene;
        /// <summary>
        /// Object Type Id. Used to save.
        /// </summary>
        public int IdObject;
        /// <summary>
        /// Instance id from server api
        /// </summary>
        public int IdServer;
        public string Name;
        public GameObject RootGameObject;
        public GameObject Asset;
        public WrappersCollection WrappersCollection;
        public ObjectController Parent;
        public TransformDT LocalTransform;
        public TransformDT WorldTransform;
        public int Index;
        public bool Embedded;
        public bool LockChildren;
        public bool DisableSelectabilityInEditor;
        public bool DisableSceneLogic;
        public bool IsDisabled;
        public bool IsDisabledInHierarchy;
        public bool IsVirtualObject;
        public bool SceneTemplateObject;
        public I18n LocalizedNames;
#if VARWIN_CLIENTCORE
        public List<InspectorPropertyData> ResourcesPropertyData;
#endif
        public List<VirtualObjectInfo> VirtualObjectsData;
    }
}

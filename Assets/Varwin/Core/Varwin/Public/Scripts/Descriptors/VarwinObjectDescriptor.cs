using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Varwin.Public
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Varwin/Varwin Object Descriptor")]
    public class VarwinObjectDescriptor : MonoBehaviour, IWrapperAware
    {
        [TextArea] public string ConfigBlockly;
        [TextArea] public string ConfigAssetBundle;

        public Texture2D Icon;

        public Texture2D ThumbnailImage;
        public Texture2D ViewImage;
        public Texture2D SpritesheetImage;

        public string Name;

        [SerializeField] public LocalizationDictionary DisplayNames = new();
        [SerializeField] public LocalizationDictionary Description = new();

        public AssetBundlePart[] AssetBundleParts;

        public string Prefab;
        public string PrefabGuid;

        public string Guid;
        public string RootGuid;

        public bool Locked;
        public bool Embedded;

        public bool SourcesIncluded;
        public bool MobileReady;

        public bool DisableSceneLogic;
        public bool AddBehavioursAtRuntime;

        public string AuthorName;
        public string AuthorEmail;
        public string AuthorUrl;

        public string LicenseCode;
        public string LicenseVersion;

        public string BuiltAt;
        public string AssemblySuffix;

        public bool CurrentVersionWasBuilt;
        public bool CurrentVersionWasBuiltAsMobileReady;

        [TextArea] public string Changelog;

        public ComponentReferenceCollection Components;
        public SignatureCollection Signatures;

        public string Namespace => $"{Name}_{RootGuid.Replace("-", "")}";

        public bool IsVarwinObject => !string.IsNullOrEmpty(RootGuid) || !string.IsNullOrEmpty(Guid);

        public bool IsFirstVersion => string.Equals(Guid, RootGuid);

        public string WrapperAssemblyQualifiedName => $"Varwin.Types.{Namespace}.{Name}Wrapper, {Namespace}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"; 

        private Wrapper _wrapper;
        private bool _triedToFindLegacyWrapper;

        public ObjectController ObjectController { get; protected set; }

        private void Reset()
        {
            Validate();
        }

        private void OnValidate()
        {
            Validate();
        }

        protected virtual void OnDestroy()
        {
            _wrapper = null;
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Guid))
            {
                return;
            }
            
            if (string.IsNullOrWhiteSpace(RootGuid))
            {
                if (string.IsNullOrWhiteSpace(Guid))
                {
                    Debug.LogError($"Object {gameObject.name} has empty guid. New guid was generated.", this);
                    Guid = System.Guid.NewGuid().ToString();
                }
                
                RootGuid = Guid;
            }

            if (string.IsNullOrWhiteSpace(AuthorName))
            {
                AuthorName = "Anonymous";
            }

            if (string.IsNullOrWhiteSpace(LicenseCode))
            {
                LicenseCode = "cc-by";
            }

            if (string.IsNullOrWhiteSpace(LicenseVersion))
            {
                LicenseVersion = "4.0";
            }
        }

        public void PreBuild()
        {
            CurrentVersionWasBuilt = true;

            CurrentVersionWasBuiltAsMobileReady = MobileReady;
            
            BuiltAt = DateTimeOffset.Now.ToString();
            
            Validate();
        }

        public void RegenerateGuid()
        {
            Guid = System.Guid.NewGuid().ToString(); 
            RootGuid = Guid;
        }

        public void CleanVarwinObjectInfo()
        {
            Prefab = null;
            PrefabGuid = null;
            Icon = null;
        }

        public void CleanBuiltInfo()
        {
            BuiltAt = null;
            CurrentVersionWasBuilt = false;
            CurrentVersionWasBuiltAsMobileReady = false;
        }

        public void InitObjectController(ObjectController objectController)
        {
            if (!ObjectController)
            {
                ObjectController = objectController;
            }
        }

        public Wrapper Wrapper()
        {
            if (_wrapper != null)
            {
                return _wrapper;
            }

            if (string.IsNullOrEmpty(RootGuid))
            {
                RootGuid = Guid;
            }

            // Backward Compatibility
            if (!_triedToFindLegacyWrapper)
            {
                _wrapper = GetWrapperInOldIWrapperAware();
                if (_wrapper != null)
                {
                    return _wrapper;
                }
            }

            Type wrapperType = null;
            var wrapperAssemblyQualifiedName = GetWrapperAssemblyQualifiedName();
            if (!string.IsNullOrEmpty(wrapperAssemblyQualifiedName))
            {
                wrapperType = Type.GetType(wrapperAssemblyQualifiedName);

                if (wrapperType == null)
                {
                    var wrapperAssemblyQualifiedNameWithoutSuffix = GetWrapperAssemblyQualifiedName(false);
                    wrapperType = Type.GetType(wrapperAssemblyQualifiedNameWithoutSuffix);
                }
            }

            if (wrapperType == null)
            {
                var varwinObjectInheritors = GetComponentsInChildren<VarwinObject>().Where(x => x.GetType().IsSubclassOf(typeof(VarwinObject)));
                
                var targetVarwinObject = varwinObjectInheritors.FirstOrDefault(x => x.GetType().FullName.IndexOf(Name) >= 0);
                if (targetVarwinObject)
                {
                    var targetAssembly = Assembly.GetAssembly(targetVarwinObject.GetType());
                    wrapperType = targetAssembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(Wrapper)));
                }

                if (wrapperType == null)
                {
                    foreach (var varwinObjectInheritor in varwinObjectInheritors)
                    {
                        var targetAssembly = Assembly.GetAssembly(varwinObjectInheritor.GetType());
                        wrapperType = targetAssembly.GetTypes().FirstOrDefault(x => x.IsSubclassOf(typeof(Wrapper)));
                        if (wrapperType != null)
                        {
                            break;
                        }
                    }
                }
            }

#if !VARWIN_SDK
            if (wrapperType == null && ObjectController && ObjectController.PrefabObject != null)
            {
                var dllNames = ObjectController.PrefabObject.AssetInfo.Assembly;
                dllNames.Reverse();
                foreach (var dllName in dllNames)
                {
                    var assembly = GameStateData.GetAssembly(dllName);
                    foreach (var assemblyExportedType in assembly.ExportedTypes)
                    {
                        if (typeof(Wrapper).IsAssignableFrom(assemblyExportedType))
                        {
                            wrapperType = assemblyExportedType;
                            break;
                        }
                    }

                    if (wrapperType != null)
                    {
                        break;
                    }
                }
            }
#endif

            if (wrapperType == null)
            {
                _wrapper = new NullWrapper(gameObject);
                return _wrapper;
            }

            _wrapper = (Wrapper) Activator.CreateInstance(wrapperType, new object[] {gameObject});
            return _wrapper;
        }

        public string GetWrapperAssemblyQualifiedName(bool useSuffix = true)
        {
            var wrapperAssemblyQualifiedNameWithSuffix = $"Varwin.Types.{Namespace}.{Name}Wrapper, {Namespace}{AssemblySuffix}, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
            return string.IsNullOrEmpty(AssemblySuffix) || !useSuffix ? WrapperAssemblyQualifiedName : wrapperAssemblyQualifiedNameWithSuffix;
        }

        private Wrapper GetWrapperInOldIWrapperAware()
        {
            if (_triedToFindLegacyWrapper)
            {
                return null;
            }

            _triedToFindLegacyWrapper = true;
            
            var allWrapperAwares = GetComponents<IWrapperAware>();

            var onlyGenericWrapperAwares = allWrapperAwares
                .Where(x => !x.GetType().IsAssignableFrom(typeof(VarwinObject)))
                .Where(x => !x.GetType().IsAssignableFrom(typeof(VarwinObjectDescriptor))).ToArray();

            if (onlyGenericWrapperAwares.Length != 1)
            {
                return null;
            }
            
            IWrapperAware oldWrapperAware = onlyGenericWrapperAwares[0];
            Type oldWrapperAwareType = oldWrapperAware.GetType();

            if (!oldWrapperAwareType.FullName.EndsWith("Type"))
            {
                return null;
            }
            
            string oldWrapperAwareName = oldWrapperAwareType.FullName.SubstringBeforeLast("Type");

            if (!oldWrapperAwareName.Equals($"Varwin.Types.{Namespace}.{Name}"))
            {
                return null;
            }
            
            Assembly oldWrapperAwareAssembly = oldWrapperAwareType.Assembly;
            Type oldWrapperAwareWrapperType = oldWrapperAwareAssembly.GetType(oldWrapperAwareName + "Wrapper");
                
            if (oldWrapperAwareWrapperType != null)
            {
                return oldWrapperAware.Wrapper();
            }

            return null;
        }
    }
}
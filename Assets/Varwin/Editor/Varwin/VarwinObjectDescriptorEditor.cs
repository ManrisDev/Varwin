using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin;
using Varwin.Editor;
using Varwin.Public;
using SignatureUtils = Varwin.Editor.SignatureUtils;

[CustomEditor(typeof(VarwinObjectDescriptor))]
[CanEditMultipleObjects]
public class VarwinObjectDescriptorEditor : Editor
{
    private VarwinObjectDescriptor _varwinObjectDescriptor;
    private MonoBehaviour[] _monoBehaviours;
    private GameObject _gameObject;
    
    private SerializedProperty _nameProperty;
    private SerializedProperty _iconProperty;
    
    private SerializedProperty _viewImageProperty;
    private SerializedProperty _spritesheetImageProperty;
    private SerializedProperty _thumbnailImageProperty;

    private SerializedProperty _lockedProperty;
    private SerializedProperty _embeddedProperty;
    
    private SerializedProperty _sourcesIncludedProperty;
    private SerializedProperty _mobileReadyProperty;
    
    private SerializedProperty _disableSceneLogic;
    private SerializedProperty _addBehavioursAtRuntime;
    
    private SerializedProperty _displayNamesProperty;
    private SerializedProperty _descriptionProperty;
    
    private SerializedProperty _configBlocklyProperty;
    private SerializedProperty _configAssetBundleProperty;
    private SerializedProperty _changelogProperty;
    private SerializedProperty _guidProperty;
    private SerializedProperty _rootGuidProperty;
    private SerializedProperty _prefabProperty;
    private SerializedProperty _assetBundlePartProperty;
    private SerializedProperty _prefabGuidProperty;
    
    private SerializedProperty _authorNameProperty;
    private SerializedProperty _authorEmailProperty;
    private SerializedProperty _authorUrlProperty;

    private SerializedProperty _licenseCodeProperty;
    private SerializedProperty _licenseVersionProperty;
    private LicenseType _selectedLicense;
    private int _selectedLicenseIndex = -1;
    private int _selectedLicenseVersionIndex = -1;
    
    private SerializedProperty _builtAtProperty;

    private bool _showDebug;
    private bool _showDeveloperMode;
    private bool _objectNameIsValid;
    private List<KeyValuePair<Signature, Signature>> _changedSignatures;
    
    private const string DefaultAssembly = "Assembly-CSharp";

    private bool _lastUnityCompilingStatus;
    private string _firstGlobalScript;
    private bool _globalScriptFolderIsRoot;
    private bool _scriptsFolderContainsAsmdef;
    private bool _containsGlobalScripts;
    
    private void Awake()
    {
        OnEnable();
    }

    private void Reset()
    {
        if (!_varwinObjectDescriptor)
        {
            _varwinObjectDescriptor = (VarwinObjectDescriptor) serializedObject.targetObject;

            if (_varwinObjectDescriptor)
            {
                _gameObject = _varwinObjectDescriptor.gameObject;
            }
        }
        
        if (_monoBehaviours == null)
        {
            _monoBehaviours = _varwinObjectDescriptor.gameObject.GetComponentsInChildren<MonoBehaviour>(true);
        }
        _containsGlobalScripts = CreateObjectUtils.ContainsGlobalScript(_monoBehaviours);

        UpdateGlobalBehaviourStatus();
        UpdateChangedSignatures();
    }

    private void UpdateChangedSignatures()
    {
        SignatureCollection currentSignatures = SignatureUtils.MakeSignatures(_varwinObjectDescriptor);
        _changedSignatures = currentSignatures.FindAllChangedSignatures(_varwinObjectDescriptor.Signatures);
    }

    private void UpdateGlobalBehaviourStatus()
    {
        _firstGlobalScript = null;

        if (_monoBehaviours == null)
        {
            return;
        }

        foreach (MonoBehaviour monoBehaviour in _monoBehaviours)
        {
            if (!monoBehaviour)
            {
                continue;
            }

            Type type = monoBehaviour.GetType();
            if (type.Assembly.GetName().Name != DefaultAssembly)
            {
                continue;
            }

            _firstGlobalScript = CreateObjectUtils.GetGlobalScriptPath(monoBehaviour);
            if (string.IsNullOrEmpty(_firstGlobalScript))
            {
                continue;
            }

            string globalScriptFolder = new FileInfo(_firstGlobalScript).DirectoryName?.Replace("\\", "/");
            _globalScriptFolderIsRoot = globalScriptFolder == UnityProject.Assets;
            _scriptsFolderContainsAsmdef = Directory.Exists("Assets/Scripts") && AsmdefUtils.FindAsmdef("Assets/Scripts") != null;
            break;
        }
    }

    private void OnEnable()
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            foreach (var targetObject in serializedObject.targetObjects)
            {
                var targetSerializedObject = new SerializedObject(targetObject);
                InitializeSerializedObject(targetSerializedObject);
            }

            InitializeProperties(serializedObject);
        }
        else
        {
            InitializeSerializedObject(serializedObject);
        }
        
        serializedObject.SetIsDifferentCacheDirty();
    }

    private void InitializeSerializedObject(SerializedObject currentSerializedObject)
    {
        _varwinObjectDescriptor = (VarwinObjectDescriptor) currentSerializedObject.targetObject;

        if (_varwinObjectDescriptor)
        {
            _gameObject = _varwinObjectDescriptor.gameObject;
        }

        InitializeProperties(currentSerializedObject);
        
        InitializeDisplayName();
        InitializeDescription();
        
        CreateObjectUtils.SetupComponentReferences(_varwinObjectDescriptor);

        InitializePrefabInfo();
        
        if (!_varwinObjectDescriptor.IsVarwinObject)
        {
            InitializeDefaultData();
        }

        currentSerializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private void InitializeProperties(SerializedObject currentSerializedObject)
    {
        _nameProperty = currentSerializedObject.FindProperty("Name");
        _iconProperty = currentSerializedObject.FindProperty("Icon");
        
        _viewImageProperty = currentSerializedObject.FindProperty("ViewImage");
        _thumbnailImageProperty = currentSerializedObject.FindProperty("ThumbnailImage");
        _spritesheetImageProperty = currentSerializedObject.FindProperty("SpritesheetImage");
        
        _mobileReadyProperty = currentSerializedObject.FindProperty("MobileReady");
        _sourcesIncludedProperty = currentSerializedObject.FindProperty("SourcesIncluded");
        
        _disableSceneLogic = currentSerializedObject.FindProperty("DisableSceneLogic");
        _addBehavioursAtRuntime = currentSerializedObject.FindProperty("AddBehavioursAtRuntime");
        
        _displayNamesProperty = currentSerializedObject.FindProperty("DisplayNames");
        _descriptionProperty = currentSerializedObject.FindProperty("Description");
        
        _guidProperty = currentSerializedObject.FindProperty("Guid");
        _rootGuidProperty = currentSerializedObject.FindProperty("RootGuid");
        _configBlocklyProperty = currentSerializedObject.FindProperty("ConfigBlockly");
        _configAssetBundleProperty = currentSerializedObject.FindProperty("ConfigAssetBundle");
        _changelogProperty = currentSerializedObject.FindProperty("Changelog");
        
        _prefabProperty = currentSerializedObject.FindProperty("Prefab");
        _prefabGuidProperty = currentSerializedObject.FindProperty("PrefabGuid");
        
        _authorNameProperty = currentSerializedObject.FindProperty("AuthorName");
        _authorEmailProperty = currentSerializedObject.FindProperty("AuthorEmail");
        _authorUrlProperty = currentSerializedObject.FindProperty("AuthorUrl");

        _licenseCodeProperty = currentSerializedObject.FindProperty("LicenseCode");
        _licenseVersionProperty = currentSerializedObject.FindProperty("LicenseVersion");
        _assetBundlePartProperty = currentSerializedObject.FindProperty("AssetBundleParts");

        _builtAtProperty = currentSerializedObject.FindProperty("BuiltAt");

        _lockedProperty = currentSerializedObject.FindProperty("Locked");
        _embeddedProperty = currentSerializedObject.FindProperty("Embedded");
    }
    
    private void InitializeDisplayName()
    {
        var localizationStringsProperty = _displayNamesProperty.FindPropertyRelative("LocalizationStrings");
        
        if (localizationStringsProperty.arraySize == 0)
        {
            List<LocalizationString> localizationStrings = null;
            var varwinObject = _varwinObjectDescriptor.GetComponentInChildren<VarwinObject>();
            if (varwinObject)
            { 
                localizationStrings = LocalizationUtils.GetLocalizationStrings(varwinObject);
            }
            
            if (localizationStrings == null || localizationStrings.Count == 0)
            {
                IWrapperAware iWrapperAware = _varwinObjectDescriptor.gameObject.GetComponents<IWrapperAware>().FirstOrDefault(x => !(x is VarwinObjectDescriptor || x is VarwinObject));

                if (iWrapperAware != null)
                {
                    localizationStrings = LocalizationUtils.GetLocalizationStrings(iWrapperAware.GetType());
                }
            }

            if (localizationStrings != null)
            {
                for (int i = 0; i < localizationStrings.Count; ++i)
                {
                    localizationStringsProperty.arraySize++;
                    var element = localizationStringsProperty.GetArrayElementAtIndex(i);
                    var key = element.FindPropertyRelative("key");
                    var value = element.FindPropertyRelative("value");
                    int keyIndex = (int) localizationStrings[i].key;
                    if (keyIndex >= 18)
                    {
                        keyIndex++;
                    }
                    key.enumValueIndex = keyIndex;
                    value.stringValue = localizationStrings[i].value;
                }
            }
        }

        string objectName = string.IsNullOrEmpty(_nameProperty?.stringValue) ? _gameObject.name : _nameProperty?.stringValue;

        if (localizationStringsProperty.arraySize == 0 && !string.IsNullOrEmpty(objectName))
        {
            localizationStringsProperty.arraySize++;
            var element = localizationStringsProperty.GetArrayElementAtIndex(0);
            var key = element.FindPropertyRelative("key");
            var value = element.FindPropertyRelative("value");
            key.enumValueIndex = (int) SystemLanguage.English;
            value.stringValue = ObjectHelper.ConvertToNiceName(objectName);
        }

        for (int i = 0; i < localizationStringsProperty.arraySize; ++i)
        {
            var element = localizationStringsProperty.GetArrayElementAtIndex(i);
            var value = element.FindPropertyRelative("value");
            value.stringValue = value.stringValue.Trim();
        }

        _displayNamesProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private void InitializeDescription()
    {
        var localizationStringsProperty = _descriptionProperty.FindPropertyRelative("LocalizationStrings");
        
        if (localizationStringsProperty.arraySize == 0)
        {
            List<LocalizationString> localizationStrings = null;
            var varwinObject = _varwinObjectDescriptor.GetComponentInChildren<VarwinObject>();
            if (varwinObject)
            { 
                localizationStrings = LocalizationUtils.GetLocalizationStrings(varwinObject);
            }
            
            if (localizationStrings == null || localizationStrings.Count == 0)
            {
                IWrapperAware iWrapperAware = _varwinObjectDescriptor.gameObject.GetComponents<IWrapperAware>()
                    .FirstOrDefault(x => !(x is VarwinObjectDescriptor || x is VarwinObject));

                if (iWrapperAware != null)
                {
                    localizationStrings = LocalizationUtils.GetLocalizationStrings(iWrapperAware.GetType());
                }
            }

            if (localizationStrings != null)
            {
                for (int i = 0; i < localizationStrings.Count; ++i)
                {
                    localizationStringsProperty.arraySize++;
                    var element = localizationStringsProperty.GetArrayElementAtIndex(i);
                    var key = element.FindPropertyRelative("key");
                    var value = element.FindPropertyRelative("value");
                    int keyIndex = (int) localizationStrings[i].key;
                    if (keyIndex >= 18)
                    {
                        keyIndex++;
                    }
                    key.enumValueIndex = keyIndex;
                    value.stringValue = localizationStrings[i].value;
                }
            }
        }

        string objectDescription = string.IsNullOrEmpty(_nameProperty?.stringValue) ? _gameObject.name : _nameProperty?.stringValue;

        if (localizationStringsProperty.arraySize == 0 && !string.IsNullOrEmpty(objectDescription))
        {
            localizationStringsProperty.arraySize++;
            var element = localizationStringsProperty.GetArrayElementAtIndex(0);
            var key = element.FindPropertyRelative("key");
            var value = element.FindPropertyRelative("value");
            key.enumValueIndex = (int) SystemLanguage.English;
            value.stringValue = ObjectHelper.ConvertToNiceName(objectDescription) + " description";
        }

        for (int i = 0; i < localizationStringsProperty.arraySize; ++i)
        {
            var element = localizationStringsProperty.GetArrayElementAtIndex(i);
            var value = element.FindPropertyRelative("value");
            value.stringValue = value.stringValue.Trim();
        }

        _descriptionProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
    
    private void InitializePrefabInfo()
    {
        GameObject prefab = CreateObjectUtils.GetPrefabObject(_gameObject);
        string prefabPath = CreateObjectUtils.GetPrefabPath(_gameObject);

        if (prefab && !string.IsNullOrEmpty(prefabPath))
        {
            string oldPrefabPath = _prefabProperty.stringValue;
            string oldPrefabGuid = _prefabGuidProperty.stringValue;
            string newPrefabGuid = AssetDatabase.AssetPathToGUID(_prefabProperty.stringValue);

            if (!string.Equals(prefabPath, oldPrefabPath, StringComparison.Ordinal) || !string.Equals(newPrefabGuid, oldPrefabGuid, StringComparison.Ordinal))
            {
                _prefabProperty.stringValue = prefabPath;
                _prefabGuidProperty.stringValue = oldPrefabGuid;
            }
        }
    }

    private void InitializeDefaultData()
    {
        if (string.IsNullOrEmpty(_rootGuidProperty?.stringValue))
        {
            _rootGuidProperty.stringValue = Guid.NewGuid().ToString();
        }

        if (string.IsNullOrEmpty(_guidProperty?.stringValue))
        {
            _guidProperty.stringValue = _rootGuidProperty.stringValue;
        }

        string objectName = string.IsNullOrEmpty(_nameProperty?.stringValue) ? _gameObject.name : _nameProperty?.stringValue;

        if (!string.IsNullOrEmpty(objectName))
        {
            if (_varwinObjectDescriptor.DisplayNames.Count == 0)
            {
                _varwinObjectDescriptor.DisplayNames.Add(Language.English, ObjectHelper.ConvertToNiceName(objectName));
            }

            if (_varwinObjectDescriptor.Description.Count == 0)
            {
                _varwinObjectDescriptor.Description.Add(Language.English, ObjectHelper.ConvertToNiceName(objectName) + " description");
            }
        }

        if (string.IsNullOrEmpty(_nameProperty.stringValue))
        {
            _nameProperty.stringValue = ObjectHelper.ConvertToClassName(ObjectHelper.ConvertToNiceName(objectName));
        }

        if (string.IsNullOrEmpty(_authorNameProperty.stringValue) && string.IsNullOrEmpty(_authorEmailProperty.stringValue) && string.IsNullOrEmpty(_authorUrlProperty.stringValue))
        {
            AuthorSettings.Initialize();
            _authorNameProperty.stringValue = AuthorSettings.Name;
            _authorEmailProperty.stringValue = AuthorSettings.Email;
            _authorUrlProperty.stringValue = AuthorSettings.Url;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawNameField();

        DrawDisplayName();
        DrawDescription();
        
        DrawPrefabField();

        EditorGUILayout.PropertyField(_iconProperty);

        if (SdkSettings.Features.DeveloperMode)
        {
            EditorGUILayout.PropertyField(_viewImageProperty, new GUIContent("View"));            
            EditorGUILayout.PropertyField(_thumbnailImageProperty, new GUIContent("Thumbnail"));
            EditorGUILayout.PropertyField(_spritesheetImageProperty, new GUIContent("Spritesheet"));
        }
        
        EditorGUILayout.PropertyField(_assetBundlePartProperty, new GUIContent("AssetBundle Parts"), true);

        DrawSourcesIncludedField();
        DrawMobileReadyField();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_disableSceneLogic);
        EditorGUILayout.PropertyField(_addBehavioursAtRuntime);
        
        EditorGUILayout.Space();
        DrawAuthorSettings();
        EditorGUILayout.Space();
        DrawLicenseSettings();
        EditorGUILayout.Space();
        if (_varwinObjectDescriptor.CurrentVersionWasBuilt)
        {
            DrawBuildInfo();
            EditorGUILayout.Space();
        }

        bool isCompiling = EditorApplication.isCompiling;
        if (isCompiling != _lastUnityCompilingStatus)
        {
            UpdateGlobalBehaviourStatus();
        }
        _lastUnityCompilingStatus = isCompiling;

        var buildingInProgress = VarwinBuilderWindow.Instance && !VarwinBuilderWindow.Instance.IsFinished;
        if (isCompiling || buildingInProgress)
        {
            EditorGUILayout.HelpBox(SdkTexts.UnityCompiling, MessageType.Info);
            EditorGUILayout.Space();
            return;
        }

        if (EditorUtility.scriptCompilationFailed)
        {
            EditorGUILayout.HelpBox(SdkTexts.ScriptCompilationFailed, MessageType.Info);
            EditorGUILayout.Space();
            return;
        }

        if (Application.unityVersion != VarwinVersionInfo.RequiredUnityVersion)
        {
            EditorGUILayout.HelpBox(
                string.Format(SdkTexts.WrongUnityVersionBuildMessage, Application.unityVersion, VarwinVersionInfo.RequiredUnityVersion),
                MessageType.Error
            );

            EditorGUILayout.Space();
            return;
        }

        if (_varwinObjectDescriptor.Signatures != null)
        {
            if (_changedSignatures == null)
            {
                UpdateChangedSignatures();
            }

            if (_changedSignatures.Count > 0)
            {
                DrawSignatureWarning();
            }
        }

        DrawStandardShaderWarning();
        DrawLinuxBuildWarning();
        DrawBuildButton();
        DrawPossibleDependencyOnOtherObjectWarning();

        if (SdkSettings.Features.DeveloperMode.Enabled)
        {
            EditorGUILayout.Space();
            DrawDebugInfo();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPossibleDependencyOnOtherObjectWarning()
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            return;
        }

        var prefabPath = _prefabProperty.stringValue;

        if (string.IsNullOrWhiteSpace(prefabPath))
        {
            return;
        }
        
        var asmdefPath = Path.Combine(Path.GetDirectoryName(prefabPath), $"{Path.GetFileNameWithoutExtension(prefabPath)}.asmdef");
        var asmdefData = AsmdefUtils.LoadAsmdefData(asmdefPath);

        var hasDependencies = false;
        foreach (var reference in asmdefData.references)
        {
            var referenceFile = AsmdefUtils.FindAsmdefByName(reference);

            if (referenceFile == null)
            {
                continue;
            }
            
            if (File.Exists(Path.Combine(referenceFile.DirectoryName, $"{Path.GetFileNameWithoutExtension(referenceFile.Name)}.prefab")))
            {
                hasDependencies = true;
                break;
            }
        }

        if (!hasDependencies)
        {
            return;
        }
        
        var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox)
        {
            border = new RectOffset(), padding = new RectOffset(0,0,2,2), normal = {background = null}
        };

        var textStyle = new GUIStyle()
        {
            fontSize = helpBoxLabelStyle.fontSize, normal = helpBoxLabelStyle.normal, wordWrap = true, alignment = TextAnchor.MiddleLeft, stretchHeight = true
        };

        EditorGUILayout.BeginHorizontal(helpBoxLabelStyle);
        GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon").image);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.TextArea(SdkTexts.PossibleDependencyOnOtherWarning, textStyle);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawLinuxBuildWarning()
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            return;
        }

        var documentationLink = @"https://docs.unity3d.com/2021.1/Documentation/Manual/GettingStartedAddingEditorComponents.html";
        if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64) || !SdkSettings.Features.Linux.Enabled)
        {
            return;
        }

        var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox)
        {
            border = new RectOffset(), padding = new RectOffset(0,0,2,2), normal = {background = null}
        };

        var textStyle = new GUIStyle()
        {
            fontSize = helpBoxLabelStyle.fontSize, normal = helpBoxLabelStyle.normal, wordWrap = true, alignment = TextAnchor.MiddleLeft, stretchHeight = true
        };

        var linkStyle = new GUIStyle(VarwinStyles.LinkStyle)
        {
            fontSize = helpBoxLabelStyle.fontSize, wordWrap = true, margin = new RectOffset(), padding = new RectOffset()
        };

        EditorGUILayout.BeginHorizontal(helpBoxLabelStyle);
        GUILayout.Label(EditorGUIUtility.IconContent("console.warnicon").image);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.TextArea(SdkTexts.LinuxModulesMissingWarning, textStyle);
        if (GUILayout.Button(documentationLink, linkStyle))
        {
            Application.OpenURL(documentationLink);
        }
        
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawStandardShaderWarning()
    {
        if (serializedObject.isEditingMultipleObjects)
        {
            return;
        }
        
        var gameObject = ((VarwinObjectDescriptor) serializedObject.targetObject).gameObject;
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        var materials = renderers.Select(a => a.sharedMaterials);
        var isUsedStandardShader = materials.Any(renderMaterials => renderMaterials?.Any(material=> material && material.shader && material.shader.name == "Standard") ?? false);

        if (isUsedStandardShader)
        {
            var guiContent = new GUIContent
            {
                image = EditorGUIUtility.IconContent("console.warnicon").image,
                text = SdkTexts.StandardShaderUsedWarning
            };

            var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox)
            {
                border = new RectOffset(), padding = new RectOffset(), normal = {background = null}
            };

            EditorGUILayout.LabelField(guiContent, helpBoxLabelStyle);
        }
    }

    private void DrawSignatureWarning()
    {
        bool moreThanOne = _changedSignatures.Count > 1;
            
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        var guiContent = new GUIContent
        {
            image = EditorGUIUtility.IconContent("console.warnicon").image,
            text = moreThanOne
                ? string.Format(SdkTexts.SignaturesAreChanged, _changedSignatures.Count)
                : SignatureUtils.MakeSignatureWarning(_changedSignatures.First())
        };

        var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox)
        {
            border = new RectOffset(), padding = new RectOffset(), normal = {background = null}
        };

        EditorGUILayout.LabelField(guiContent, helpBoxLabelStyle);

        EditorGUILayout.BeginVertical();

        var helpBoxButtonStyle = new GUIStyle(GUI.skin.button)
        {
            margin = new RectOffset(10, 10, 10, 10)
        };
        
        SignatureDiffWindow.SetDiffs(_changedSignatures);

        if (moreThanOne)
        {
            if (GUILayout.Button("Show all", helpBoxButtonStyle))
            {
                SignatureDiffWindow.OpenWindow();
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
            
        EditorGUILayout.Space();
    }

    private void DrawNameField()
    {
        EditorGUILayout.PropertyField(_nameProperty);
        
        _objectNameIsValid = false;
        
        if (string.IsNullOrEmpty(_nameProperty.stringValue))
        {
            EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameEmptyWarning, MessageType.Error);
        }
        else if (!ObjectHelper.IsValidTypeName(_nameProperty.stringValue))
        {
            EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameUnavailableSymbolsWarning, MessageType.Error);
        }
        else
        {
            _objectNameIsValid = true;
        }
    }
    
    private void DrawDisplayName()
    {
        EditorGUI.BeginDisabledGroup(Selection.objects.Length > 1);

        SerializedProperty actualDisplayNamesProperty = serializedObject.isEditingMultipleObjects
            ? new SerializedObject(serializedObject.targetObject).FindProperty("DisplayNames")
            : _displayNamesProperty;
        
        EditorGUILayout.PropertyField(actualDisplayNamesProperty);
        
        EditorGUI.EndDisabledGroup();
    }

    private void DrawDescription()
    {
        EditorGUI.BeginDisabledGroup(Selection.objects.Length > 1);

        SerializedProperty actualDescriptionProperty = serializedObject.isEditingMultipleObjects
            ? new SerializedObject(serializedObject.targetObject).FindProperty("Description")
            : _descriptionProperty;
        
        EditorGUILayout.PropertyField(actualDescriptionProperty);
        
        EditorGUI.EndDisabledGroup();
    }
    
    private void DrawPrefabField()
    {
        EditorGUI.BeginDisabledGroup(true);
        
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabProperty.stringValue);
        EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
            
        EditorGUI.EndDisabledGroup();
    }

    private void DrawSourcesIncludedField()
    {
        bool sourcesIncludedChangingIsAvailable = CreateObjectUtils.GetPrefabObject(_gameObject);
        
        
        EditorGUI.BeginDisabledGroup(!sourcesIncludedChangingIsAvailable);
        
        EditorGUILayout.PropertyField(_sourcesIncludedProperty);
        
        EditorGUI.EndDisabledGroup();

        if (!sourcesIncludedChangingIsAvailable)
        {
            _sourcesIncludedProperty.boolValue = true;
        }
    }

    private void DrawMobileReadyField()
    {
        if (SdkSettings.Features.Mobile is { Enabled: true })
        {
            EditorGUILayout.PropertyField(_mobileReadyProperty);
            if (_mobileReadyProperty.boolValue)
            {
                if (!AndroidPlatformHelper.IsAndroidPlatformInstalled)
                {
                    EditorGUILayout.HelpBox("Android platform not installed", MessageType.Error);
                    if (GUILayout.Button("Install android module (from unity hub)"))
                    {
                        AndroidPlatformHelper.InstallAndroidPlatformFromHub();
                    }

                    if (GUILayout.Button("Open setup android module page"))
                    {
                        AndroidPlatformHelper.OpenSetupAndroidPage();
                    }
                }
            }
        }
    }

    private void DrawDebugInfo()
    {
        _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug Info");
        if (_showDebug)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.PropertyField(_guidProperty);
            EditorGUILayout.PropertyField(_rootGuidProperty);
            EditorGUILayout.PropertyField(_configBlocklyProperty);
            EditorGUILayout.PropertyField(_configAssetBundleProperty);
            EditorGUILayout.PropertyField(_authorNameProperty);
            EditorGUILayout.PropertyField(_authorEmailProperty);
            EditorGUILayout.PropertyField(_authorUrlProperty);
            EditorGUILayout.PropertyField(_licenseCodeProperty);
            EditorGUILayout.PropertyField(_licenseVersionProperty);

            EditorGUILayout.PropertyField(_embeddedProperty);
            EditorGUILayout.PropertyField(_lockedProperty);
            
            EditorGUILayout.PropertyField(_changelogProperty);
            
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawAuthorSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("Author:", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(_authorNameProperty);
        EditorGUILayout.PropertyField(_authorEmailProperty);
        EditorGUILayout.PropertyField(_authorUrlProperty);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button(SdkTexts.ResetDefaultAuthorSettingsButton))
        {
            AuthorSettings.Initialize();
            if (string.IsNullOrWhiteSpace(AuthorSettings.Name))
            {
                AuthorSettingsWindow.OpenWindow();
            }
            else
            {
                GUI.FocusControl(null);
                _authorNameProperty.stringValue = AuthorSettings.Name;
                _authorEmailProperty.stringValue = AuthorSettings.Email;
                _authorUrlProperty.stringValue = AuthorSettings.Url;
            }
        }

        GUILayout.EndHorizontal();

        DrawAuthorCanNotBeEmptyHelpBox();

        EditorGUILayout.EndVertical();
    }

    private void DrawLicenseSettings()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("License:", EditorStyles.miniBoldLabel);
        
        EditorGUILayout.BeginHorizontal();

        string prevSelectedLicenseCode = _licenseCodeProperty.stringValue;
        
        _selectedLicense = LicenseSettings.Licenses.FirstOrDefault(x => string.Equals(x.Code, _licenseCodeProperty.stringValue));
        if (_selectedLicense == null)
        {
            _selectedLicense = LicenseSettings.Licenses.FirstOrDefault();
            
            _selectedLicenseIndex = 0;
            _selectedLicenseVersionIndex = 0;
        }
        else
        {
            _selectedLicenseIndex = LicenseSettings.Licenses.IndexOf(_selectedLicense);
            _selectedLicenseVersionIndex = Array.IndexOf(_selectedLicense.Versions, _licenseVersionProperty.stringValue);
        }

        var licenseNames = LicenseSettings.Licenses.Select(license => license.Name).ToArray();
        _selectedLicenseIndex = EditorGUILayout.Popup(_selectedLicenseIndex, licenseNames);

        _selectedLicense = LicenseSettings.Licenses.ElementAt(_selectedLicenseIndex);
        _licenseCodeProperty.stringValue = _selectedLicense.Code;

        if (!string.Equals(prevSelectedLicenseCode, _licenseCodeProperty.stringValue))
        {
            _selectedLicenseVersionIndex = 0;
        }

        if (_selectedLicense.Versions != null && _selectedLicense.Versions.Length > 0)
        {
            if (_selectedLicenseVersionIndex >= _selectedLicense.Versions.Length || _selectedLicenseVersionIndex < 0)
            {
                _selectedLicenseVersionIndex = 0;
            }
            
            _selectedLicenseVersionIndex = EditorGUILayout.Popup(_selectedLicenseVersionIndex, _selectedLicense.Versions, GUILayout.Width(80));
            _licenseVersionProperty.stringValue = _selectedLicense.Versions[_selectedLicenseVersionIndex];
        }
        else
        {
            _licenseVersionProperty.stringValue = string.Empty;
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (_selectedLicense == null)
        {
            VarwinStyles.Link(LicenseSettings.CreativeCommonsLink);
        }
        else
        {
            VarwinStyles.Link(_selectedLicense.GetLink(_licenseVersionProperty.stringValue));
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBuildInfo()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        GUILayout.Label("Build Info:", EditorStyles.miniBoldLabel);

        GUILayout.Label($"Built at {_builtAtProperty.stringValue}", EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();
    }

    private void DrawBuildButton()
    {
        if (Application.isPlaying)
        {
            EditorGUI.BeginDisabledGroup(true);
            GUILayout.Button("Build");
            EditorGUI.EndDisabledGroup();
            return;
        }

        EditorGUI.BeginDisabledGroup(!IsAvailableForBuild());

        if (GUILayout.Button("Build"))
        {
            if (SdkSettings.Features.Changelog || EditorUtility.DisplayDialog(SdkTexts.VarwinObjectBuildDialogTitle, SdkTexts.EditorWillProceed, "Yes", "Cancel"))
            {
                Build();
            }
        }

        if (SdkSettings.Features.DeveloperMode &&  GUILayout.Button("Logic Only Build"))
        {
            if (SdkSettings.Features.Changelog || EditorUtility.DisplayDialog(SdkTexts.VarwinObjectLogicBuildDialogTitle, SdkTexts.EditorLogicWillProceed, "Yes", "Cancel"))
            {
                BuildLogicOnly();
            }
        }
        
        EditorGUI.EndDisabledGroup();

        if (!VarwinVersionInfo.Exists)
        {
            EditorGUILayout.HelpBox(SdkTexts.VersionNotFoundMessage, MessageType.Error);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(_nameProperty?.stringValue))
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectTypeNameEmpty, MessageType.Error);
            }
        }
        catch
        {
            // ignored
        }

        if (_containsGlobalScripts)
        {
            DrawGlobalScriptsHelpBox();
        }
        
        DrawSelectableHelpBox();

        if (CreateObjectUtils.ContainsObjectIdDuplicates(_gameObject))
        {
            EditorGUILayout.HelpBox(SdkTexts.DuplicateObjectIds, MessageType.Warning);
        }
    }

    private void DrawGlobalScriptsHelpBox()
    {
        if (string.IsNullOrEmpty(_firstGlobalScript) || _firstGlobalScript == null)
        {
            return;
        }

        string message = string.Format(SdkTexts.ScriptWithoutAsmdefFormat, _firstGlobalScript);
            
        if (_globalScriptFolderIsRoot)
        {
            message = string.Format(SdkTexts.ScriptWithoutAsmdefInAssetsFormat, _firstGlobalScript);

            if (_scriptsFolderContainsAsmdef)
            {
                message = string.Format(SdkTexts.ScriptWithoutAsmdefInAssetsWithScriptsAsmdefFormat, _firstGlobalScript);
            }
        }

        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        var guiContent = new GUIContent
        {
            image = EditorGUIUtility.IconContent("console.erroricon").image,
            text = message
        };

        var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox) {border = new RectOffset(), padding = new RectOffset(), normal = {background = null}};

        EditorGUILayout.LabelField(guiContent, helpBoxLabelStyle);

        EditorGUILayout.BeginVertical();

        var helpBoxButtonStyle = new GUIStyle(GUI.skin.button) {margin = new RectOffset(10, 10, 10, 10)};

        if (_globalScriptFolderIsRoot)
        {
            string newScriptPath = $"Assets/Scripts/{Path.GetFileName(_firstGlobalScript)}";
            bool disabledAction = File.Exists(newScriptPath);
                
            EditorGUI.BeginDisabledGroup(disabledAction);

            string title = "Move script and create Assembly Definition";
            if (_scriptsFolderContainsAsmdef)
            {
                title = "Move script";
            }
                
            if (GUILayout.Button("Move script", helpBoxButtonStyle))
            {
                string dialogMessage = string.Format(SdkTexts.MoveScriptAndCreateAssemblyDefinitionQuestionFormat, _firstGlobalScript, "Assets/Scripts");

                if (_scriptsFolderContainsAsmdef)
                {
                    dialogMessage = string.Format(SdkTexts.MoveScriptQuestionFormat, _firstGlobalScript, "Assets/Scripts");
                }
                    
                if (EditorUtility.DisplayDialog(title, dialogMessage, "Yes", "Cancel"))
                {
                    if (!Directory.Exists("Assets/Scripts"))
                    {
                        Directory.CreateDirectory("Assets/Scripts");
                    }
                        
                    File.Move(_firstGlobalScript, newScriptPath);
                    if (File.Exists(_firstGlobalScript + ".meta"))
                    {
                        File.Move(_firstGlobalScript + ".meta", newScriptPath + ".meta");
                    }

                    if (!_scriptsFolderContainsAsmdef)
                    {
                        VarwinAssemblyDefinition.Create("Assets/Scripts");
                    }
                }
            }
                
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            if (GUILayout.Button("Create Assembly Definition", helpBoxButtonStyle))
            {
                VarwinAssemblyDefinition.Create(_firstGlobalScript);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSelectableHelpBox()
    {
        if (IsSelectable())
        {
            return;
        }

        var rigidbodies = _gameObject.GetComponentsInChildren<Rigidbody>();
        var colliders = _gameObject.GetComponentsInChildren<Collider>();

        var rigidbody = _gameObject.GetComponent<Rigidbody>();
        var collider = _gameObject.GetComponent<Collider>();

        bool requireRigidbody = false;
        bool requireBoxCollider = false;
            
        if (rigidbodies.Length == 0 || !rigidbody && colliders.Length > 0)
        {
            requireRigidbody = true;
        }
            
        if (colliders.Length == 0 || !collider && rigidbodies.Length > 0)
        {
            requireBoxCollider = true;
        }
            
        string message = SdkTexts.ObjectNotSelectable;
            
        if (requireRigidbody && !requireBoxCollider)
        {
            message = SdkTexts.ObjectNotSelectableRigidbody;
        }
            
        if (requireBoxCollider && !requireRigidbody)
        {
            message = SdkTexts.ObjectNotSelectableCollider;
        }
            
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

        var guiContent = new GUIContent
        {
            image = EditorGUIUtility.IconContent("console.erroricon").image,
            text = message
        };
            
        var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox);
        helpBoxLabelStyle.border = new RectOffset();
        helpBoxLabelStyle.padding = new RectOffset();
        helpBoxLabelStyle.normal.background = null;

        EditorGUILayout.LabelField(guiContent, helpBoxLabelStyle);

        EditorGUILayout.BeginVertical();
                
        var helpBoxButtonStyle = new GUIStyle(GUI.skin.button);
        helpBoxButtonStyle.margin = new RectOffset(10, 10, 10, 10);

        if (requireRigidbody && requireBoxCollider)
        {
            if (GUILayout.Button("Add components", helpBoxButtonStyle))
            {
                _gameObject.AddComponent<Rigidbody>();
                ColliderUtils.SetupBoxColliderByBounds(_gameObject.AddComponent<BoxCollider>());
            }
        }
        else if (requireRigidbody)
        {
            if (GUILayout.Button("Add Rigidbody", helpBoxButtonStyle))
            {
                _gameObject.AddComponent<Rigidbody>();
            }
        }
        else if (requireBoxCollider)
        {
            if (GUILayout.Button("Add BoxCollider", helpBoxButtonStyle))
            {
                ColliderUtils.SetupBoxColliderByBounds(_gameObject.AddComponent<BoxCollider>());
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
    
    public bool IsAvailableForBuild()
    {
        return IsSelectable()
               && !string.IsNullOrWhiteSpace(_nameProperty.stringValue)
               && !string.IsNullOrWhiteSpace(_authorNameProperty.stringValue)
               && VarwinVersionInfo.Exists
               && _objectNameIsValid
               && _displayNamesProperty.FindPropertyRelative("IsValidDictionary").boolValue
               && _descriptionProperty.FindPropertyRelative("IsValidDictionary").boolValue
               && !_containsGlobalScripts;
    }

    private void Build()
    {
        if (CreateObjectUtils.ContainsObjectIdDuplicates(_gameObject))
        {
            if (!DisplayDuplicatesObjectIdsDialog())
            {
                return;
            }
        }
        
        VarwinBuilderController.BuildSelectedObjects();
    }

    private void BuildLogicOnly()
    {
        if (CreateObjectUtils.ContainsObjectIdDuplicates(_gameObject))
        {
            if (!DisplayDuplicatesObjectIdsDialog())
            {
                return;
            }
        }

        VarwinBuilderController.BuildSelectedObjectsLogicOnly();
    }

    private bool DisplayDuplicatesObjectIdsDialog()
    {
        if (EditorUtility.DisplayDialog(SdkTexts.VarwinObjectBuildDialogTitle, SdkTexts.DuplicateObjectIdsHelp, "OK", "Cancel"))
        {
            var duplicates = CreateObjectUtils.GetObjectIdDuplicates(_gameObject);
            var rootObjectId = _gameObject.GetComponent<ObjectId>();

            if (rootObjectId)
            {
                if (duplicates.Contains(rootObjectId))
                {
                    duplicates.Remove(rootObjectId);
                }
            }

            foreach (var duplicate in duplicates)
            {
                DestroyImmediate(duplicate);
            }

            return true;
        }
        return false;
    }

    private bool IsSelectable()
    {
        var rbs = _gameObject.GetComponentsInChildren<Rigidbody>();
        var cols = _gameObject.GetComponentsInChildren<Collider>();
        if (rbs.Length > 0 && cols.Length > 0)
        {
            return _gameObject.GetComponent<Rigidbody>() != null || _gameObject.GetComponent<Collider>() != null;
        }
        return false;
    }

    private void DrawAuthorCanNotBeEmptyHelpBox()
    {
        if (string.IsNullOrWhiteSpace(_authorNameProperty.stringValue))
        {
            EditorGUILayout.HelpBox(SdkTexts.AuthorNameEmptyWarning, MessageType.Error);
        }
    }

    public void BuildVarwinObject(VarwinObjectDescriptor varwinObjectDescriptor)
    {
        varwinObjectDescriptor.PreBuild();

        if (CreateObjectUtils.GetPrefabObject(varwinObjectDescriptor.gameObject) && !CreateObjectUtils.IsPrefabAsset(varwinObjectDescriptor.gameObject))
        {
            CreateObjectUtils.ApplyPrefabInstanceChanges(varwinObjectDescriptor.gameObject);
        }

        VarwinBuilderController.Build(varwinObjectDescriptor);
    }

    [MenuItem("CONTEXT/VarwinObjectDescriptor/Set as new Varwin-object")]
    public static void SetAsNewVarwinObject(MenuCommand command)
    {
        var varwinObjectDescriptor = (VarwinObjectDescriptor) command.context;
        
        varwinObjectDescriptor.RegenerateGuid();
        varwinObjectDescriptor.CleanVarwinObjectInfo();
        varwinObjectDescriptor.CleanBuiltInfo();
    }
}

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using Varwin.Editor;
using Varwin.Public;

[CustomEditor(typeof(VarwinPackageDescriptor))]
[CanEditMultipleObjects]
public class VarwinPackageDescriptorEditor : Editor
{
    private VarwinPackageDescriptor _varwinPackageDescriptor;
    
    private SerializedProperty _viewProperty;
    private SerializedProperty _thumbnailProperty;
    
    private SerializedProperty _viewImageProperty;
    private SerializedProperty _thumbnailImageProperty;
    
    private SerializedProperty _localizedNameProperty;
    private SerializedProperty _localizedDescriptionProperty;
    
    private SerializedProperty _guidProperty;
    private SerializedProperty _rootGuidProperty;
    
    private SerializedProperty _authorNameProperty;
    private SerializedProperty _authorEmailProperty;
    private SerializedProperty _authorUrlProperty;

    private SerializedProperty _licenseCodeProperty;
    private SerializedProperty _licenseVersionProperty;
    private LicenseType _selectedLicense;
    private int _selectedLicenseIndex = -1;
    private int _selectedLicenseVersionIndex = -1;
    
    private SerializedProperty _builtAtProperty;
    private SerializedProperty _currentVersionWasBuiltProperty;
    
    private SerializedProperty _objectsProperty;
    private ReorderableList _objectsList;
    
    private SerializedProperty _sceneTemplatesProperty;
    private ReorderableList _sceneTemplatesList;
    
    private SerializedProperty _resourcesProperty;
    private ReorderableList _resourcesList;

    private bool _showDebug;
    
    private void Awake()
    {
        OnEnable();
    }

    private void OnEnable()
    {
        if (!target)
        {
            return;
        }
        
        _varwinPackageDescriptor = (VarwinPackageDescriptor) target;

        InitializeProperties();

        bool needApply = InitializeLocalizedName() || InitializeLocalizedDescription();
        if (needApply)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        if (string.IsNullOrEmpty(_guidProperty.stringValue))
        {
            InitializeDefaultData();
        }

        TryMigrateImages();
    }

    private void TryMigrateImages()
    {
        foreach (VarwinPackageDescriptor packageDescriptor in serializedObject.targetObjects)
        {
            if (packageDescriptor.Thumbnail)
            {
                packageDescriptor.ThumbnailImage = new LocalizedDictionary<Texture2D>();
                packageDescriptor.ThumbnailImage.SetLocale(Varwin.Language.English, packageDescriptor.Thumbnail);
                packageDescriptor.Thumbnail = null;
            }
            
            if (packageDescriptor.View)
            {
                packageDescriptor.ViewImage = new LocalizedDictionary<Texture2D>();
                packageDescriptor.ViewImage.SetLocale(Varwin.Language.English, packageDescriptor.View);
                packageDescriptor.View = null;
            }
            
            EditorUtility.SetDirty(packageDescriptor);
            AssetDatabase.SaveAssetIfDirty(packageDescriptor);
        }
    }

    private void InitializeProperties()
    {
        _localizedNameProperty = serializedObject.FindProperty("Name");
        _localizedDescriptionProperty = serializedObject.FindProperty("Description");

        _viewProperty = serializedObject.FindProperty("View");
        _thumbnailProperty = serializedObject.FindProperty("Thumbnail");
        
        _viewImageProperty = serializedObject.FindProperty("ViewImage");
        _thumbnailImageProperty = serializedObject.FindProperty("ThumbnailImage");
        
        _guidProperty = serializedObject.FindProperty("Guid");
        _rootGuidProperty = serializedObject.FindProperty("RootGuid");
        
        _authorNameProperty = serializedObject.FindProperty("AuthorName");
        _authorEmailProperty = serializedObject.FindProperty("AuthorEmail");
        _authorUrlProperty = serializedObject.FindProperty("AuthorUrl");

        _licenseCodeProperty = serializedObject.FindProperty("LicenseCode");
        _licenseVersionProperty = serializedObject.FindProperty("LicenseVersion");
        
        _builtAtProperty = serializedObject.FindProperty("BuiltAt");
        _currentVersionWasBuiltProperty = serializedObject.FindProperty("CurrentVersionWasBuilt");

        _objectsProperty = serializedObject.FindProperty("Objects");
        _objectsList = new ReorderableList(serializedObject, _objectsProperty, true, true, true, true);

        _objectsList.drawHeaderCallback += (Rect rect) =>
        {
            GUI.Label(rect, "Objects");
        };
        
        _objectsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _objectsList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };

        _sceneTemplatesProperty = serializedObject.FindProperty("SceneTemplates");
        _sceneTemplatesList = new ReorderableList(serializedObject, _sceneTemplatesProperty, true, true, true, true);

        _sceneTemplatesList.drawHeaderCallback += (Rect rect) =>
        {
            GUI.Label(rect, "Scene Templates");
        };
        
        _sceneTemplatesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _sceneTemplatesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
        
        _resourcesProperty = serializedObject.FindProperty("Resources");
        _resourcesList = new ReorderableList(serializedObject, _resourcesProperty, true, true, true, true);

        _resourcesList.drawHeaderCallback += (Rect rect) =>
        {
            GUI.Label(rect, "Resources");
        };
        
        _resourcesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            SerializedProperty element = _resourcesList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        };
    }
    
    private bool InitializeLocalizedName()
    {
        var localizationStringsProperty = _localizedNameProperty.FindPropertyRelative("LocalizationStrings");
        
        if (localizationStringsProperty.arraySize == 0)
        {
            localizationStringsProperty.arraySize++;
            var element = localizationStringsProperty.GetArrayElementAtIndex(0);
            var key = element.FindPropertyRelative("key");
            var value = element.FindPropertyRelative("value");
            key.enumValueIndex = (int) SystemLanguage.English;
            value.stringValue = ObjectHelper.ConvertToNiceName(_varwinPackageDescriptor.name);

            return true;
        }

        return false;
    }
    
    private bool InitializeLocalizedDescription()
    {
        var localizationStringsProperty = _localizedDescriptionProperty.FindPropertyRelative("LocalizationStrings");
        
        if (localizationStringsProperty.arraySize == 0)
        {
            localizationStringsProperty.arraySize++;
            var element = localizationStringsProperty.GetArrayElementAtIndex(0);
            var key = element.FindPropertyRelative("key");
            var value = element.FindPropertyRelative("value");
            key.enumValueIndex = (int) SystemLanguage.English;
            value.stringValue = string.Empty;

            return true;
        }

        return false;
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

        if (string.IsNullOrEmpty(_authorNameProperty.stringValue) && string.IsNullOrEmpty(_authorEmailProperty.stringValue) && string.IsNullOrEmpty(_authorUrlProperty.stringValue))
        {
            AuthorSettings.Initialize();
            _authorNameProperty.stringValue = AuthorSettings.Name;
            _authorEmailProperty.stringValue = AuthorSettings.Email;
            _authorUrlProperty.stringValue = AuthorSettings.Url;
        }

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawLocalizedName();

        DrawLocalizedDescription();

        EditorGUILayout.PropertyField(_viewImageProperty);
        EditorGUILayout.PropertyField(_thumbnailImageProperty);

        EditorGUILayout.Space();
        DrawAuthorSettings();
        EditorGUILayout.Space();
        DrawLicenseSettings();
        EditorGUILayout.Space();
        _objectsList.DoLayoutList();
        EditorGUILayout.Space();
        _sceneTemplatesList.DoLayoutList();
        EditorGUILayout.Space();
        _resourcesList.DoLayoutList();
        EditorGUILayout.Space();
        
        if (_currentVersionWasBuiltProperty.boolValue)
        {
            DrawBuildInfo();
            EditorGUILayout.Space();
        }

        if (EditorApplication.isCompiling)
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
        
        DrawBuildButton();

        if (SdkSettings.Features.DeveloperMode.Enabled)
        {
            EditorGUILayout.Space();
            DrawDebugInfo();
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLocalizedName()
    {
        EditorGUI.BeginDisabledGroup(Selection.objects.Length > 1);
        EditorGUILayout.PropertyField(_localizedNameProperty);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawLocalizedDescription()
    {
        EditorGUI.BeginDisabledGroup(Selection.objects.Length > 1);
        EditorGUILayout.PropertyField(_localizedDescriptionProperty);
        EditorGUI.EndDisabledGroup();
    }

    private void DrawDebugInfo()
    {
        _showDebug = EditorGUILayout.Foldout(_showDebug, "Debug Info");
        if (_showDebug)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.PropertyField(_guidProperty);
            EditorGUILayout.PropertyField(_rootGuidProperty);
            EditorGUILayout.PropertyField(_authorNameProperty);
            EditorGUILayout.PropertyField(_authorEmailProperty);
            EditorGUILayout.PropertyField(_authorUrlProperty);
            EditorGUILayout.PropertyField(_licenseCodeProperty);
            EditorGUILayout.PropertyField(_licenseVersionProperty);
            
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
            VarwinBuilderController.Build(_varwinPackageDescriptor);
            _currentVersionWasBuiltProperty.boolValue = true;
            return;
        }
        
        EditorGUI.EndDisabledGroup();

        if (!VarwinVersionInfo.Exists)
        {
            EditorGUILayout.HelpBox(SdkTexts.VersionNotFoundMessage, MessageType.Error);
        }
    }

    public bool IsAvailableForBuild()
    {
        return !string.IsNullOrWhiteSpace(_authorNameProperty.stringValue)
               && Selection.objects.Length < 2
               && VarwinVersionInfo.Exists
               && _localizedNameProperty.FindPropertyRelative("IsValidDictionary").boolValue
               && _localizedDescriptionProperty.FindPropertyRelative("IsValidDictionary").boolValue;
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
}

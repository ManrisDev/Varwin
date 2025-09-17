using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class CreateObjectWindow : EditorWindow
    {
        private static Vector2 MinWindowSize => new Vector2(420, 480);
        private static Vector2 MaxWindowSize => new Vector2(640, 800);

        private static Vector2 _scrollPosition;

        private static float _pathButtonWidth = 22;
        
        private static string _objectClassName = "ObjectClassName";
        private static bool _objectTypeNameIsValid = true;

        private static LocalizationDictionary _localizedName;
        private static LocalizationDictionaryDrawer _localizedNameDrawer;

        private static LocalizationDictionary _localizedDescription;
        private static LocalizationDictionaryDrawer _localizedDescriptionDrawer;
        
        private static string _authorName;
        private static string _authorEmail;
        private static string _authorUrl;

        private static string _licenseCode;
        private static string _licenseVersion;
        private LicenseType _selectedLicense;
        private int _selectedLicenseIndex = -1;
        private int _selectedLicenseVersionIndex = -1;

        private string _objectFolderPath;
        private string[] _existingObjectsInObjectFolderPath;

        private static long _createdAt = -1;
        private static bool _mobileReady;

        private static GameObject _gameObject;
        
        private static GameObject _prefab;
        private string _tags;
        
        private static bool _selectedGameObjectIsValid = false;

        private static bool _onlyAddVarwinObjectDescriptor = false;
        
        public static CreateObjectWindow OpenWindow()
        {
            var window = GetWindow<CreateObjectWindow>(true, "Create object", true);
            window.minSize = MinWindowSize;
            window.maxSize = MaxWindowSize;
            window.Show();

            window.Awake();

            return window;
        }

        public void Awake()
        {
            InitializeDisplayName();
            InitializeDescription();

            _objectFolderPath = SdkSettings.ObjectCreationFolderPath;
            if (!Directory.Exists(_objectFolderPath))
            {
                SetupDefaultDirectory();
            }

            UpdateExistingObjectsInObjectFolderPath();
            
            if (Selection.activeGameObject != null)
            {
                _gameObject = Selection.activeGameObject;
                
                string objectTypeName = ObjectHelper.ConvertToNiceName(_gameObject.name);
                _objectClassName = Regex.Replace(objectTypeName, @"\W", "");
                _localizedName.Add(new LocalizationString(Varwin.Language.English, objectTypeName));
                _localizedDescription.Add(new LocalizationString(Varwin.Language.English, objectTypeName ));
                
                if (Application.systemLanguage != SystemLanguage.English)
                {
                    _localizedName.Add(new LocalizationString((Language)Application.systemLanguage, objectTypeName));
                    _localizedDescription.Add(new LocalizationString((Language)Application.systemLanguage, objectTypeName));
                }
            }

            AuthorSettings.Initialize();
            _authorName = AuthorSettings.Name;
            _authorEmail = AuthorSettings.Email;
            _authorUrl = AuthorSettings.Url;
            _mobileReady = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.MobileReady : _mobileReady;
            _scrollPosition = Vector2.zero;
        }

        private void OnEnable()
        {
            Awake();
        }

        private void SetupDefaultDirectory()
        {
            _objectFolderPath = "Assets/Objects";
            if (!Directory.Exists(_objectFolderPath))
            {
                Directory.CreateDirectory(_objectFolderPath);
            }
        }
        
        private static void InitializeDisplayName()
        {
            _localizedName = new LocalizationDictionary();
        
            _localizedNameDrawer = new LocalizationDictionaryDrawer(_localizedName, typeof(LocalizationString), true, false, true, true)
            {
                Title = "Display Name", 
                ErrorMessage = SdkTexts.AnItemWithLanguageHasAlreadyBeenAddedFormat
            };
        
            _localizedNameDrawer.Initialize(_localizedName);
        }
        
        private static void InitializeDescription()
        {
            _localizedDescription = new LocalizationDictionary();

            _localizedDescriptionDrawer = new LocalizationDictionaryDrawer(_localizedDescription, typeof(LocalizationString), true, false, true, true)
            {
                Title = "Description",
                ErrorMessage = SdkTexts.AnItemWithLanguageHasAlreadyBeenAddedFormat
            };

            _localizedDescriptionDrawer.Initialize(_localizedDescription);
        }
        
        private void OnGUI()
        {
            if (File.Exists(CreateObjectTempModel.TempFilePath))
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUILayout.Label(SdkTexts.WaitUntilObjectsCreate, EditorStyles.boldLabel, GUILayout.ExpandHeight(true));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Object Settings", EditorStyles.boldLabel);
            DrawObjectTypeNameField();
            
            EditorGUILayout.Space();
            DrawObjectField();

            EditorGUILayout.Space();
            DrawObjectFolder();
            
            if (SdkSettings.Features.Mobile.Enabled)
            {
                EditorGUILayout.Space();
                DrawPlatformsSettings();
            }
            else
            {
                _mobileReady = false;
            }

            EditorGUILayout.Space();
            DrawDisplayName();
            
            EditorGUILayout.Space();
            DrawDescription();
            
            EditorGUILayout.Space();
            DrawAuthorSettings();
            
            EditorGUILayout.Space();
            DrawLicenseSettings();
            
            EditorGUILayout.Space();
            _tags = EditorGUILayout.TextField("Tags (by \",\")", _tags);

            EditorGUILayout.Space();
            
            _onlyAddVarwinObjectDescriptor = EditorGUILayout.ToggleLeft("Only add Varwin Object Descriptor", _onlyAddVarwinObjectDescriptor);
            
            EditorGUILayout.Space();

            DrawCreateObjectButton();
            
            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        private void DrawObjectTypeNameField()
        {
            _objectClassName = EditorGUILayout.TextField("Object Class Name", _objectClassName);
            
            _objectTypeNameIsValid = false;
            if (string.IsNullOrEmpty(_objectClassName))
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameEmptyWarning, MessageType.Error);
            }
            else if (!ObjectHelper.IsValidTypeName(_objectClassName))
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameUnavailableSymbolsWarning, MessageType.Error);
            }
            else
            {
                _objectTypeNameIsValid = true;
            }
        }

        private void DrawPlatformsSettings()
        {
            _mobileReady = EditorGUILayout.Toggle("Mobile Ready", _mobileReady);
        }

        private void DrawDisplayName()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            
            if (_localizedNameDrawer == null)
            {
                InitializeDisplayName();
            }
            
            _localizedNameDrawer?.Draw();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawDescription()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);

            if (_localizedDescriptionDrawer == null)
            {
                InitializeDescription();
            }
            
            _localizedDescriptionDrawer?.Draw();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawObjectField()
        {
            _gameObject = (GameObject) EditorGUILayout.ObjectField("Game Object",  _gameObject, typeof(GameObject), true);

            _selectedGameObjectIsValid = false;
            if (_gameObject != null)
            {
                var varwinObject = _gameObject.GetComponentsInChildren<VarwinObjectDescriptor>(true);
                if (varwinObject != null && varwinObject.Length > 0)
                {
                    DrawObjectContainsComponentHelpBox<VarwinObjectDescriptor>();
                    return;
                }
                
                var objectIdComponent = _gameObject.GetComponentsInChildren<ObjectId>(true);
                if (objectIdComponent != null && objectIdComponent.Length > 0)
                {
                    DrawObjectContainsComponentHelpBox<ObjectId>();
                    return;
                }
                
                var iWrapperAware = _gameObject.GetComponentsInChildren<IWrapperAware>(true);
                if (iWrapperAware != null && iWrapperAware.Length > 0)
                {
                    DrawObjectContainsComponentHelpBox<IWrapperAware>();
                    return;
                }

                _selectedGameObjectIsValid = true;
            }
            else
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectNullWarning, MessageType.Error);
            }
        }

        private void DrawObjectContainsComponentHelpBox<T>(MessageType messageType = MessageType.Error)
        {
            EditorGUILayout.HelpBox(string.Format(SdkTexts.ObjectContainsComponentWarningFormat, typeof(T).Name), messageType);
        }
        
        private void DrawObjectFolder()
        {
            string prevObjectFolderPath = _objectFolderPath;
            
            Rect controlRect = EditorGUILayout.GetControlRect();
            
            var labelRect = new Rect(controlRect) {size = EditorUtils.CalcSize("Object Folder Path: ", EditorStyles.label)};
            labelRect.width = 150;
            EditorGUI.LabelField(labelRect, "Object Folder Path: ", EditorStyles.label);

            string folderName = _objectClassName.Replace(" ", "").Trim();
            string path = $"{_objectFolderPath}/{folderName}/{folderName}.prefab";
            Vector2 labelSize = EditorUtils.CalcSize(path + " ", EditorStyles.label);
            var pathRect = new Rect(controlRect)
            {
                x = labelRect.x + labelRect.width,
                width = Mathf.Min(position.width - labelRect.width - _pathButtonWidth - 10, labelSize.x),
                height = labelSize.y
            };

            EditorGUI.LabelField(pathRect, path, EditorStyles.label);

            var buttonRect = new Rect(
                Mathf.Min(position.width - _pathButtonWidth - 5, pathRect.x + pathRect.width),
                labelRect.y,
                _pathButtonWidth,
                labelRect.height - 1);
            
            if (GUI.Button(buttonRect, "...", EditorStyles.miniButton))
            {
                _objectFolderPath = EditorUtility.OpenFolderPanel("Object Folder Path", _objectFolderPath, "").Trim().TrimEnd('/');

                if (string.IsNullOrEmpty(_objectFolderPath))
                {
                    _objectFolderPath = prevObjectFolderPath;
                }
                else
                {
                    if (_objectFolderPath.IndexOf(UnityProject.Path, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        _objectFolderPath = _objectFolderPath.Replace(UnityProject.Path, "").TrimStart('/');
                    }
                    else
                    {
                        _objectFolderPath = prevObjectFolderPath;
                        EditorUtility.DisplayDialog("Error!", "Object Folder must be in unity project's Assets folder", "OK");
                    }
                }
            }
            
            if (string.IsNullOrEmpty(_objectFolderPath))
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameEmptyWarning, MessageType.Error);
            }

            if (prevObjectFolderPath != _objectFolderPath)
            {
                UpdateExistingObjectsInObjectFolderPath();
            }
            
            if (_existingObjectsInObjectFolderPath != null && _existingObjectsInObjectFolderPath.Any(x => string.Equals(x, _objectClassName, StringComparison.OrdinalIgnoreCase)))
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameDuplicateWarning, MessageType.Error);
                _objectTypeNameIsValid = false;
            }
        }

        private void UpdateExistingObjectsInObjectFolderPath()
        {
            _existingObjectsInObjectFolderPath = Directory.GetDirectories(_objectFolderPath)
                .Select(x => x.Replace(_objectFolderPath + "\\", "")).ToArray();
        }
        
        private void DrawAuthorSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Author:", EditorStyles.miniBoldLabel);
            _authorName = EditorGUILayout.TextField("Name", _authorName);
            _authorEmail = EditorGUILayout.TextField("E-Mail", _authorEmail);
            _authorUrl = EditorGUILayout.TextField("URL", _authorUrl);

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
                    _authorName = AuthorSettings.Name;
                    _authorEmail = AuthorSettings.Email;
                    _authorUrl = AuthorSettings.Url;
                }
            }

            GUILayout.EndHorizontal();

            DrawAuthorCanNotBeEmptyHelpBox();

            EditorGUILayout.EndVertical();
        }

        private void DrawAuthorCanNotBeEmptyHelpBox()
        {
            if (string.IsNullOrWhiteSpace(_authorName))
            {
                EditorGUILayout.HelpBox(SdkTexts.AuthorNameEmptyWarning, MessageType.Error);
            }
        }

        private void DrawLicenseSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Label("License:", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
    
            string prevSelectedLicenseCode = _licenseCode;
            
            _selectedLicense = LicenseSettings.Licenses.FirstOrDefault(x => string.Equals(x.Code, _licenseCode));
            if (_selectedLicense == null)
            {
                _selectedLicense = LicenseSettings.Licenses.FirstOrDefault();
                
                _selectedLicenseIndex = 0;
                _selectedLicenseVersionIndex = 0;
            }
            else
            {
                _selectedLicenseIndex = LicenseSettings.Licenses.IndexOf(_selectedLicense);
                _selectedLicenseVersionIndex = Array.IndexOf(_selectedLicense.Versions, _licenseVersion);
            }
    
            var licenseNames = LicenseSettings.Licenses.Select(license => license.Name).ToArray();
            _selectedLicenseIndex = EditorGUILayout.Popup(_selectedLicenseIndex, licenseNames);
    
            _selectedLicense = LicenseSettings.Licenses.ElementAt(_selectedLicenseIndex);
            _licenseCode = _selectedLicense.Code;
    
            if (!string.Equals(prevSelectedLicenseCode, _licenseCode))
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
                _licenseVersion = _selectedLicense.Versions[_selectedLicenseVersionIndex];
            }
            else
            {
                _licenseVersion = string.Empty;
            }
            
            EditorGUILayout.EndHorizontal();
            
            if (_selectedLicense == null)
            {
                VarwinStyles.Link(LicenseSettings.CreativeCommonsLink);
            }
            else
            {
                VarwinStyles.Link(_selectedLicense.GetLink(_licenseVersion));
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawCreateObjectButton()
        {
            bool isDisabled = string.IsNullOrWhiteSpace(_authorName) 
                              || !_objectTypeNameIsValid
                              || !_selectedGameObjectIsValid
                              || !_localizedNameDrawer.IsValidDictionary
                              || !_localizedDescriptionDrawer.IsValidDictionary;

            EditorGUI.BeginDisabledGroup(isDisabled);

            if (GUILayout.Button("Create"))
            {
                try
                {
                    if (_onlyAddVarwinObjectDescriptor)
                    {
                        var varwinObjectDescriptor = _gameObject.AddComponent<VarwinObjectDescriptor>();
                        
                        varwinObjectDescriptor.Name = _objectClassName;
                        
                        varwinObjectDescriptor.Guid = Guid.NewGuid().ToString();
                        varwinObjectDescriptor.RootGuid = varwinObjectDescriptor.Guid;
                        
                        varwinObjectDescriptor.DisplayNames = new LocalizationDictionary(_localizedName.LocalizationStrings);
                        varwinObjectDescriptor.Description = new LocalizationDictionary(_localizedDescription.LocalizationStrings);
                        
                        varwinObjectDescriptor.AuthorName = _authorName;
                        varwinObjectDescriptor.AuthorEmail = _authorEmail;
                        varwinObjectDescriptor.AuthorUrl = _authorUrl;

                        varwinObjectDescriptor.LicenseCode = _licenseCode;
                        varwinObjectDescriptor.LicenseVersion = _licenseVersion;

                        varwinObjectDescriptor.AddBehavioursAtRuntime = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.AddBehavioursAtRuntime : varwinObjectDescriptor.AddBehavioursAtRuntime;  
                        varwinObjectDescriptor.MobileReady = SdkSettings.Features.Mobile.Enabled && _mobileReady;
                        varwinObjectDescriptor.SourcesIncluded = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.SourcesIncluded : varwinObjectDescriptor.SourcesIncluded;  
                        varwinObjectDescriptor.DisableSceneLogic = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.DisableSceneLogic : varwinObjectDescriptor.DisableSceneLogic;  
                        
                        Close();
                    }
                    else
                    {
                        Selection.activeObject = null;
                        Create();
                    }
                }
                catch (Exception e)
                {
                    
                    EditorUtility.DisplayDialog("Error!", string.Format(SdkTexts.CannotCreateObjectErrorFormat,e.Message), "OK");
                    Debug.LogException(e);
                    Close();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        [Obsolete("Is it used anywhere?")]
        public void Create(GameObject gameObject, string className)
        {
            _gameObject = gameObject;
            _objectClassName = className;

            if (_localizedName == null)
            {
                _localizedName = new LocalizationDictionary();
            }
            
            _localizedName.Add(new LocalizationString(Language.English, className));
            
            if (_localizedDescription == null)
            {
                _localizedDescription = new LocalizationDictionary();
            }
            
            _localizedDescription.Add(new LocalizationString(Language.English, className + " description"));
            
            Create();
        }

        private void Create()
        {
            if (!_gameObject)
            {
                EditorUtility.DisplayDialog("Error!", SdkTexts.ObjectNullWarning, "OK");
                return;
            }

            if (!ObjectHelper.IsValidTypeName(_objectClassName, true))
            {
                return;
            }

            foreach (var localizationString in _localizedName)
            {
                localizationString.value = ObjectHelper.EscapeString(localizationString.value);
            }
            
            var model = new CreateObjectModel
            {
                Guid = Guid.NewGuid().ToString(),
                ObjectName = _objectClassName.Replace(" ", ""),
                DisplayNames = _localizedName,
                Description = _localizedDescription,
                MobileReady =  _mobileReady
            };
            model.ObjectFolder = $"{_objectFolderPath}/{model.ObjectName}";
            model.PrefabPath = $"{model.ObjectFolder}/{model.ObjectName}.prefab";

            if (Directory.Exists(model.ObjectFolder))
            {
                EditorUtility.DisplayDialog(string.Format(SdkTexts.ObjectWithNameAlreadyExistsFormat, model.ObjectName),
                    SdkTexts.ObjectWithNameAlreadyExists, "OK");
            }
            else
            {
                Debug.Log("Create folder " + model.ObjectFolder);
                Directory.CreateDirectory(model.ObjectFolder);

                Debug.Log("Create prefab " + _gameObject.name);

                CreatePrefab(_gameObject, model.PrefabPath, model.Guid,
                    _objectClassName, null);

                CreateTags(model, _tags);

                model.ClassName = CreateCode(model);

                SerializeObjectModelInfo(model);

                AssetDatabase.Refresh();
            }
        }

        public static void SerializeObjectModelInfo(CreateObjectModel model)
        {
            var modelsList = new List<CreateObjectModel>
            {
                model
            };

            var temp = new CreateObjectTempModel
            {
                Objects = modelsList, 
                BuildNow = false
            };

            string jsonModels = JsonConvert.SerializeObject(temp, Formatting.None, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
                
            File.WriteAllText(CreateObjectTempModel.TempFilePath, jsonModels);
        }

        public static void CreateTags(CreateObjectModel model, string tagString)
        {
            if (string.IsNullOrEmpty(tagString))
            {
                return;
            }

            var tags = tagString.Split(',');
            var tagList = new List<string>();

            foreach (string tag in tags)
            {
                int i = 0;
                tagList.Add(tag[i] == ' ' ? tag.Remove(i, 1) : tag);
            }

            File.WriteAllLines($"{model.ObjectFolder}/tags.txt", tagList);
        }

        public static string CreateCode(CreateObjectModel model)
        {
            string wrapperScript = File.ReadAllText($"{Application.dataPath}/Varwin/Editor/Varwin/Templates/ObjectType.txt");
            string asmdef = File.ReadAllText($"{Application.dataPath}/Varwin/Editor/Varwin/Templates/ObjectAsmdef.txt");

            string cleanGuid = model.Guid.Replace("-", "");

            wrapperScript = wrapperScript
                .Replace("{%Object%}", model.ObjectName)
                .Replace("{%Guid%}", cleanGuid)
                .Replace("{%EnglishName%}", model.DisplayNames.Get(Language.English).value)
                .Replace("{%Localization%}", 
                    model.DisplayNames.Contains(SystemLanguage.Russian) 
                        ? $", Russian: \"{model.DisplayNames.Get(Language.Russian).value}\"" 
                        : "");

            asmdef = asmdef.Replace("{%Object%}", $"{model.ObjectName}_{cleanGuid}");

            File.WriteAllText($"{model.ObjectFolder}/{model.ObjectName}.cs", wrapperScript, Encoding.UTF8);
            File.WriteAllText($"{model.ObjectFolder}/{model.ObjectName}.asmdef", asmdef, Encoding.UTF8);

            return $"{model.ObjectName}_{cleanGuid}";
        }


        public static void CreatePrefab(GameObject gameObject, string localPath, string guid, string objectName, CreateObjectModel.AssetExtras licenseData)
        {
            Debug.Log(string.Format(SdkTexts.ConvertingToPrefabFormat, gameObject.name));
            CreateNewPrefab(gameObject, localPath, guid, objectName, licenseData);
        }

        private static void CreateNewPrefab(GameObject obj, string localPath, string guid, string objectName, CreateObjectModel.AssetExtras licenseData)
        {
            var prefabStatus = PrefabUtility.GetPrefabInstanceStatus(obj);
            if (prefabStatus == PrefabInstanceStatus.Connected)
            {
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            var newPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(obj, localPath, InteractionMode.AutomatedAction, out var success);

            if (!success)
            {
                Debug.LogError("Can not create prefab!");
                return;
            }

            using var prefabScope = new PrefabScope(newPrefab);
            var prefab = prefabScope.Prefab;

            var config = prefab.GetComponent<VarwinObjectDescriptor>() ?? prefab.AddComponent<VarwinObjectDescriptor>();
            SetupConfig(localPath, guid, guid, objectName, config);

            if (licenseData != null)
            {
                HandleLicense(licenseData, config);
            }

            DestroyImmediate(obj);
            var prefabInstance = PrefabUtility.InstantiatePrefab(newPrefab);
            EditorGUIUtility.PingObject(prefabInstance);
        }

        private static void AddPrefabComponents(GameObject obj, string classFullName, string componentName)
        {
            using var prefabScope = new PrefabScope(obj);
            var prefab = prefabScope.Prefab;

            var rigidbody = prefab.GetComponentInChildren<Rigidbody>();

            if (!rigidbody)
            {
                prefab.AddComponent<Rigidbody>();
            }

            Type objectType = Type.GetType(classFullName);

            if (objectType == null)
            {
                Assembly assembly = Assembly.Load(classFullName);
                objectType = assembly.GetType($"Varwin.Types.{classFullName}.{componentName}");
            }

            prefab.AddComponent(objectType);
        }

        private static void HandleLicense(CreateObjectModel.AssetExtras licenseData, VarwinObjectDescriptor config)
        {
            Regex reg = new Regex(@"(.+)\s+\((.+)\)");
            var authorData = reg.Match(licenseData.Author);

            try
            {
                config.AuthorName = authorData.Groups[1].Value;
                config.AuthorUrl = authorData.Groups[2].Value;
            }
            catch
            {
                Debug.LogWarning(SdkTexts.CannotReadAuthorInfo);
            }

            reg = new Regex(@"([a-zA-Z-]+)-([0-9\.]+)\s+\((.+)\)");
            var license = reg.Match(licenseData.License);

            try
            {
                config.LicenseCode = license.Groups[1].Value.ToLower();
            }
            catch
            {
                Debug.LogWarning(SdkTexts.CannotReadLicenseInfo);
            }
        }

        private static void SetupConfig(string localPath, string guid, string rootGuid,  string objectName, VarwinObjectDescriptor config)
        {
            config.Guid = guid;

            if(string.IsNullOrEmpty(rootGuid))
            {
                rootGuid = guid;
            }

            config.RootGuid = rootGuid;

            config.Name = objectName;

            config.DisplayNames = _localizedName;
            config.Description = _localizedDescription;

            config.Prefab = localPath;
            config.PrefabGuid = AssetDatabase.AssetPathToGUID(localPath);

            config.AuthorName = _authorName;
            config.AuthorEmail = _authorEmail;
            config.AuthorUrl = _authorUrl;

            config.LicenseCode = _licenseCode;

            config.BuiltAt = DateTimeOffset.Now.ToString();

            config.AddBehavioursAtRuntime = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.AddBehavioursAtRuntime : config.AddBehavioursAtRuntime;  
            config.MobileReady = SdkSettings.Features.Mobile.Enabled && _mobileReady;
            config.SourcesIncluded = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.SourcesIncluded : config.SourcesIncluded;  
            config.DisableSceneLogic = SdkSettings.Features.OverrideDefaultObjectSettings ? SdkSettings.Features.DisableSceneLogic : config.DisableSceneLogic;  
        }

        /// <summary>
        /// This callback runs when all scripts have been reloaded
        /// But - this window reloads too, so, we just load config from temp file
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            try
            {
                //GetWindow<ImportModelsWindow>().Close();

                int createdNum = 0;

                if (File.Exists(CreateObjectTempModel.TempFilePath))
                {
                    string config = File.ReadAllText(CreateObjectTempModel.TempFilePath);

                    CreateObjectTempModel temp = JsonConvert.DeserializeObject<CreateObjectTempModel>(config);

                    File.Delete(CreateObjectTempModel.TempFilePath);

                    if (temp == null)
                    {
                        Debug.LogError(SdkTexts.TempBuildError);

                        return;
                    }

                    List<CreateObjectModel> _modelsList = temp.Objects;

                    foreach (CreateObjectModel fileModel in _modelsList)
                    {
                        Debug.Log(string.Format(SdkTexts.CreatingPrefabFormat, fileModel.ObjectName));

                        GameObject gameObject, prefab;

                        //Check if it's model import
                        if (fileModel.ModelImportPath != null)
                        {
                            if (fileModel.Skip)
                            {
                                continue;
                            }

                            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fileModel.ModelImportPath);

                            if (modelPrefab == null)
                            {
                                Debug.LogError(string.Format(SdkTexts.CannotCreateObjectFormat, fileModel.ObjectName, fileModel.Path));
                                Directory.Delete(fileModel.ObjectFolder, true);

                                continue;
                            }

                            gameObject = Instantiate(modelPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            
                            //Calculate bounds
                            Bounds bounds = GetBounds(gameObject);
                            float maxBound = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                            float scale = fileModel.BiggestSideSize / maxBound;

                            gameObject.transform.localScale = Vector3.one * scale;
                            Rigidbody objectBody = gameObject.AddComponent<Rigidbody>();
                            objectBody.isKinematic = !fileModel.IsPhysicsOn;
                            objectBody.mass = fileModel.Mass;

                            InteractableObjectBehaviour objectBehaviour =
                                gameObject.AddComponent<InteractableObjectBehaviour>();
                            objectBehaviour.SetIsGrabbable(fileModel.IsGrabbable);
                            objectBehaviour.SetIsUsable(false);
                            objectBehaviour.SetIsTouchable(false);

                            CreateObjectUtils.AddObjectId(gameObject);
                            
                            MeshFilter[] meshes = gameObject.GetComponentsInChildren<MeshFilter>();

                            foreach (MeshFilter meshFilter in meshes)
                            {
                                MeshCollider collider = meshFilter.gameObject.AddComponent<MeshCollider>();
                                collider.sharedMesh = meshFilter.sharedMesh;
                                collider.convex = true;
                            }

                            if (meshes == null || meshes.Length == 0)
                            {
                                BoxCollider box = gameObject.AddComponent<BoxCollider>();

                                box.center = bounds.center;
                                box.size = bounds.size;
                            }

                            CreatePrefab(gameObject,
                                fileModel.PrefabPath,
                                fileModel.Guid,
                                fileModel.ObjectName,
                                fileModel.Extras);
                        }
                        else
                        {
                            AssetDatabase.Refresh();
                            gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(fileModel.PrefabPath);

                            if (gameObject == null)
                            {
                                Debug.LogError(string.Format(SdkTexts.CannotCreateObjectWithoutPrefabAndModelFormat, fileModel.ObjectName));
                                return;
                            }

                            AddPrefabComponents(gameObject, fileModel.ClassName, fileModel.ObjectName);
                        }

                        AssetDatabase.Refresh();

                        if (fileModel.ModelImportPath != null)
                        {
                            DestroyImmediate(gameObject);
                        }

                        createdNum++;

                        gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(fileModel.PrefabPath);
                        EditorGUIUtility.PingObject(gameObject);
                    }

                    EditorUtility.DisplayDialog("Done!", string.Format(createdNum == 1 ? SdkTexts.SingleObjectCreateDoneFormat : SdkTexts.ObjectsCreateDoneFormat, createdNum), "OK");

                    GetWindow<CreateObjectWindow>().Close();
                }
            }
            catch (Exception e)
            {               
                EditorUtility.DisplayDialog("Error!", string.Format(SdkTexts.ObjectsCreateProblemFormat, e.Message), "OK");
                
                
                Debug.LogException(e);
            }
        }

        private static void DestroyAndSpawnPrefab(GameObject prefabInstance)
        {
            using var prefabScope = new PrefabScope(prefabInstance);
            var newInstance = GameObject.Instantiate(prefabScope.Prefab, prefabInstance.transform.position, prefabInstance.transform.rotation, prefabInstance.transform.parent);
            newInstance.transform.localScale = prefabInstance.transform.localScale;

            DestroyImmediate(prefabInstance);
        }

        #region BOUNDS HELPERS

        private static Bounds GetBounds(GameObject obj)
        {
            Bounds bounds;
            Renderer childRender;
            bounds = GetRenderBounds(obj);

            if (bounds.extents.x == 0)
            {
                bounds = new Bounds(obj.transform.position, Vector3.zero);

                foreach (Transform child in obj.transform)
                {
                    childRender = child.GetComponent<Renderer>();

                    if (childRender)
                    {
                        bounds.Encapsulate(childRender.bounds);
                    }
                    else
                    {
                        bounds.Encapsulate(GetBounds(child.gameObject));
                    }
                }
            }

            return bounds;
        }

        private static Bounds GetRenderBounds(GameObject obj)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer render = obj.GetComponent<Renderer>();

            if (render != null)
            {
                return render.bounds;
            }

            return bounds;
        }

        #endregion
    }
}

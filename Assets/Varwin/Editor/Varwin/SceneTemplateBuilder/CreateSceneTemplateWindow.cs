using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Varwin.Editor;
using Varwin.Public;

namespace Varwin.SceneTemplateBuilding
{
    public class CreateSceneTemplateWindow : EditorWindow
    {
        private static SceneTemplateBuilder _sceneTemplateBuilder;
        
        private static LocalizationDictionary _localizedName;
        private static LocalizationDictionaryDrawer _localizedNameDrawer;
        
        private static LocalizationDictionary _localizedDescription;
        private static LocalizationDictionaryDrawer _localizedDescriptionDrawer;
        
        private static string _guid;
        private static string _rootGuid;

        private static Rect _windowRect = new Rect(0, 0, 700, 720);

        private static Camera _camera;
        private static WorldDescriptor _worldDescriptor;
        private static SceneCamera _sceneTemplatePreview;

        private const int ImageSize = 200;
        private const int ColumnsPadding = 20;
        private const int LabelWidth = 42;

        private static string _authorName;
        private static string _authorEmail;
        private static string _authorUrl;

        private static string _licenseCode;
        private static string _licenseVersion;
        private LicenseType _selectedLicense;
        private int _selectedLicenseIndex = -1;
        private int _selectedLicenseVersionIndex = -1;

        private static long _createdAt = -1;
        private static bool _sourcesIncluded;
        private static bool _mobileReady;
        private static bool _currentVersionWasBuilt;
        private static bool _currentVersionWasBuiltAsMobileReady;
        private static bool _hasWrongScripts;

        private static string _changelog;
        private static bool _changelogIsChanged;

        private static bool _varwinToolsIsEnabled;

        private Vector2 _usedScriptsScrollView;
        private Vector2 _wrongScriptsScrollView;
        private Vector2 _scrollPosition;

        private static ChangelogEditorWindow _changelogEditorWindow;

        private static readonly string[] ExcludedDlls =
        {
            "Varwin",
            "Unity",
            "Bakery"
        };

        private static readonly Type[] ExcludedTypes = 
        {
            typeof(WorldDescriptor),
            typeof(SceneCamera),
            typeof(VarwinTools),
            typeof(ObjectId),
        };

        private static GUIStyle _scriptsHelpBoxStyle;

        private static List<MonoBehaviour> _monoBehavioursOnScene; 
        private static List<string> _dllsOnScene;
        

        public static void OpenWindow()
        {
            GetWindowWithRect(typeof(CreateSceneTemplateWindow),
                _windowRect,
                false,
                SdkTexts.CreateSceneTemplateWindowTitle);
        }

        private void Awake()
        {
            _sceneTemplateBuilder ??= new();
            OnEnable();
        }
        
        private void OnEnable()
        {
            Initialize();
        }

        private static void Initialize()
        {
            DetectScripts(out bool breakProcess, out _monoBehavioursOnScene, out _dllsOnScene, false);
            
            FindOrCreateWorldDescriptor();
            FindOrCreateCamera();

            InitializeName();
            InitializeDescription();
            _changelog = _worldDescriptor.Changelog;
            
            FillDefaultDataIfNull();
            
            _guid = _worldDescriptor.Guid;
            _rootGuid = _worldDescriptor.RootGuid;
            _sourcesIncluded = _worldDescriptor.SourcesIncluded;
            _mobileReady = SdkSettings.Features.Mobile.Enabled && _worldDescriptor.MobileReady;
            _currentVersionWasBuilt = _worldDescriptor.CurrentVersionWasBuilt;
            _currentVersionWasBuiltAsMobileReady = _worldDescriptor.CurrentVersionWasBuiltAsMobileReady;

            if (string.IsNullOrWhiteSpace(_worldDescriptor.AuthorName))
            {
                AuthorSettings.Initialize();
                _authorName = AuthorSettings.Name;
                _authorEmail = AuthorSettings.Email;
                _authorUrl = AuthorSettings.Url;
            }
            else
            {
                _authorName = _worldDescriptor.AuthorName;
                _authorEmail = _worldDescriptor.AuthorEmail;
                _authorUrl = _worldDescriptor.AuthorUrl;
            }
        }

        private void OnDisable()
        {
            if (_worldDescriptor)
            {
                EditorUtility.SetDirty(_worldDescriptor);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        
        private static void InitializeName()
        {
            if (!_worldDescriptor)
            {
                FindOrCreateWorldDescriptor();
            }
            
            _localizedName = _worldDescriptor.LocalizedName;

            if (_localizedName.Count == 0 && !string.IsNullOrEmpty(_worldDescriptor.Name))
            {
                if (Regex.IsMatch(_worldDescriptor.Name, @"[а-яА-Я]"))
                {
                    _localizedName.Add(Language.Russian, _worldDescriptor.Name);
                }
                else
                {
                    _localizedName.Add(Language.English, _worldDescriptor.Name);
                }
            }

            _localizedNameDrawer ??= new LocalizationDictionaryDrawer(_localizedName, typeof(LocalizationString), true, false, true, true)
            {
                Title = "Name",
                ErrorMessage = SdkTexts.AnItemWithLanguageHasAlreadyBeenAddedFormat
            };

            _localizedNameDrawer.Initialize(_localizedName);
        }

        private static void InitializeDescription()
        {
            if (!_worldDescriptor)
            {
                FindOrCreateWorldDescriptor();
            }
            
            _localizedDescription = _worldDescriptor.LocalizedDescription;

            if (_localizedDescription.Count == 0 && !string.IsNullOrEmpty(_worldDescriptor.Description))
            {
                if (Regex.IsMatch(_worldDescriptor.Description, @"[а-яА-Я]"))
                {
                    _localizedDescription.Add(Language.Russian, _worldDescriptor.Description);
                }
                else
                {
                    _localizedDescription.Add(Language.English, _worldDescriptor.Description);
                }
            }

            _localizedDescriptionDrawer ??= new LocalizationDictionaryDrawer(_localizedDescription, typeof(LocalizationString), true, false, true, true)
            {
                Title = "Description",
                ErrorMessage = SdkTexts.AnItemWithLanguageHasAlreadyBeenAddedFormat
            };

            _localizedDescriptionDrawer.Initialize(_localizedDescription);
        }

        private static void FillDefaultDataIfNull()
        {
            if (_worldDescriptor.LocalizedName.Count == 0)
            {
                _worldDescriptor.LocalizedName.Add(Language.English, "Scene Template");
            }

            if (string.IsNullOrEmpty(_worldDescriptor.Guid))
            {
                _worldDescriptor.Guid = Guid.NewGuid().ToString();
                _worldDescriptor.RootGuid = _worldDescriptor.Guid;
                _worldDescriptor.CurrentVersionWasBuilt = false;
                _worldDescriptor.CurrentVersionWasBuiltAsMobileReady = false;
            }
        }

        private static Camera FindOrCreateCamera()
        {
            _sceneTemplatePreview = FindObjectsOfType<SceneCamera>(true).FirstOrDefault();

            if (_sceneTemplatePreview != null)
            {
                _sceneTemplatePreview.gameObject.name = "[Camera Preview]";
                _sceneTemplatePreview.transform.SetParent(_worldDescriptor.transform, true);
            }
            else
            {
                GameObject cameraPreview = new GameObject("[Camera Preview]");
                cameraPreview.transform.SetParent(_worldDescriptor.transform, true);
                cameraPreview.transform.position = new Vector3(0, 1, 0);

                _sceneTemplatePreview = cameraPreview.AddComponent<SceneCamera>();
            }

            if (!_sceneTemplatePreview.Camera)
            {
                _sceneTemplatePreview.Camera = _sceneTemplatePreview.GetComponent<Camera>();
            }

            if (!_sceneTemplatePreview.Camera.targetTexture)
            {
                _sceneTemplatePreview.Init(256, 256);
            }

            _sceneTemplatePreview.gameObject.SetActive(true);

            _camera = _sceneTemplatePreview.GetComponent<Camera>();
            _camera.enabled = true;

            return _camera;
        }

        private static void FindOrCreateWorldDescriptor()
        {
            _worldDescriptor = FindObjectsOfType<WorldDescriptor>(true).FirstOrDefault();

            if (_worldDescriptor)
            {
                _worldDescriptor.gameObject.SetActive(true);
            }
            else
            {
                var worldDescriptorGo = new GameObject("[World Descriptor]");
                worldDescriptorGo.transform.position = Vector3.zero;
                worldDescriptorGo.transform.rotation = Quaternion.identity;
                worldDescriptorGo.transform.localScale = Vector3.one;

                _worldDescriptor = worldDescriptorGo.AddComponent<WorldDescriptor>();
            }

            FindOrCreateSpawnPoint(_worldDescriptor);

            UnityEngine.SceneManagement.Scene scene = _worldDescriptor.gameObject.scene;

            if (string.IsNullOrEmpty(_worldDescriptor.SceneGuid))
            {
                _worldDescriptor.SceneGuid = AssetDatabase.AssetPathToGUID(scene.path);
                EditorSceneManager.SaveScene(scene);
            }
            else
            {
                string sceneGuid = AssetDatabase.AssetPathToGUID(scene.path);
                if (!string.Equals(_worldDescriptor.SceneGuid, sceneGuid) && !string.IsNullOrEmpty(sceneGuid))
                {
                    _worldDescriptor.SceneGuid = sceneGuid;
                    _worldDescriptor.RegenerateGuid();
                    _worldDescriptor.CleanBuiltInfo();
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }

        private static Transform FindOrCreateSpawnPoint(WorldDescriptor worldDescriptor)
        {
            if (!worldDescriptor.PlayerSpawnPoint)
            {
                var spawnPoint = new GameObject("[Spawn Point]");
                spawnPoint.transform.position = Vector3.zero;
                spawnPoint.transform.rotation = Quaternion.identity;
                spawnPoint.transform.localScale = Vector3.one;
                spawnPoint.transform.SetParent(_worldDescriptor.transform);
                worldDescriptor.PlayerSpawnPoint = spawnPoint.transform;
            }
            else
            {
                worldDescriptor.PlayerSpawnPoint.SetParent(_worldDescriptor.transform, true);
                worldDescriptor.PlayerSpawnPoint.gameObject.name = "[Spawn Point]";
            }

            return worldDescriptor.PlayerSpawnPoint;
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            GUILayout.Label(SdkTexts.SceneTemplateSettings, EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(GUILayout.Width(_windowRect.width - ImageSize - ColumnsPadding * 2));

            DrawLocalizedName();
            EditorGUILayout.Space();

            DrawLocalizedDescription();

            if (SdkSettings.Features.DeveloperMode.Enabled)
            {
                DrawGuidLabel();
            }

            EditorGUILayout.Space();

            DrawSourcesArea();

            if (SdkSettings.Features.Mobile.Enabled)
            {
                DrawPlatformsArea();
            }
            else
            {
                _mobileReady = false;
            }

            EditorGUILayout.Space();
            DrawScriptsArea();

            EditorGUILayout.Space();
            DrawAuthorSettings();

            EditorGUILayout.Space();
            DrawLicenseSettings();

            if (EditorUtility.scriptCompilationFailed)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(SdkTexts.ScriptCompilationFailed, MessageType.Error);
                EditorGUILayout.Space();
            }
            else
            {
                EditorGUILayout.Space();
                DrawBuildButton();
            }

            EditorGUILayout.EndVertical();

            if (_camera && _camera.targetTexture)
            {
                var cameraRect = new Rect(_windowRect.width - ImageSize - ColumnsPadding,
                    ColumnsPadding,
                    ImageSize,
                    ImageSize);
                EditorGUI.DrawPreviewTexture(cameraRect, _camera.targetTexture);

                var cameraMoveButtonRect = new Rect(cameraRect) {height = 20, y = cameraRect.yMax};

                if (GUI.Button(cameraMoveButtonRect, SdkTexts.MoveCameraToEditorView))
                {
                    SceneUtils.MoveCameraToEditorView(_camera ? _camera : FindOrCreateCamera());
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void OnDestroy()
        {
            if (_camera)
            {
                _camera.enabled = false;
            }

            ChangelogEditorUnsubscribe(true);
        }
        
        private void DrawLocalizedName()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            if (_localizedNameDrawer == null)
            {
                InitializeName();
            }
            _localizedNameDrawer?.Draw();
            EditorGUILayout.EndVertical();
        }

        private void DrawLocalizedDescription()
        {
            EditorGUILayout.BeginVertical(EditorStyles.inspectorFullWidthMargins);
            if (_localizedDescriptionDrawer == null)
            {
                InitializeDescription();
            }
            _localizedDescriptionDrawer?.Draw();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawGuidLabel()
        {
            EditorGUILayout.LabelField($"Guid: {_guid}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Root Guid: {_rootGuid}", EditorStyles.miniLabel);
        }
        
        private void DrawSourcesArea()
        {
            _sourcesIncluded = EditorGUILayout.Toggle("Include Sources", _sourcesIncluded);
            _worldDescriptor.SourcesIncluded = _sourcesIncluded;
        }

        private void DrawPlatformsArea()
        {
            _mobileReady = EditorGUILayout.Toggle("Mobile Ready", _mobileReady);
            _worldDescriptor.MobileReady = _mobileReady;
        }
        
        private void DrawBuildButton()
        {
            bool isDisabled = string.IsNullOrWhiteSpace(_authorName)
                              || !_localizedName.IsValidDictionary
                              || _localizedName.Count == 0
                              || string.IsNullOrWhiteSpace(_guid)
                              || string.IsNullOrWhiteSpace(_worldDescriptor.SceneGuid)
                              || !VarwinVersionInfo.Exists
                              || _hasWrongScripts;

            if (!CheckUnityVersion())
            {
                isDisabled = true;
                EditorGUILayout.HelpBox(
                    string.Format(SdkTexts.WrongUnityVersionBuildMessage, Application.unityVersion, VarwinVersionInfo.RequiredUnityVersion),
                    MessageType.Error
                );
            }

            EditorGUI.BeginDisabledGroup(isDisabled);

            if (GUILayout.Button("Build"))
            {
                if (!CheckLocationForTeleport())
                {
                    if (!EditorUtility.DisplayDialog(SdkTexts.SceneTemplateWindowTitle,
                            SdkTexts.NoTeleportAreaMessage,
                            "Build",
                            "Cancel"))
                    {
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                }

                if (SdkSettings.Features.Changelog)
                {
                    _changelogEditorWindow = ChangelogEditorWindow.OpenWindow(_changelog);
                    _changelogEditorWindow.BuildButtonPressed += ChangelogEditorWindowOnBuildButtonPressed;
                    _changelogEditorWindow.CancelButtonPressed += ChangelogEditorWindowOnCancelButtonPressed;
                }
                else
                {
                    PrepareSceneForBuild();
                    var sceneBuilder = GetSceneTemplateBuilder();
                    sceneBuilder.PrepareBuildSteps();
                }
            }

            #if VARWIN_DEVELOPER_MODE
            if (_worldDescriptor.UseScripts)
            {
                if (GUILayout.Button("Logic Only Build"))
                {
                    if (!CheckLocationForTeleport())
                    {
                        if (!EditorUtility.DisplayDialog(SdkTexts.SceneTemplateWindowTitle,
                                SdkTexts.NoTeleportAreaMessage,
                                "Build",
                                "Cancel"))
                        {
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndVertical();
                            return;
                        }
                    }

                    if (SdkSettings.Features.Changelog)
                    {
                        _changelogEditorWindow = ChangelogEditorWindow.OpenWindow(_changelog);
                        _changelogEditorWindow.BuildButtonPressed += ChangelogEditorWindowOnBuildButtonPressed;
                        _changelogEditorWindow.CancelButtonPressed += ChangelogEditorWindowOnCancelButtonPressed;
                    }
                    else
                    {
                        PrepareSceneForBuild();
                        var sceneBuilder = GetSceneTemplateLogicOnlyBuilder();
                        sceneBuilder.PrepareBuildSteps();
                    }
                }                
            }

            #endif

            EditorGUI.EndDisabledGroup();

            if (!VarwinVersionInfo.Exists)
            {
                EditorGUILayout.HelpBox(SdkTexts.VersionNotFoundMessage, MessageType.Error);
            }

            if (string.IsNullOrWhiteSpace(_worldDescriptor.SceneGuid))
            {
                EditorGUILayout.HelpBox(SdkTexts.SceneIsNotSaved, MessageType.Error);
            }

            if (!CheckLocationForTeleport())
            {
                EditorGUILayout.HelpBox(SdkTexts.NoTeleportAreaWarning, MessageType.Warning);
            }

            if (!CheckAndroidModuleInstalled())
            {
                EditorGUILayout.HelpBox(SdkTexts.NoAndroidModuleWarning, MessageType.Warning);
            }
        }

        private static void ChangelogEditorWindowOnBuildButtonPressed(ChangelogEditorWindow changelogEditorWindow, string changelog, bool isChanged)
        {
            if (isChanged)
            {
                _changelog = changelog;
                _worldDescriptor.Changelog = changelog;
            }
            
            ChangelogEditorUnsubscribe(true);
            PrepareSceneForBuild();
            var sceneBuilder = GetSceneTemplateBuilder();
            sceneBuilder.PrepareBuildSteps();
        }

        private static void ChangelogEditorWindowOnCancelButtonPressed(ChangelogEditorWindow changelogEditorWindow)
        {
            ChangelogEditorUnsubscribe(true);
        }

        private static void ChangelogEditorUnsubscribe(bool close = false)
        {
            if (_changelogEditorWindow)
            {
                _changelogEditorWindow.BuildButtonPressed -= ChangelogEditorWindowOnBuildButtonPressed;
                _changelogEditorWindow.CancelButtonPressed -= ChangelogEditorWindowOnCancelButtonPressed;
                if (close && !_changelogEditorWindow.Destroyed)
                {
                    _changelogEditorWindow.Close();
                }
                _changelogEditorWindow = null;
            }
        }

        private static void SetupNewVersion()
        {
            _guid = Guid.NewGuid().ToString();

            _worldDescriptor.Guid = _guid;

            _currentVersionWasBuilt = false;
            _worldDescriptor.CurrentVersionWasBuilt = false;

            _currentVersionWasBuiltAsMobileReady = false;
            _worldDescriptor.CurrentVersionWasBuiltAsMobileReady = false;
        }
        
        private void DrawScriptsArea()
        {
            if (_scriptsHelpBoxStyle == null)
            {
                _scriptsHelpBoxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    fixedHeight = 108
                };
            }
            
            EditorGUILayout.BeginVertical(_scriptsHelpBoxStyle);

            GUILayout.Label("Scripts:", EditorStyles.miniBoldLabel);

            if (_dllsOnScene.Count == 0)
            {
                EditorGUI.BeginDisabledGroup(true);
                _worldDescriptor.UseScripts = EditorGUILayout.ToggleLeft("Use Scripts on SceneTemplate", false);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.HelpBox("No scripts found on scene", MessageType.Info);
            }
            else
            {
                _worldDescriptor.UseScripts = EditorGUILayout.ToggleLeft("Use Scripts on SceneTemplate", _worldDescriptor.UseScripts);
                _hasWrongScripts = DetectWrongVarwinStrips(_monoBehavioursOnScene, out var wrongScripts);
                if (_hasWrongScripts)
                {
                    _wrongScriptsScrollView = EditorGUILayout.BeginScrollView(_wrongScriptsScrollView);

                    EditorGUILayout.HelpBox("Scene contains wrong scripts (ObjectId / VarwinObjectDescriptor/ VarwinObject)", MessageType.Error);
                    foreach (var monoBehaviour in wrongScripts)
                    {
                        if (GUILayout.Button($"{monoBehaviour.name} ({monoBehaviour.GetType().Name})", EditorStyles.miniLabel))
                        {
                            EditorGUIUtility.PingObject(monoBehaviour);
                            Selection.objects = new[] {monoBehaviour.gameObject};
                        }
                    }

                    EditorGUILayout.EndScrollView();
                }

                EditorGUI.BeginDisabledGroup(!_worldDescriptor.UseScripts);

                _usedScriptsScrollView = EditorGUILayout.BeginScrollView(_usedScriptsScrollView);

                if(!_hasWrongScripts)
                {
                    foreach (var monoBehaviour in _monoBehavioursOnScene)
                    {
                        if (!monoBehaviour)
                        {
                            DetectScripts(out bool breakProcess, out _monoBehavioursOnScene, out _dllsOnScene, false);
                            break;
                        }

                        if (GUILayout.Button($"{monoBehaviour.name} ({monoBehaviour.GetType().Name})", EditorStyles.miniLabel))
                        {
                            EditorGUIUtility.PingObject(monoBehaviour);
                            Selection.objects = new[] {monoBehaviour.gameObject};
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
                
                EditorGUI.EndDisabledGroup();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawAuthorSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Author:", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(LabelWidth));
            _authorName = EditorGUILayout.TextField(_authorName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("E-Mail:", GUILayout.Width(LabelWidth));
            _authorEmail = EditorGUILayout.TextField(_authorEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("URL:", GUILayout.Width(LabelWidth));
            _authorUrl = EditorGUILayout.TextField(_authorUrl);
            EditorGUILayout.EndHorizontal();

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

            _worldDescriptor.AuthorName = _authorName;
            _worldDescriptor.AuthorEmail = _authorEmail;
            _worldDescriptor.AuthorUrl = _authorUrl;

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
                if (_selectedLicenseVersionIndex >= _selectedLicense.Versions.Length
                    || _selectedLicenseVersionIndex < 0)
                {
                    _selectedLicenseVersionIndex = 0;
                }

                _selectedLicenseVersionIndex = EditorGUILayout.Popup(_selectedLicenseVersionIndex,
                    _selectedLicense.Versions,
                    GUILayout.Width(80));
                _licenseVersion = _selectedLicense.Versions[_selectedLicenseVersionIndex];
            }
            else
            {
                _licenseVersion = string.Empty;
            }

            _worldDescriptor.LicenseCode = _licenseCode;
            _worldDescriptor.LicenseVersion = _licenseVersion;

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

        private bool CheckLocationForTeleport()
        {
            var areas = GameObject.FindGameObjectsWithTag("TeleportArea");

            return areas.Length > 0;
        }

        private static bool CheckAndroidModuleInstalled()
        {
            if (!_mobileReady)
            {
                return true;
            }

            var bindingFlags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            var moduleManager = System.Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", bindingFlags);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", bindingFlags);
            return (bool) isPlatformSupportLoaded.Invoke(null, new object[] {(string) getTargetStringFromBuildTarget.Invoke(null, new object[] {BuildTarget.Android})});
        }

        private static bool CheckUnityVersion()
        {
            return Application.unityVersion == VarwinVersionInfo.RequiredUnityVersion;
        }

        public static void PrepareSceneForBuild()
        {
            Debug.Log(SdkTexts.SceneTemplateBuildStartMessage);
            
            Initialize();
            
            SetupNewVersion();
            
            _worldDescriptor.LocalizedName = _localizedName;
            _worldDescriptor.LocalizedDescription = _localizedDescription;
            _worldDescriptor.Guid = _guid;
            _worldDescriptor.RootGuid = _rootGuid;
            if (string.IsNullOrEmpty(_worldDescriptor.RootGuid))
            {
                _rootGuid = _guid;
                _worldDescriptor.RootGuid = _guid;
            }

            _worldDescriptor.Image = "bundle.png";
            _worldDescriptor.AssetBundleLabel = "bundle";

            _worldDescriptor.AuthorName = _authorName;
            _worldDescriptor.AuthorEmail = _authorEmail;
            _worldDescriptor.AuthorUrl = _authorUrl;

            _worldDescriptor.LicenseCode = _licenseCode;
            _worldDescriptor.LicenseVersion = _licenseVersion;

            _worldDescriptor.BuiltAt = DateTimeOffset.Now.ToString();
            _worldDescriptor.SourcesIncluded = _sourcesIncluded;
            _worldDescriptor.MobileReady = _mobileReady;

            _currentVersionWasBuilt = true;
            _worldDescriptor.CurrentVersionWasBuilt = true;

            _currentVersionWasBuiltAsMobileReady = _worldDescriptor.MobileReady;
            _worldDescriptor.CurrentVersionWasBuiltAsMobileReady = _worldDescriptor.MobileReady;


            if (_worldDescriptor.UseScripts)
            {
                DetectScripts(out bool breakProcess, out var monoBehaviours, out var dlls);
                if (breakProcess)
                {
                    return;
                }

                _worldDescriptor.DllNames = dlls.ToArray();

                var monoBehaviourTypes = new HashSet<Type>();
                foreach (var monoBehaviour in monoBehaviours)
                {
                    var monoBehaviourType = monoBehaviour.GetType();
                    monoBehaviourTypes.Add(monoBehaviourType);
                }
                
                var asmdefNames = new HashSet<string>();
                foreach (var monoBehaviourType in monoBehaviourTypes)
                {
                    var monoBehaviourAsmdef = AsmdefUtils.FindAsmdef(monoBehaviourType);

                    if (monoBehaviourAsmdef == null)
                    {
                        Debug.LogError("Can't find assembly definition file for " + monoBehaviourType);
                        continue;
                    }
                    
                    var monoBehaviourAsmdefData = AsmdefUtils.LoadAsmdefData(monoBehaviourAsmdef);
                    asmdefNames.Add(monoBehaviourAsmdefData.name);
                }
                _worldDescriptor.AsmdefNames = asmdefNames.ToArray().Where(name => !ExcludedDlls.Any(name.StartsWith)).ToArray();
            }
            else
            {
                _worldDescriptor.DllNames = Array.Empty<string>();
                _worldDescriptor.AsmdefNames = Array.Empty<string>();
            }

            ToggleCamerasActivity(false);
            
            SetVarwinToolsActivity(false);
            if (_worldDescriptor && _worldDescriptor.gameObject)
            {
                EditorSceneManager.SaveScene(_worldDescriptor.gameObject.scene);
            }
        }

        public static SceneTemplateBuilder GetSceneTemplateBuilder()
        {
            try
            {
                _sceneTemplateBuilder ??= new SceneTemplateBuilder();

                _sceneTemplateBuilder.Initialize(_worldDescriptor, _sceneTemplatePreview);
                return _sceneTemplateBuilder;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog(SdkTexts.SceneTemplateWindowTitle, $"{SdkTexts.SceneTemplateBuildErrorMessage}\n{e}", "OK");
                return null;
            }
        }

        #if VARWIN_DEVELOPER_MODE

        public static SceneTemplateLogicOnlyBuilder GetSceneTemplateLogicOnlyBuilder()
        {
            try
            {
                if (_sceneTemplateBuilder is not SceneTemplateLogicOnlyBuilder)
                    _sceneTemplateBuilder = new SceneTemplateLogicOnlyBuilder();
                
                _sceneTemplateBuilder.Initialize(_worldDescriptor, _sceneTemplatePreview);
                return (SceneTemplateLogicOnlyBuilder)_sceneTemplateBuilder;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                EditorUtility.DisplayDialog(SdkTexts.SceneTemplateWindowTitle, $"{SdkTexts.SceneTemplateBuildErrorMessage}\n{e}", "OK");
                return null;
            }
        }
        
        #endif

        private static void ToggleCamerasActivity(bool enabled)
        {
            var sceneCameras = FindObjectsOfType<Camera>(true).ToList();
            Camera sceneTemplatePreviewCamera = null;
            if (_sceneTemplatePreview && _sceneTemplatePreview.TryGetComponent(out sceneTemplatePreviewCamera))
            {
                sceneCameras.Remove(sceneTemplatePreviewCamera);
            }
            ToggleCams(sceneCameras, sceneTemplatePreviewCamera, enabled);
        }

        private static void DetectScripts(out bool breakProcess, out List<MonoBehaviour> monoBehaviours, out List<string> dlls, bool detectGlobalScripts = true)
        {
            breakProcess = false;
            monoBehaviours = FindObjectsOfType<MonoBehaviour>(true).Where(CheckMonoBehaviourType).ToList();

            if (detectGlobalScripts)
            {
                DetectGlobalScripts(monoBehaviours, out breakProcess);
            }

            if (!breakProcess)
            {
                var scriptDlls = DllHelper.GetForScripts(monoBehaviours).Keys.ToArray();
                dlls = new();
                foreach (string scriptDll in scriptDlls)
                {
                    string scriptDllName = scriptDll.Replace('\\', '/').SubstringAfterLast("/");
                    if (!ExcludedDlls.Any(scriptDllName.Contains))
                    {
                        dlls.Add(scriptDll);
                    }
                }
            }
            else
            {
                dlls = null;
            }

            bool CheckMonoBehaviourType(MonoBehaviour monoBehaviour)
            {
                var fullName = monoBehaviour.GetType().FullName;

                return monoBehaviour
                       && !fullName.StartsWith("Unity")
                       && !fullName.StartsWith("VarwinCore")
                       && !fullName.StartsWith("SmartLocalization")
                       && !fullName.StartsWith("UnityEditor")
                       && !fullName.StartsWith("Bakery")
                       && !fullName.Contains("Varwin.AdapterLoader")
                       && ExcludedTypes.All(t => t != monoBehaviour.GetType());
            }
        }

        private static void DetectGlobalScripts(ICollection<MonoBehaviour> scripts, out bool breakProcess)
        {
            string anyGlobalScript = CreateObjectUtils.GetGlobalScriptsPaths(scripts).FirstOrDefault();
            if (anyGlobalScript != null)
            {
                string message = string.Format(SdkTexts.ScriptWithoutAsmdefInAssetsFormat, anyGlobalScript) + "\n\n" + SdkTexts.ContinueSceneTemplateBuildingProblems;
                if (EditorUtility.DisplayDialog("Scene building error!", message, "Continue", "Cancel"))
                {
                    var globalScripts = CreateObjectUtils.GetGlobalScripts(scripts);
                    foreach (MonoBehaviour script in globalScripts)
                    {
                        scripts.Remove(script);
                    }
                }
                else
                {
                    breakProcess = true;
                    return;
                }
            }

            breakProcess = false;
        }

        private static bool DetectWrongVarwinStrips(List<MonoBehaviour> sceneMonoBehaviours, out List<MonoBehaviour> wrongScripts)
        {
            wrongScripts = sceneMonoBehaviours
                .Where(monoBehaviour => monoBehaviour && monoBehaviour is ObjectId or VarwinObjectDescriptor or VarwinObject)
                .ToList();

            return wrongScripts.Count > 0;
        }
        
        private static void SetVarwinToolsActivity(bool isActive)
        {
            VarwinTools varwinToolsObject = FindObjectsOfType<VarwinTools>(true).FirstOrDefault();
            
            if (!varwinToolsObject)
            {
                return;
            }

            if (!isActive)
            {
                _varwinToolsIsEnabled = varwinToolsObject.gameObject.activeSelf;
                varwinToolsObject.gameObject.SetActive(false);
            }
            else
            {
                varwinToolsObject.gameObject.SetActive(_varwinToolsIsEnabled);
            }
        }

        private static void ToggleCams(IEnumerable<Camera> cams, Camera preview, bool toggle)
        {
            foreach (Camera cam in cams)
            {
                if (!cam || !cam.gameObject)
                {
                    continue;
                }

                if (!cam.targetTexture)
                {
                    cam.enabled = toggle;
                }
            }

            if (_sceneTemplatePreview)
            {
                _sceneTemplatePreview.enabled = toggle;
            }

            if (preview)
            {
                preview.enabled = toggle;
            }
        }

        private void Update()
        {
            Repaint();
            
            if (EditorApplication.isCompiling)
            {
                return;
            }

            _sceneTemplateBuilder ??= SceneTemplateBuilder.Deserialize();
            _sceneTemplateBuilder?.Update();
            if (_sceneTemplateBuilder is { IsFinished: true })
            {
                if (!_sceneTemplateBuilder.HasErrors)
                {
                    OnSceneTemplateBuildingFinished();
                }
                
                _sceneTemplateBuilder = null;
            }
        }

        private static void OnSceneTemplateBuildingFinished()
        {
            Debug.Log(SdkTexts.SceneTemplateWasBuilt);
            DirectoryUtils.OpenFolder(SdkSettings.SceneTemplateBuildingFolderPath);
            EditorUtility.DisplayDialog(SdkTexts.SceneTemplateWindowTitle, SdkTexts.SceneTemplateWasBuilt, "OK");

            GetWindow(typeof(CreateSceneTemplateWindow)).Close();

            SetVarwinToolsActivity(true);
            ToggleCamerasActivity(true);
            if (_worldDescriptor && _worldDescriptor.gameObject)
            {
                EditorSceneManager.SaveScene(_worldDescriptor.gameObject.scene);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class CreatePackageWindow : EditorWindow
    {
        private static Vector2 MinWindowSize => new Vector2(420, 480);
        private static Vector2 MaxWindowSize => new Vector2(640, 800);

        private static CreatePackageWindow _window;
        private static Vector2 _scrollPosition;

        private static Texture2D _packageIcon;

        private static string _packagePath = "Assets/VarwinPackage.asset";
        private static bool _packagePathIsValid;

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

        private static long _createdAt = -1;

        public static CreatePackageWindow OpenWindow()
        {
            _window = GetWindow<CreatePackageWindow>(true, "Create package", true);
            _window.minSize = MinWindowSize;
            _window.maxSize = MaxWindowSize;
            _window.Show();

            _window.Initialize();

            return _window;
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (!_window)
            { 
                _window = GetWindow<CreatePackageWindow>(true, "Create package", true);
            }
            
            InitializeName();
            InitializeDescription();
            
            _localizedName.Add(new LocalizationString(Language.English, "Package Name"));
            _localizedDescription.Add(new LocalizationString(Language.English, "Package Description"));
            
            AuthorSettings.Initialize();
            _authorName = AuthorSettings.Name;
            _authorEmail = AuthorSettings.Email;
            _authorUrl = AuthorSettings.Url;

            _scrollPosition = Vector2.zero;
        }

        private static void InitializeName()
        {
            _localizedName = new LocalizationDictionary();

            _localizedNameDrawer = new LocalizationDictionaryDrawer(_localizedName, typeof(LocalizationString), true, false, true, true)
            {
                Title = "Name",
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

            GUILayout.Label("Package Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            DrawPackagePath();

            DrawPackageIcon();

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            DrawLocalizedName();

            EditorGUILayout.Space();
            DrawLocalizedDescription();

            EditorGUILayout.Space();
            DrawAuthorSettings();

            EditorGUILayout.Space();
            DrawLicenseSettings();

            EditorGUILayout.Space();
            DrawCreateObjectButton();

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawPackagePath()
        {
            Rect controlRect = EditorGUILayout.GetControlRect();
            
            var labelRect = new Rect(controlRect) {size = EditorUtils.CalcSize("Package Path: ", EditorStyles.miniBoldLabel)};
            EditorGUI.LabelField(labelRect, "Package Path: ", EditorStyles.miniBoldLabel);
            
            var pathRect = new Rect(controlRect) {x = labelRect.x + labelRect.width, size = EditorUtils.CalcSize(_packagePath + " ", EditorStyles.miniLabel)};
            EditorGUI.LabelField(pathRect, _packagePath, EditorStyles.miniLabel);
            
            var buttonRect = new Rect(pathRect.x + pathRect.width, labelRect.y + 1, 22, labelRect.height);
            if (GUI.Button(buttonRect, "...", EditorStyles.miniButton))
            {
                _packagePath = EditorUtility.SaveFilePanelInProject("Package Path", "VarwinPackage", "asset", "Choose package destination").Trim().TrimEnd('/');
            }
            
            _packagePathIsValid = false;
            if (string.IsNullOrEmpty(_packagePath))
            {
                EditorGUILayout.HelpBox(SdkTexts.ObjectClassNameEmptyWarning, MessageType.Error);
            }
            else
            {
                _packagePathIsValid = true;
            }
        }

        private void DrawPackageIcon()
        {
            var iconPadding = new Vector2Int(4, 4);
            var iconSize = new Vector2Int(60, 60);
            
            var iconRect = new Rect(_window.position.width - iconSize.x - iconPadding.x, iconPadding.y, iconSize.x, iconSize.y);
            
            _packageIcon = (Texture2D) EditorGUI.ObjectField(iconRect, _packageIcon, typeof(Texture2D), false);
        }
        
        private void DrawLocalizedName()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Package Name:", EditorStyles.miniBoldLabel);

            if (_localizedNameDrawer == null)
            {
                InitializeName();
            }

            _localizedNameDrawer?.Draw();

            EditorGUILayout.EndVertical();
        }

        private void DrawLocalizedDescription()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Package Description:", EditorStyles.miniBoldLabel);

            if (_localizedDescriptionDrawer == null)
            {
                InitializeDescription();
            }

            _localizedDescriptionDrawer?.Draw();

            EditorGUILayout.EndVertical();
        }

        private void DrawObjectContainsComponentHelpBox<T>(MessageType messageType = MessageType.Error)
        {
            EditorGUILayout.HelpBox(string.Format(SdkTexts.ObjectContainsComponentWarningFormat, typeof(T).Name), messageType);
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
                              || !_packagePathIsValid
                              || !_localizedNameDrawer.IsValidDictionary;

            EditorGUI.BeginDisabledGroup(isDisabled);

            if (GUILayout.Button("Create"))
            {
                try
                {
                    var packageDescriptor = ScriptableObject.CreateInstance<VarwinPackageDescriptor>();

                    packageDescriptor.Name = new LocalizationDictionary(_localizedName.LocalizationStrings);
                    packageDescriptor.Description = new LocalizationDictionary(_localizedDescription.LocalizationStrings);
                    
                    packageDescriptor.Guid = Guid.NewGuid().ToString();
                    packageDescriptor.RootGuid = packageDescriptor.Guid;

                    packageDescriptor.AuthorName = _authorName;
                    packageDescriptor.AuthorEmail = _authorEmail;
                    packageDescriptor.AuthorUrl = _authorUrl;

                    packageDescriptor.LicenseCode = _licenseCode;
                    packageDescriptor.LicenseVersion = _licenseVersion;
                    
                    AssetDatabase.CreateAsset(packageDescriptor, _packagePath);
                    AssetDatabase.SaveAssets();

                    EditorUtility.FocusProjectWindow();

                    Selection.activeObject = packageDescriptor;
                }
                catch (Exception e)
                {

                    EditorUtility.DisplayDialog("Error!", string.Format(SdkTexts.CannotCreateObjectErrorFormat, e.Message), "OK");
                    Debug.LogException(e);
                    Close();
                }
            }

            EditorGUI.EndDisabledGroup();
        }
    }
}

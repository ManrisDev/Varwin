using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class VarwinBuilderWindow : EditorWindow
    {
        public static VarwinBuilderWindow Instance { get; private set; }
        
        public bool IsFinished { get; private set; }
        public bool IsStopped { get; private set; }

        private VarwinBuilder _varwinBuilder;
        
        private int _objectsToBuildCount;
        private int _objectsToBuildWithErrorsCount;
        private string[] _objectsToBuildWithErrorsNames;
        private float _displayProgress;
        private Vector2 _scrollPosition;
        
        public static VarwinBuilderWindow GetWindow(string title = "Building varwin objects")
        {
            VarwinBuilderWindow window = Instance;
            if (!window)
            {
                window = GetWindow<VarwinBuilderWindow>(true, title, true);
            }
            else
            {
                window.titleContent = new GUIContent(title);
            }

            window.minSize = new Vector2(350, 216);
            window.maxSize = window.minSize;
            window.Show();

            return window;
        }

        public void Build(VarwinObjectDescriptor varwinObjectDescriptor)
        {
            Build(new [] {varwinObjectDescriptor});
        }
        
        public void Build(IEnumerable<VarwinObjectDescriptor> varwinObjectDescriptors)
        {
            BuildTimeUtils.StartTime = DateTime.Now;
            _varwinBuilder = new VarwinBuilder();
            _varwinBuilder.Build(varwinObjectDescriptors);
            IsFinished = false;
            _displayProgress = 0f;
        }

        public void Build(VarwinPackageDescriptor varwinPackageDescriptor)
        {
            BuildTimeUtils.StartTime = DateTime.Now;
            _varwinBuilder = new VarwinBuilder();
            _varwinBuilder.Build(varwinPackageDescriptor);
            IsFinished = false;
            _displayProgress = 0f;
        }

        public void Build(IEnumerable<VarwinPackageDescriptor> varwinPackageDescriptors)
        {
            BuildTimeUtils.StartTime = DateTime.Now;
            _varwinBuilder = new VarwinBuilder();
            _varwinBuilder.Build(varwinPackageDescriptors);
            IsFinished = false;
            _displayProgress = 0f;
        }

        public void BuildLogic(IEnumerable<VarwinObjectDescriptor> varwinObjectDescriptors)
        {
            BuildTimeUtils.StartTime = DateTime.Now;

            _varwinBuilder = new VarwinLogicOnlyBuilder();
            _varwinBuilder.Build(varwinObjectDescriptors);

            IsFinished = false;
            _displayProgress = 0f;
        }

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void OnGUI()
        {
            if (!Instance)
            {
                Instance = this;
            }
            
            if (!VersionExists())
            {
                Exit();
                return;
            }

            if (_varwinBuilder is { IsStopped: true })
            {
                Exit(false);
                return;
            }

            GUILayout.BeginVertical();

            if (!IsFinished)
            {
                DrawHint();
                GUILayout.FlexibleSpace();
                DrawProgress();
                GUILayout.FlexibleSpace();
                EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), _displayProgress, _varwinBuilder?.Label ?? SdkTexts.CompilingScriptsStep);
            }
            else
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
                
                DisplayBuildingDuration();
                DrawObjectBuildInfo();
                
                GUILayout.FlexibleSpace();
                
                GUILayout.EndScrollView();
                
                if (GUILayout.Button("Show in Explorer"))
                {
                    DirectoryUtils.OpenFolder(VarwinBuildingPath.BakedObjects);
                }
            }

            GUILayout.EndVertical();
        }
        
        #region DRAW
        
        private void DrawHint()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var waitUntilObjectsCreateStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            
            GUILayout.Label(SdkTexts.WaitUntilObjectsCreate, waitUntilObjectsCreateStyle, GUILayout.ExpandHeight(true));
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawProgress()
        { 
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var progressStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 42,
                normal = {textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black},
                alignment = TextAnchor.MiddleCenter
            };

            var progressRect = new Rect(Vector2.zero, Instance.maxSize);
                
            GUI.Label(progressRect, _displayProgress.ToString("0%"), progressStyle);
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DisplayBuildingDuration()
        {
            string messageTitle = $"Build was finished in {BuildTimeUtils.GetBuildTime()}!";
            GUILayout.Label(messageTitle, ObjectBuilderStyles.GreenResultMessageStyle);
        }

        private void DebugBuildingDuration()
        {
            Debug.Log($"<color=green><b>Build was finished in {BuildTimeUtils.GetBuildTime()}!</b></color>");
        }

        private void DrawObjectBuildInfo()
        {
            string objectsBuiltCount = $"Objects built: {_objectsToBuildCount - _objectsToBuildWithErrorsCount}/{_objectsToBuildCount}";
            GUILayout.Label(objectsBuiltCount, ObjectBuilderStyles.ResultMessageStyle);

            if (_objectsToBuildWithErrorsCount > 0 && _objectsToBuildWithErrorsNames != null)
            {
                GUILayout.Label("Can't build objects:", ObjectBuilderStyles.RedResultMessageStyle);
                foreach (string objectsToBuildWithError in _objectsToBuildWithErrorsNames)
                {
                    GUILayout.Label("    " + objectsToBuildWithError, ObjectBuilderStyles.RedResultInlineMessageStyle);
                }
            }
        }
        
        #endregion
        
        private void Update()
        {
            if (EditorApplication.isCompiling)
            {
                return;
            }

            try
            {
                if (IsFinished)
                {
                    Repaint();
                    return;
                }

                if (_varwinBuilder == null)
                {
                    _varwinBuilder = VarwinBuilder.Deserialize();

                    if (_varwinBuilder == null)
                    {
                        Exit();
                    }
                }
                
                UpdateLocalVariables();

                var objectsExistsNotExists = _varwinBuilder?.ObjectsToBuild == null || _varwinBuilder.ObjectsToBuild.Count == 0;
                var sceneTemplatesNotExists = _varwinBuilder?.ScenesToBuild == null || _varwinBuilder.ScenesToBuild.Count == 0;
                if (objectsExistsNotExists && sceneTemplatesNotExists)
                {
                    Exit();
                    EditorUtility.DisplayDialog("Build Results",  SdkTexts.NoSuitableForBuildObjectsFound, "OK");
                    return;
                }
                
                var objectToBuildWithErrors = _varwinBuilder.ObjectsToBuild.Where(x => x != null && x.HasError).ToList();
                if (_varwinBuilder.ObjectsToBuild.Where(x => !x.HasError).ToList().Count == 0)
                {
                    string message = "Build failed!";
                    if (objectToBuildWithErrors.Count > 0)
                    {
                        message += "\nCan't build objects: " + string.Join(", ", objectToBuildWithErrors.Select(x => x.ObjectName));
                    }
                    Exit();
                    EditorUtility.DisplayDialog("Build Results", message, "OK");
                    return;
                }

                if (_varwinBuilder is { IsStopped: true })
                {
                    return;
                }
            
                if (!IsFinished)
                {
                    if (_varwinBuilder.IsFinished && _varwinBuilder.DisplayProgress >= 0.995f)
                    {
                        Finish();
                        return;
                    }
                    
                    _varwinBuilder?.Update();
                }
                
                Repaint();
            }
            catch (Exception e)
            {
                ObjectBuilderHelper.DeleteTempFolder();
                
                string message = string.Format(SdkTexts.ProblemWhenBuildObjectsFormat, e.Message);
                EditorUtility.DisplayDialog("Error!", message, "OK");
                Debug.LogException(e);
                Exit();
                
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        private void UpdateLocalVariables()
        {
            if (_varwinBuilder == null)
            {
                return;
            }

            _displayProgress = Mathf.Max(_displayProgress, _varwinBuilder.DisplayProgress);
            
            _objectsToBuildCount = _varwinBuilder.ObjectsToBuild.Count;
            
            var objectToBuildWithErrors = _varwinBuilder.ObjectsToBuild.Where(x => x != null && x.HasError).ToList();
            _objectsToBuildWithErrorsCount = objectToBuildWithErrors.Count;
            if (_objectsToBuildWithErrorsCount > 0)
            {
                _objectsToBuildWithErrorsNames = objectToBuildWithErrors.Select(x => x.ObjectName).ToArray();
            }
        }

        private void Finish()
        {
            IsFinished = true;
            ObjectBuilderHelper.DeleteTempFolder();
            
            BuildTimeUtils.FinishTime = DateTime.Now;

            DeleteTempStateFile();
            Cleanup();
            DebugBuildingDuration();

            if (SdkSettings.Features.DynamicVersioning.Enabled)
            {
                EditorEx.ForceRecompile();
            }
        }

        private void Cleanup()
        {
            DeleteTempStateFile();
            DeleteWrappersIfNeeded();
        }

        private void Exit(bool withRefresh = true)
        {
            DeleteTempStateFile();
            Cleanup();
            Close();
            if (withRefresh)
            {
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }
        }

        private void DeleteTempStateFile()
        {
            if (File.Exists(VarwinBuilder.TempStateFilename))
            {
                if (VarwinBuilder.DeleteTempStateFile)
                {
                    File.Delete(VarwinBuilder.TempStateFilename);
                }
                else
                {
                    var newName = $"{VarwinBuilder.TempStateFilename.SubstringBefore(".json")}_{Guid.NewGuid().ToString().Replace("-", "")}.json";
                    File.Move(VarwinBuilder.TempStateFilename, newName);
                }
            }
        }

        private void DeleteWrappersIfNeeded()
        {
            if (!VarwinBuilder.DeleteWrappers)
            {
                return;
            }

            try
            {
                if (_varwinBuilder == null || _varwinBuilder.ObjectsToBuild == null)
                {
                    return;
                }

                foreach (var objectBuildDescription in _varwinBuilder.ObjectsToBuild)
                {
                    if (objectBuildDescription != null && objectBuildDescription.ContainedObjectDescriptor)
                    {
                        WrapperGenerator.RemoveWrapperIfNeeded(objectBuildDescription.ContainedObjectDescriptor);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Can't delete wrappers after build: {e}");
            }
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void DeleteTempBuildObjectsIfNeeded()
        {
            if (!File.Exists(VarwinBuilder.TempStateFilename) && !Instance)
            {
                ObjectBuilderHelper.DeleteTempFolder();
            }
        }
        
        public static bool VersionExists()
        {
            if (VarwinVersionInfo.Exists)
            {
                if (Application.unityVersion == VarwinVersionInfo.RequiredUnityVersion)
                {
                    return true;
                }

                var message = string.Format(SdkTexts.WrongUnityVersionBuildMessage, Application.unityVersion, VarwinVersionInfo.RequiredUnityVersion);

                Debug.LogError(message);
                EditorUtility.DisplayDialog("Unity version error", message, "Ok");

                return false;
            }
            
            EditorUtility.DisplayDialog(SdkTexts.VersionNotFoundTitle, SdkTexts.VersionNotFoundMessage, "OK");
            return false;
        }
    }
}

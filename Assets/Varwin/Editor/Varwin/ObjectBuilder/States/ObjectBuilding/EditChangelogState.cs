using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public class EditChangelogState : BaseObjectBuildingState
    {
        private ChangelogEditorWindow _changelogEditorWindow;
        
        private string _changelog;
        private bool _changelogWasChanged;
        private bool _changelogEditingFinished;
        
        public EditChangelogState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Waiting for changelog";
        }

        ~EditChangelogState()
        {
            UnsubscribeWindow(true);
        }
        
        protected override void OnEnter()
        {
            if (!SdkSettings.Features.Changelog.Enabled)
            {
                _changelogEditingFinished = true;
                _changelogWasChanged = false;
                _changelog = VarwinVersionInfoContainer.VersionNumber;
                return;
            }

            base.OnEnter();
            _changelogEditorWindow = OpenChangelogEditorWindow();
            _changelogEditorWindow.BuildButtonPressed += OnBuildButtonPressed;
            _changelogEditorWindow.CancelButtonPressed += OnCancelButtonPressed;
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            try
            {
                if (!_changelogEditingFinished)
                {
                    CurrentIndex = -1;
                    return;
                }

                if (_changelogWasChanged)
                {
                    foreach (var objectToBuild in ObjectBuildDescriptions)
                    {
                        objectToBuild.ContainedObjectDescriptor.Changelog = _changelog;
                    }
                }
                
                Exit();
            }
            catch (Exception e)
            {
                string message = $"{currentObjectBuildDescription.ObjectName} error: Problem when changelog editing object";
                Debug.LogError($"{message}\n{e}", currentObjectBuildDescription.ContainedObjectDescriptor);
                currentObjectBuildDescription.HasError = true;
                if (ObjectBuildDescriptions.Count == 1)
                {
                    EditorUtility.DisplayDialog("Error!", message, "OK");
                }

                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        protected override void OnExit()
        {
            base.OnExit();
            UnsubscribeWindow();
            Builder.Serialize();
        }

        private ChangelogEditorWindow OpenChangelogEditorWindow()
        {
            var changelogs = ObjectBuildDescriptions.Select(x => x.ContainedObjectDescriptor.Changelog).ToArray();

            var firstChangelogsElement = changelogs.FirstOrDefault();
            var allChangelogsAreEqual = changelogs.All(x => string.Equals(x, firstChangelogsElement, StringComparison.Ordinal));

            return ChangelogEditorWindow.OpenWindow(allChangelogsAreEqual ? firstChangelogsElement : "-");
        }

        private void OnBuildButtonPressed(ChangelogEditorWindow changelogEditorWindow, string changelog, bool isChanged)
        {
            UnsubscribeWindow(true);
            _changelogEditingFinished = true;
            _changelogWasChanged = isChanged;
            _changelog = changelog;
        }

        private void OnCancelButtonPressed(ChangelogEditorWindow changelogEditorWindow)
        {
            UnsubscribeWindow();
            Builder.Stop();
        }

        private void UnsubscribeWindow(bool close = false)
        {
            if (_changelogEditorWindow)
            {
                _changelogEditorWindow.BuildButtonPressed -= OnBuildButtonPressed;
                _changelogEditorWindow.CancelButtonPressed -= OnCancelButtonPressed;
                if (close && !_changelogEditorWindow.Destroyed)
                {
                    _changelogEditorWindow.Close();
                }
                _changelogEditorWindow = null;
            }
        }
    }
}
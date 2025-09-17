using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    public abstract class BaseCommonBuildingState : IBuildingState
    {
        public VarwinBuilder Builder { get; set; }
        
        protected int CurrentIndex;
        
        public string Label { get; protected set; }
        public bool IsFinished { get; protected set; }
        public float Progress { get; protected set; }
        
        protected List<ObjectBuildDescription> ObjectBuildDescriptions => Builder.ObjectsToBuild;
        protected List<SceneTemplateBuildDescription> SceneTemplateBuildDescriptions => Builder.ScenesToBuild;
        protected List<ResourceBuildDescription> ResourcesToBuild => Builder.ResourcesToBuild;
        protected VarwinBuildingData VarwinBuildingData => Builder.Data;

        protected enum BuildingState
        {
            Objects,
            Scenes,
            Resources
        }

        protected BuildingState State;

        public BaseCommonBuildingState(VarwinBuilder builder)
        {
            Builder = builder;
            CurrentIndex = -1;
            Progress = 0;
            IsFinished = false;
        }

        public void Initialize()
        {
            
        }

        public void Update()
        {
            if (IsFinished)
            {
                return;
            }

            if (State == BuildingState.Objects)
            {
                if (CurrentIndex < 0)
                {
                    OnEnter();
                    CurrentIndex = 0;
                }
                else if (ObjectBuildDescriptions != null && ObjectBuildDescriptions.Count > 0 && CurrentIndex < ObjectBuildDescriptions.Count)
                {
                    var currentObjectBuildDescription = ObjectBuildDescriptions[CurrentIndex];
                    if (currentObjectBuildDescription != null && !currentObjectBuildDescription.HasError)
                    {
                        Update(currentObjectBuildDescription);
                    }

                    CurrentIndex++;
                }
                else
                {
                    CurrentIndex = 0;
                    State = BuildingState.Scenes;
                }
            }
            else if (State == BuildingState.Scenes)
            {
                if (CurrentIndex < 0)
                {
                    CurrentIndex = 0;
                }
                else if (SceneTemplateBuildDescriptions != null && SceneTemplateBuildDescriptions.Count > 0 && CurrentIndex < SceneTemplateBuildDescriptions.Count)
                {
                    var sceneTemplateBuildDescription = SceneTemplateBuildDescriptions[CurrentIndex];
                    Update(sceneTemplateBuildDescription);

                    CurrentIndex++;
                }
                else
                {
                    CurrentIndex = 0;
                    State = BuildingState.Resources;
                }
            }
            else
            {
                if (CurrentIndex < 0)
                {
                    CurrentIndex = 0;
                }
                else if (ResourcesToBuild != null && ResourcesToBuild.Count > 0 && CurrentIndex < ResourcesToBuild.Count)
                {
                    var varwinResource = ResourcesToBuild[CurrentIndex];
                    Update(varwinResource);

                    CurrentIndex++;
                }
                else
                {
                    Exit();
                    return;
                }
            }

            var objectsCount = ObjectBuildDescriptions?.Count ?? 0; 
            var scenesCount = SceneTemplateBuildDescriptions?.Count ?? 0;
            var allEntitiesCount = objectsCount + scenesCount;
            if (allEntitiesCount == 0)
            {
                Progress = 1;
            }
            else
            {
                Progress = IsFinished ? 1f : Mathf.Clamp01(Mathf.Max(Progress, (float) CurrentIndex / allEntitiesCount));
            }
        }

        protected abstract void Update(ObjectBuildDescription objectBuildDescription);
        
        protected abstract void Update(ResourceBuildDescription resourceBuildDescription);

        protected abstract void Update(SceneTemplateBuildDescription sceneTemplateBuildDescription);
        
        protected virtual void OnEnter()
        {
            
        }

        protected virtual void OnExit()
        {
            
        }

        protected void Exit()
        {
            IsFinished = true;
            OnExit();
        }
    }
}
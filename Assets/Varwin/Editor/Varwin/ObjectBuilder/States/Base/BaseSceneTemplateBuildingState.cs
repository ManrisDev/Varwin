using System.Collections.Generic;
using UnityEngine;

namespace Varwin.Editor
{
    public class BaseSceneTemplateBuildingState : IBuildingState
    {
        public VarwinBuilder Builder { get; set; }
        
        protected int CurrentIndex;
        
        protected List<SceneTemplateBuildDescription> ScenesToBuild => Builder.ScenesToBuild;
        protected VarwinBuildingData VarwinBuildingData => Builder.Data;
        
        public string Label { get; protected set; }
        public bool IsFinished { get; protected set; }
        public float Progress { get; protected set; }

        public virtual bool IsSceneBuildFinished { get; }

        public BaseSceneTemplateBuildingState(VarwinBuilder builder)
        {
            Builder = builder;
            CurrentIndex = -1;
            Progress = 0;
            IsFinished = false;
        }

        public void Initialize()
        {
            OnInitialize();
        }

        public void Update()
        {
            if (IsFinished)
            {
                return;
            }

            if (ScenesToBuild == null || ScenesToBuild.Count == 0)
            {
                Progress = 1f;
                Exit();
                return;
            }
            
            if (CurrentIndex < 0)
            {
                OnEnter();
                CurrentIndex = 0;
            }
            else if (CurrentIndex < ScenesToBuild.Count)
            {
                var sceneTemplateBuildDescription = ScenesToBuild[CurrentIndex];
                Update(sceneTemplateBuildDescription);

                if (IsSceneBuildFinished)
                {
                    CurrentIndex++;
                }
            }
            else
            {
                Exit();
            }

            Progress = IsFinished ? 1f : Mathf.Clamp01(Mathf.Max(Progress, (float) CurrentIndex / ScenesToBuild.Count));
        }

        protected virtual void Update(SceneTemplateBuildDescription sceneTemplateBuildDescription)
        {
            Exit();
        }

        protected virtual void OnInitialize()
        {
            
        }
        
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
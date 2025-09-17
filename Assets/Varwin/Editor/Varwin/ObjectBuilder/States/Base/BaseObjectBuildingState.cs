using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public abstract class BaseObjectBuildingState : IBuildingState
    {
        public VarwinBuilder Builder { get; set; }
        
        protected int CurrentIndex;
        
        protected List<ObjectBuildDescription> ObjectBuildDescriptions => Builder.ObjectsToBuild;
        protected VarwinBuildingData VarwinBuildingData => Builder.Data;
        
        public string Label { get; protected set; }
        public bool IsFinished { get; protected set; }

        public float Progress { get; private set; }

        public BaseObjectBuildingState(VarwinBuilder builder)
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
            
            if (CurrentIndex < 0)
            {
                OnEnter();
                CurrentIndex = 0;
            }
            else if (CurrentIndex < ObjectBuildDescriptions.Count)
            {
                ObjectBuildDescription currentObjectBuildDescription = ObjectBuildDescriptions[CurrentIndex];
                if (currentObjectBuildDescription is { HasError: false })
                {
                    VarwinBuildStepNotifier.NotifyState(currentObjectBuildDescription.GameObject, GetType(), true);
                    Update(currentObjectBuildDescription);
                    VarwinBuildStepNotifier.NotifyState(currentObjectBuildDescription.GameObject, GetType(), false);
                }

                if (currentObjectBuildDescription.HasError)
                {
                    CreateObjectUtils.RestoreObjectFromBackup(currentObjectBuildDescription);
                }
                
                CurrentIndex++;
            }
            else
            {
                Exit();
            }

            Progress = IsFinished ? 1f : Mathf.Clamp01(Mathf.Max(Progress, (float) CurrentIndex / ObjectBuildDescriptions.Count));
        }

        protected virtual void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Exit();
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
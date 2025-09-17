using System;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Varwin.SceneTemplateBuilding
{
    public class SaveSceneStep : BaseSceneTemplateBuildStep
    {
        public SaveSceneStep(SceneTemplateBuilder builder) : base(builder)
        {
        }
        
        public override void Update()
        {
            base.Update();
            
            try
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            }
            catch (Exception e)
            {
                Debug.LogError($"Cannot save active scene.\n{e}");
            }
        }
    }
}
namespace Varwin.SceneTemplateBuilding
{
    public abstract class BaseSceneTemplateBuildStep
    {
        protected readonly SceneTemplateBuilder Builder;
        public bool HasErrors = false;
        
        public BaseSceneTemplateBuildStep(SceneTemplateBuilder builder)
        {
            Builder = builder;
        }
        
        public virtual void Update()
        {
            
        }
    }
}
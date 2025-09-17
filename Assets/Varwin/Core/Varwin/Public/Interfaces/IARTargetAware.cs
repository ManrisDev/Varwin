using UnityEngine;

namespace Varwin.Public
{
    public interface IARTargetBase : IVarwinInputAware
    {

    }

    public interface IARImageTargetAware : IARTargetBase
    {
        public Texture2D Image { get; }
        public float Size { get; }
    }

    public interface IARTargetFoundAware : IVarwinInputAware
    {
        void OnTargetFound();
    }

    public interface IARTargetLostAware : IVarwinInputAware
    {
        void OnTargetLost();
    }
    
    public interface IARTargetPreRenderAware : IVarwinInputAware
    {
        void OnTargetPreRender();
    }
    
    public interface IARTargetPostRenderAware : IVarwinInputAware
    {
        void OnTargetPostRender();
    }
}
namespace Varwin.Editor
{
    public interface IBuildingState
    {
        VarwinBuilder Builder { get; set; }
        
        string Label { get; }
        bool IsFinished { get; }
        float Progress { get; }
        void Initialize();
        void Update();
    }
}
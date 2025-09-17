namespace Varwin.PlatformAdapter
{
    public abstract class PointerController
    {
        public IMonoComponent<PointerManager> Managers;
        public PointerManager Left { get; protected set; }
        public PointerManager Right { get; protected set; }
        public abstract bool IsMenuOpened { set; }
        
        public void SetLeftManager(PointerManager manager)
        {
            Left = manager;
        }
        
        public void SetRightManager(PointerManager manager)
        {
            Right = manager;
        }
    }
}

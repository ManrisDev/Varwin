namespace Varwin.PlatformAdapter
{
    public abstract class PointerManager
    {
        public abstract bool IsMenuOpened { get; set; }
            
        public abstract bool ShowUIPointer { get; set; }
    }
}

namespace Varwin.PlatformAdapter
{
    public abstract class PlayerAppearance
    {
        public IMonoComponent<InteractControllerAppearance> ControllerAppearance;

        public abstract class InteractControllerAppearance
        {
            public abstract bool HideControllerOnGrab { set; }

            public abstract void DestroyComponent();
        }
    }
}
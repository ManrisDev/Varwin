namespace Varwin.PlatformAdapter
{
    public abstract class ARTrackingController
    {
        public IMonoComponent<TrackedObject> Image;

        public abstract class TrackedObject
        {
            public abstract bool IsTrackable { set; get; }

            public delegate void TrackingObjectEventHandler(object sender);
            public abstract event TrackingObjectEventHandler TargetFound;
            public abstract event TrackingObjectEventHandler TargetLost;
            public abstract event TrackingObjectEventHandler TargetPreRender;
            public abstract event TrackingObjectEventHandler TargetPostRender;

            public virtual void DestroyComponent()
            {
            }
        }
    }
}
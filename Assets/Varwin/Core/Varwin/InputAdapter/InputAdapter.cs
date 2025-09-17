using System.Collections.Generic;
using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public static class InputAdapter
    {
        public static SDKAdapter Instance { get; private set; }
        public static event InputAdapterChanged Changed;

        public static void ChangeCurrentAdapter(SDKAdapter adapter)
        {
            var prevInputAdapter = Instance;
            Instance = adapter;
            Changed?.Invoke(adapter, prevInputAdapter);
        }

        public delegate void InputAdapterChanged(SDKAdapter newInputAdapter, SDKAdapter prevInputAdapter);
    }

    public abstract class SDKAdapter
    {
        public PlayerAppearance PlayerAppearance { get; }
        public ControllerInput ControllerInput { get; }
        public ObjectInteraction ObjectInteraction { get; }
        public ControllerInteraction ControllerInteraction { get; }
        public PlayerController PlayerController { get; }
        public PointerController PointerController { get; }
        public IMonoComponent<PointableObject> PointableObject { get; }
        public IMonoComponent<Tooltip> Tooltip { get; }
        public ARTrackingController ARTracking { get; }

        protected SDKAdapter(
            PlayerAppearance playerAppearance,
            ControllerInput controllerInput,
            ObjectInteraction objectInteraction,
            ControllerInteraction controllerInteraction,
            PlayerController playerController,
            PointerController pointerController,
            IMonoComponent<PointableObject> pointableObject,
            IMonoComponent<Tooltip> tooltip, 
            ARTrackingController arTracking = null)
        {
            PlayerAppearance = playerAppearance;
            ControllerInput = controllerInput;
            ObjectInteraction = objectInteraction;
            ControllerInteraction = controllerInteraction;
            PlayerController = playerController;
            PointerController = pointerController;
            PointableObject = pointableObject;
            Tooltip = tooltip;
            ARTracking = arTracking;
        }
    }

    public interface IInitializable<in T> where T : Component
    {
        void Init(T monoBehaviour);
    }

    public interface IMonoComponent<out T>
    {
        T GetFrom(GameObject go);
        T GetFromChildren(GameObject go, bool includeInactive);
        T AddTo(GameObject go);
        T GetFrom(Transform transform);
        T GetFromChildren(Transform transform, bool includeInactive);
    }

    /// <summary>
    /// Base class for all of the fabric implementations.
    /// Provides a Unity-like methods for constructing and initialising wrap-classes over given MonoBehaviours.
    /// </summary>
    /// <typeparam name="TBase">Base type of a wrap-class to construct.</typeparam>
    /// <typeparam name="TWrap">Type of a wrap-class to construct. Must implement Varwin.VRInput.IInitable <typeparamref name="TComponent"/> \n
    /// interface.</typeparam>
    /// <typeparam name="TComponent">Type of MonoBehaviour that you want to wrap.</typeparam>
    public class ComponentWrapFactory<TBase, TWrap, TComponent> : IMonoComponent<TBase>
        where TBase : class
        where TWrap : TBase, IInitializable<TComponent>, new()
        where TComponent : Component
    {
        private readonly Dictionary<TComponent, TWrap> _wrapCollection = new Dictionary<TComponent, TWrap>();
        
        public virtual TBase GetFrom(GameObject go)
        {
            TComponent behComponent = go.GetComponent<TComponent>();

            return GetCreateWrap(behComponent);
        }

        public virtual TBase GetFromChildren(GameObject go, bool includeInactive)
        {
            TComponent behComponent = go.GetComponentInChildren<TComponent>(includeInactive);

            return GetCreateWrap(behComponent);
        }


        public virtual TBase AddTo(GameObject go)
        {
            TComponent behComponent = go.AddComponent<TComponent>();
            
            return GetCreateWrap(behComponent);
        }

        public virtual TBase GetFrom(Transform transform)
        {
            TComponent behComponent = transform.GetComponent<TComponent>();

            return GetCreateWrap(behComponent);
        }

        public virtual TBase GetFromChildren(Transform transform, bool includeInactive)
        {
            TComponent behComponent = transform.GetComponentInChildren<TComponent>(includeInactive);

            return GetCreateWrap(behComponent);
        }

        private TBase GetCreateWrap(TComponent behComponent)
        {
            if (!behComponent)
            {
                return null;
            }
            
            if (_wrapCollection.TryGetValue(behComponent, out TWrap ret))
            {
                return ret;
            }
            
            ret = new TWrap();
            ret.Init(behComponent);
            _wrapCollection[behComponent] = ret;

            return ret;
        }
    }

    public class ComponentFactory<TBase, TComponent> : IMonoComponent<TBase>
        where TBase : MonoBehaviour
        where TComponent : TBase
    {
        public TBase GetFrom(GameObject go) => go.GetComponent<TComponent>();

        public TBase GetFromChildren(GameObject go, bool includeInactive) =>
            go.GetComponentInChildren<TComponent>(includeInactive);

        public TBase AddTo(GameObject go) => go.AddComponent<TComponent>();

        public TBase GetFrom(Transform transform) => transform.GetComponent<TComponent>();

        public TBase GetFromChildren(Transform transform, bool includeInactive) =>
            transform.GetComponentInChildren<TComponent>(includeInactive);
    }

    public static class VectorExt
    {
        public static void ExtSetGlobalScale(this Transform transform, Vector3 globalScale)
        {
        }
    }
}

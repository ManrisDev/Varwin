using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Core.Behaviours.ConstructorLib;

namespace Varwin.Core.Behaviours
{
    public static class BehavioursCollection
    {
        private static HashSet<VarwinBehaviourContainer> _varwinBehaviourContainers;

        private static HashSet<VarwinBehaviourContainer> BehaviourContainers
        {
            get
            {
                if (_varwinBehaviourContainers == null)
                {
                    _varwinBehaviourContainers = new HashSet<VarwinBehaviourContainer>
                    {
                        // v1
                        new(typeof(MaterialChangeBehaviour), new MaterialChangeBehaviourHelper()),
                        new(typeof(MovableBehaviour), new MovableBehaviourHelper()),
                        new(typeof(ScalableBehaviour), new ScalableBehaviourHelper()),
                        new(typeof(InteractableBehaviour), new InteractableBehaviourHelper()),
                        new(typeof(LightBehaviour), new LightBehaviourHelper()),
                        // v2
                        new(typeof(RotateBehaviour), new RotateBehaviourHelper()),
                        new(typeof(PhysicsBehaviour), new PhysicsBehaviourHelper()),
                        new(typeof(ScaleBehaviour), new ScaleBehaviourHelper()),
                        new(typeof(MotionBehaviour), new MotionBehaviourHelper()),
                        new(typeof(VisualizationBehaviour), new VisualizationBehaviourHelper()),
                        new(typeof(InteractionBehaviour), new InteractionBehaviourHelper()),
                    };
                }

                return _varwinBehaviourContainers;
            }
        }

        public static List<System.Type> GetAllBehavioursTypes() => BehaviourContainers.Select(x => x.BehaviourType).ToList();

        public static List<string> GetBehaviours(ObjectController objectController)
        {
            var behaviours = new List<string>();

            foreach (var behaviourContainer in BehaviourContainers)
            {
                var behaviour = objectController.RootGameObject.GetComponentInChildren(behaviourContainer.BehaviourType) as VarwinBehaviour;
                if (behaviour == null)
                {
                    continue;
                }
                
                var wrapper = objectController.WrappersCollection.Get(objectController.Id);
                wrapper.AddBehaviour(behaviourContainer.BehaviourType, behaviour);
                behaviours.Add(behaviourContainer.BehaviourType.FullName);
            }

            return behaviours;
        }

        public static List<string> AddBehaviours(ObjectController objectController)
        {
            var behaviours = new List<string>();
            if (CanAddBehaviours(objectController))
            {
                AddBehaviours(objectController.RootGameObject, objectController.WrappersCollection.Get(objectController.Id), behaviours);
            }

            return behaviours;
        }

        public static bool IsAnOldTypeEnumBehaviour(Type behaviourType)
        {
            if (BehaviourContainers.FirstOrDefault(x => x.BehaviourType == behaviourType) == null)
            {
                return false;
            }

            return behaviourType.GetCustomAttributes(typeof(ObsoleteAttribute), true).Length > 0
                   || behaviourType == typeof(MaterialChangeBehaviour)
                   || behaviourType == typeof(MovableBehaviour)
                   || behaviourType == typeof(ScalableBehaviour)
                   || behaviourType == typeof(InteractableBehaviour)
                   || behaviourType == typeof(ScalableBehaviour)
                   || behaviourType == typeof(MaterialChangeBehaviourHelper)
                   || behaviourType == typeof(InteractableBehaviourHelper);
        }

        private static void AddBehaviours(GameObject gameObject, Wrapper wrapper, List<string> behaviours)
        {
            foreach (var behaviourContainer in BehaviourContainers)
            {
                if (behaviourContainer.CanAddBehaviour(gameObject))
                {
                    var behaviour = (VarwinBehaviour) gameObject.AddComponent(behaviourContainer.BehaviourType);
                    wrapper.AddBehaviour(behaviourContainer.BehaviourType, behaviour);
                    behaviours.Add(behaviourContainer.BehaviourType.FullName);

                    behaviour.Wrapper = wrapper;
                    behaviour.ObjectController = wrapper.GetObjectController();
                    behaviour.OnPrepare();
                }
            }
        }

        private static bool CanAddBehaviours(ObjectController objectController)
        {
            return !(objectController.IsEmbedded 
                     || objectController.IsVirtualObject
                     || objectController.IsSceneTemplateObject
                     || !objectController.VarwinObjectDescriptor
                     || !objectController.VarwinObjectDescriptor.AddBehavioursAtRuntime
                     || objectController.VarwinObjectDescriptor.Components.ComponentReferences.Any(x => x.Type.ToString().Contains("Varwin.ConstructorLib.v1")));
        }
    }
}


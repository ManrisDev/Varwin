using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public abstract class ConstructorVarwinBehaviour : VarwinBehaviour
    {
        protected  readonly WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
        protected class BehaviourState
        {
            public bool IsPaused;
            public bool IsMoving;
            public bool IsRotating;
            public bool IsScaling;
            public bool IsPerforming;
        }
        
        protected static List<Transform> FillLockedHierarchyList(ObjectController highestLocked, List<Transform> transformsToMove)
        {
            transformsToMove.Add(highestLocked.gameObject.transform);

            foreach (var objectController in highestLocked.Children)
            {
                transformsToMove = FillLockedHierarchyList(objectController, transformsToMove);
            }

            return transformsToMove;
        }
        
        protected static List<Transform> GetHierarchy(GameObject objInHierarchy)
        {
            var lockedInHierarchyObjects = new List<Transform>();

            var objectController = objInHierarchy.GetWrapper()?.GetObjectController();

            if (objectController is {LockChildren: true})
            {
                return FillLockedHierarchyList(objectController, lockedInHierarchyObjects);
            }

            lockedInHierarchyObjects.Add(objInHierarchy.transform);

            return lockedInHierarchyObjects;
        }
    }
}
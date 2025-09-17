using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.ObjectsInteractions;
using Varwin.Public;

namespace Varwin.PlatformAdapter
{
    public class TeleportThroughCollisionChecker : MonoBehaviour
    {
        [SerializeField] private Transform _headTransform;

        private HashSet<Collider> _colliders = new HashSet<Collider>();
        
        public bool PossibleToTeleport
        {
            get
            {
                foreach (var coll in _colliders.ToList())
                {
                    if (!coll || !coll.enabled || !coll.gameObject.activeInHierarchy || CheckIfColliderInHand(coll)) 
                    {
                        _colliders.Remove(coll);
                    }
                }
  
                if (!HandInsideCollider && !_possibleToTeleport)
                {
                    _possibleToTeleport = HeadConnectionCheck();
                }
                
                return _possibleToTeleport;
            }
        }
        private bool _possibleToTeleport = true;

        private bool HandInsideCollider => _colliders.Count != 0;

        private void OnTriggerEnter(Collider other)
        {
            if (!other || other.isTrigger ||_colliders.Contains(other) || other.gameObject.layer != 0 || other.GetComponent<CollisionControllerElement>())
            {
                return;
            }

            if (!CheckIfColliderInHand(other))
            {
                _colliders.Add(other);
                _possibleToTeleport = false;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other && _colliders.Contains(other))
            {
                if (CheckIfColliderInHand(other))
                {
                    _colliders.Remove(other);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other && _colliders.Contains(other))
            {
                _colliders.Remove(other);
            }
        }

        private bool CheckIfColliderInHand(Collider other)
        {
            Transform currentGrabbedL = InputAdapter.Instance?.PlayerController?.Nodes?.LeftHand?.Controller?.GetGrabbedObject()?.transform;
            Transform currentGrabbedR = InputAdapter.Instance?.PlayerController?.Nodes?.RightHand?.Controller?.GetGrabbedObject()?.transform;

            if (!other)
            {
                return false;
            }

            Transform grabbedInHand = other.transform;

            if (!grabbedInHand)
            {
                return false;
            }

            var objectDescriptorReached = false;

            do
            {
                if (grabbedInHand.GetComponent<VarwinObjectDescriptor>())
                {
                    objectDescriptorReached = true;
                }

                if (currentGrabbedL == grabbedInHand || currentGrabbedR == grabbedInHand)
                {
                    return true;
                }

                grabbedInHand = grabbedInHand.parent;
            } while (grabbedInHand && !objectDescriptorReached);

            return false;
        }

        private bool HeadConnectionCheck()
        {
            Vector3 handPosition = transform.position;
            Vector3 headPosition = _headTransform.position;
            
            Vector3 toHandDirection = handPosition - headPosition;
            float distanceToHead = toHandDirection.magnitude;
            
            Ray rayToHand = new Ray(handPosition, -toHandDirection);
            Ray rayToHead = new Ray(headPosition, toHandDirection);

            RaycastHit[] toHead = new RaycastHit[6];
            RaycastHit[] toHand = new RaycastHit[6];
            
            var sizeToHand = Physics.RaycastNonAlloc(rayToHand, toHand, distanceToHead, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            var sizeToHead = Physics.RaycastNonAlloc(rayToHead, toHead, distanceToHead, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

            var countOfHits = sizeToHand + sizeToHead;
            
            if (countOfHits != 0)
            {
                countOfHits -= CheckArrayOfHits(toHand, sizeToHand);
                countOfHits -= CheckArrayOfHits(toHead, sizeToHead);
            }
  
            return countOfHits == 0;
        }

        private int CheckArrayOfHits(RaycastHit[] hits, int countOfHits)
        {
            int count = 0;
            for (int i = 0; i < countOfHits; i++)
            {
                if (CheckIfColliderInHand(hits[i].collider))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
using System;
using Core.Varwin.Extensions;
using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public abstract class PlayerController : IPlayerController
    {
        public PlayerTracking Tracking;
        public PlayerRigInitializer RigInitializer;
        public PlayerNodes Nodes;
        public PlayerControls Controls;

        public delegate void TeleportHandler(Vector3 position);

        public delegate void RotateHandler(Quaternion newRotation, Quaternion oldRotation);
        public event TeleportHandler PlayerTeleported;
        public event RotateHandler PlayerRotated;
        
        public event Action PlayerPreTeleport;
        public event Action PlayerPreRotate;

        public abstract class PlayerTracking
        {
            public abstract GameObject GetBoundaries(Transform transform);

            public abstract bool IsHeadsetOnHead();

            public delegate void TrackingEvent(PlayerNodes.Node node);

            public event TrackingEvent TrackingLost;
            public event TrackingEvent TrackingRestored;

            public abstract void SetHmdState(HmdState state);

            public enum HmdState
            {
                OnHead,
                OffHead
            }
        }

        public abstract class PlayerRigInitializer
        {
            public abstract GameObject InitializeRig();
        }

        public class ControllerReferenceArgs
        {
            public ControllerInteraction.ControllerHand hand;
        }

        public virtual void Teleport(Vector3 position)
        {
            PlayerTeleported?.Invoke(position);
        }
        
        
        public virtual void PreTeleport()
        {
            PlayerPreTeleport?.Invoke();
        }

        public virtual void PreRotate()
        {
            PlayerPreRotate?.Invoke();
        }

        public virtual void Rotate(Quaternion newRotation, Quaternion oldRotation)
        {
            PlayerRotated?.Invoke(newRotation, oldRotation);
        }

        public virtual void ResetVelocity()
        {
            
        }

        public abstract class PlayerNodes
        {
            public Node Rig { get; protected set; }
            public Node Head { get; protected set; }
            public virtual Node LeftEye => Head;
            public virtual Node RightEye => Head;
            public ControllerNode LeftHand { get; protected set; }
            public ControllerNode RightHand { get; protected set; }

            public abstract void Init(GameObject rig);

            public abstract ControllerNode GetControllerReference(GameObject controller);

            public virtual ControllerNode GetControllerReference(ControllerInteraction.ControllerHand hand)
            {
                switch (hand)
                {
                    case ControllerInteraction.ControllerHand.Left: return LeftHand;
                    case ControllerInteraction.ControllerHand.Right: return RightHand;
                }

                return null;
            }
            
            public abstract class Node
            {
                public virtual GameObject GameObject { get; protected set; }
                public virtual Transform Transform { get; protected set; }

                public virtual void SetNode(GameObject gameObject)
                {
                    GameObject = gameObject;
                    Transform = gameObject.transform;
                }
            }

            public abstract class ControllerNode : Node
            {
                public abstract ControllerInteraction.ControllerSelf Controller { get; protected set; }
                public abstract PointerManager PointerManager { get; protected set; }
            }

            public abstract GameObject GetModelAliasController(GameObject controllerEventsGameObject);

            public abstract ControllerInteraction.ControllerHand
                GetControllerHand(GameObject controllerEventsGameObject);


            public abstract string GetControllerElementPath(
                ControllerInteraction.ControllerElements findElement,
                ControllerInteraction.ControllerHand controllerHand,
                bool b);
        }


        public abstract class PlayerControls
        {
            public abstract void RotatePlayer(float angle);
        }

        public abstract void Init(GameObject rig);
        
        public virtual Quaternion Rotation
        {
            get => GetRotation();
            set
            {
                if (TryGetRigTransform(out _))
                {
                    var yAngle = InputAdapter.Instance?.PlayerController?.Nodes?.Head?.Transform?.rotation.GetSignedYAngleBetween(value) ?? 0f;
                    InputAdapter.Instance?.PlayerController?.Controls?.RotatePlayer(yAngle);
                }
            }
        }

        public virtual Vector3 Position
        {
            get
            {
                if (!InputAdapter.Instance.PlayerController.Nodes.Rig.Transform)
                {
                    return PlayerAnchorManager.SpawnPoint ? PlayerAnchorManager.SpawnPoint.position : Vector3.zero;
                }

                return InputAdapter.Instance.PlayerController.Nodes.Rig.Transform.position;
            }
            set
            {
                Transform rigTransform;
                if (TryGetRigTransform(out rigTransform))
                {
                    rigTransform.position = value;
                }
            }
        }

        private bool TryGetRigTransform(out Transform rigTransform)
        {
            rigTransform = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;
            if (rigTransform == null)
            {
                Debug.LogWarning("Player transform equals null.");
                return false;
            }

            return true;
        }

        public virtual Quaternion GetRotation()
        {
            if (InputAdapter.Instance.PlayerController.Nodes.Rig.Transform == null)
            {
                return PlayerAnchorManager.SpawnPoint.rotation;
            }

            return InputAdapter.Instance.PlayerController.Nodes.Rig.Transform.rotation;
        }

        public virtual void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public virtual void SetRotation(Quaternion rotation)
        {
            Rotation = rotation;
        }

        public void CopyTransform(Transform targetTransform)
        {
            Position = targetTransform.position;
            Rotation = targetTransform.rotation;
        }
    }
}

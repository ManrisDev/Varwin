using System;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.DesktopInput
{
    public class SpectatorPlayerController : PlayerController
    {
        public override Vector3 Position
        {
            get => SpectatorPlayer.SpectatorPlayerController.Instance ? SpectatorPlayer.SpectatorPlayerController.Instance.Position : Vector3.zero;
            set => SpectatorPlayer.SpectatorPlayerController.Instance.SetPosition(value);
        }

        public override Quaternion Rotation
        {
            get => SpectatorPlayer.SpectatorPlayerController.Instance ? SpectatorPlayer.SpectatorPlayerController.Instance.Rotation : Quaternion.identity;
            set => SpectatorPlayer.SpectatorPlayerController.Instance.SetRotation(value);
        }

        public SpectatorPlayerController()
        {
            Tracking = new DesktopPlayerTracking();
            RigInitializer = new DesktopPlayerRigInitializer();
            Nodes = new DesktopPlayerNodes();
            Controls = new DesktopPlayerControls();
        }
        
        public override void Init(GameObject rig)
        {
            Nodes.Init(rig);
        }

        public override void Teleport(Vector3 position)
        {
            PreTeleport();
            
            Transform player = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;
            if (!player)
            {
                Debug.LogError("Can not teleport! Player not found");
                return;
            }

            Transform head = InputAdapter.Instance.PlayerController.Nodes.Head.Transform;
            if (!head)
            {
                Debug.LogError("Can not teleport! Head not found");
                return;
            }

            Vector3 playerPosition = position;
            Vector3 headDelta = player.position - head.position;

            headDelta.y = 0;
            playerPosition = playerPosition + headDelta;
            player.position = playerPosition;
            
            base.Teleport(position);
        }

        public override void SetPosition(Vector3 position)
        {
            Teleport(position);
        }
        
        public override void SetRotation(Quaternion rotation)
        {
            DesktopPlayer.DesktopPlayerController.Instance.SetRotation(rotation);
        }
        
        private class DesktopPlayerTracking : PlayerTracking
        {
            public override GameObject GetBoundaries(Transform transform) => transform.gameObject;

            public override bool IsHeadsetOnHead() => ProjectData.IsPlayMode;

            public override void SetHmdState(HmdState state)
            {
                switch (state)
                {
                    case HmdState.OnHead:
                        TrackingManager.OnHmdOnHead();
                        break;
                    case HmdState.OffHead:
                        TrackingManager.OnHmdOffHead();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
        }
        
        private class DesktopPlayerRigInitializer : PlayerRigInitializer
        {
            public override GameObject InitializeRig()
            {
                return Resources.Load<GameObject>("PlayerRig/SpectatorPlayer");
            }
        }
        
        private class DesktopPlayerNodes : PlayerNodes
        {
            public DesktopPlayerNodes()
            {
                Rig = new DesktopNode();
                Head = new DesktopNode();
                RightHand = new DesktopControllerNode();
                LeftHand = RightHand;
            }
            
            public override void Init(GameObject rig)
            {
                GameObject camera = rig.GetComponentInChildren<Camera>()?.gameObject;
                Head?.SetNode(camera);
                Rig.SetNode(rig);
            }

            public override ControllerNode GetControllerReference(GameObject controller)
            {
                return RightHand;
            }

            public override GameObject GetModelAliasController(GameObject controllerEventsGameObject)
            {
                throw new NotImplementedException();
            }

            public override ControllerInteraction.ControllerHand GetControllerHand(GameObject controllerEventsGameObject)
            {
                return ControllerInteraction.ControllerHand.Right;
            }

            public override string GetControllerElementPath(ControllerInteraction.ControllerElements findElement, 
                ControllerInteraction.ControllerHand controllerHand, bool b)
            {
                throw new System.NotImplementedException();
            }
            
            private class DesktopNode : Node
            { }
            
            private class DesktopControllerNode : ControllerNode
            {
                public override ControllerInteraction.ControllerSelf Controller { get; protected set; }
                public override PointerManager PointerManager { get; protected set; }
                
                public override void SetNode(GameObject gameObject)
                {
                    Controller = InputAdapter.Instance.ControllerInteraction.Controller.GetFrom(gameObject);
                    PointerManager = InputAdapter.Instance.PointerController.Managers.GetFrom(gameObject);
                    base.SetNode(gameObject);
                }
            }
        }
        
        private class DesktopPlayerControls : PlayerControls
        {
            public override void RotatePlayer(float angle)
            {
                throw new NotImplementedException();
            }
        }
    }
}

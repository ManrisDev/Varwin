using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    public class VarwinXRPlayerController : PlayerController
    {
        private VarwinXRPlayerMoveController _moveController;
        
        public override Vector3 Position
        {
            get => GetRigPosition();
            set => SetRigPosition(value);
        }

        public VarwinXRPlayerController()
        {
            Tracking = new VarwinXRPlayerTracking();
            RigInitializer = new VarwinXRPlayerRigInitializer();
            Nodes = new VarwinXRPlayerNodes();
            Controls = new VarwinXRPlayerControls();
        }

        public override void Teleport(Vector3 position)
        {
            PreTeleport();
            SetRigPosition(position);
            base.Teleport(position);
        }

        private void SetRigPosition(Vector3 position)
        {
            _moveController.SetRigPosition(position);
        }
        
        private Vector3 GetRigPosition()
        {
            return _moveController.GetPosition();
        }

        public override void ResetVelocity()
        {
            _moveController.ResetVelocity();
        }

        public override void Init(GameObject rig)
        {
            Nodes.Init(rig);
            _moveController = rig.GetComponent<VarwinXRPlayerMoveController>();

            ((VarwinXRPlayerControls) InputAdapter.Instance.PlayerController.Controls).Init(rig.transform, this);
        }
        
        public class VarwinXRPlayerNodes : PlayerNodes
        {
            public override Node LeftEye { get; }
            public override Node RightEye { get; }

            public VarwinXRPlayerNodes()
            {
                Rig = new VarwinXRNode();
                Head = new VarwinXRNode();
                LeftHand = new VarwinXRControllerNode();
                RightHand = new VarwinXRControllerNode();
                LeftEye = new VarwinXRNode();
                RightEye = new VarwinXRNode();
            }

            public override void Init(GameObject rig)
            {
                GameObject go = rig.GetComponentInChildren<Camera>().gameObject;
                Head?.SetNode(go);
                Rig.SetNode(rig);

                InitializeControllers(rig);
                InitializeEyeTracking(rig);
            }

            private void InitializeEyeTracking(GameObject rig)
            {
                var eyeManager  = rig.GetComponentInChildren<EyeTrackingManager>();
                LeftEye.SetNode(eyeManager.LeftEyeTransform.gameObject);
                RightEye.SetNode(eyeManager.RightEyeTransform.gameObject);
            }

            private void InitializeControllers(GameObject rig)
            {
                var hands = rig.GetComponentsInChildren<VarwinXRController>();
                foreach (var hand in hands)
                {
                    if (hand.IsLeft)
                    {
                        LeftHand.SetNode(hand.gameObject);
                    }
                    else
                    {
                        RightHand.SetNode(hand.gameObject);
                    }
                }
            }

            public override ControllerNode GetControllerReference(GameObject controller)
            {
                var hand = controller.GetComponent<VarwinXRControllerModel>();

                if (!hand)
                {
                    return null;
                }

                return hand.IsLeftHand ? LeftHand : RightHand;
            }

            public override ControllerNode GetControllerReference(ControllerInteraction.ControllerHand hand)
            {
                switch (hand)
                {
                    case ControllerInteraction.ControllerHand.Left: return LeftHand;
                    case ControllerInteraction.ControllerHand.Right: return RightHand;
                }

                return null;
            }

            private class VarwinXRNode : Node
            {
            }

            private class VarwinXRControllerNode : ControllerNode
            {
                public override void SetNode(GameObject gameObject)
                {
                    Controller = InputAdapter.Instance.ControllerInteraction.Controller.GetFrom(gameObject);
                    PointerManager = InputAdapter.Instance.PointerController.Managers.GetFrom(gameObject);
                    base.SetNode(gameObject);
                }

                public override ControllerInteraction.ControllerSelf Controller { get; protected set; }
                public override PointerManager PointerManager { get; protected set; }
            }

            public override GameObject GetModelAliasController(GameObject controllerEventsGameObject) => throw new NotImplementedException();

            public override ControllerInteraction.ControllerHand GetControllerHand(GameObject controllerEventsGameObject)
            {
                if (!controllerEventsGameObject)
                {
                    return ControllerInteraction.ControllerHand.None;
                }

                var controller = controllerEventsGameObject.GetComponentInChildren<VarwinXRController>(true);

                return controller
                    ? (controller.IsLeft ? ControllerInteraction.ControllerHand.Left : ControllerInteraction.ControllerHand.Right)
                    : ControllerInteraction.ControllerHand.None;
            }

            public override string GetControllerElementPath(ControllerInteraction.ControllerElements findElement, ControllerInteraction.ControllerHand controllerHand, bool b) =>
                throw new NotImplementedException();
        }

        private class VarwinXRPlayerRigInitializer : PlayerRigInitializer
        {
            private GameObject _rig;

            public override GameObject InitializeRig()
            {
                _rig = Resources.Load<GameObject>("PlayerRig/VarwinXRPlayer");
                return _rig;
            }
        }

        private class VarwinXRPlayerControls : PlayerControls
        {
            private Transform _player;
            private PlayerController _controller;
            private const int TurnAngle = 45;

            public void Init(Transform playerRig, PlayerController controller)
            {
                _player = playerRig;
                _controller = controller;

                ((VarwinXRControllerInput) InputAdapter.Instance.ControllerInput).TurnLeftPressed +=
                    (sender, args) =>
                    {
                        if (!PlayerManager.TurnInVREnabled)
                        {
                            return;
                        }
                        
                        Turn(true);
                    };

                ((VarwinXRControllerInput) InputAdapter.Instance.ControllerInput).TurnRightPressed +=
                    (sender, args) =>
                    {
                        if (!PlayerManager.TurnInVREnabled)
                        {
                            return;
                        }

                        Turn(false);
                    };
            }

            public void Turn(bool isLeft)
            {
                _controller.PreRotate();

                var oldRotation = _player.rotation;
                if (isLeft)
                {
                    RotatePlayer(-TurnAngle);
                }
                else
                {
                    RotatePlayer(TurnAngle);
                }

                _controller.Rotate(_player.rotation, oldRotation);
            }

            public override void RotatePlayer(float angle)
            {
                Vector3 headPos = InputAdapter.Instance.PlayerController.Nodes.Head.Transform.position;
                Transform player = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;

                Vector3 headDelta = player.position - headPos;
                headDelta.y = 0;
                Vector3 rigPosition = new Vector3(headPos.x, player.position.y, headPos.z);

                _player.position = rigPosition + Quaternion.AngleAxis(angle, Vector3.up) * headDelta;
                _player.Rotate(_player.up, angle);
            }
        }

        private class VarwinXRPlayerTracking : PlayerTracking
        {
            private const int LoadScreenSceneId = 0;
            private static GameObject _steamVRStateListener;
            private InputDevice? _inputDevice;

            public VarwinXRPlayerTracking()
            {
                if (ProjectData.SceneId == LoadScreenSceneId)
                {
                    ProjectData.SceneLoaded += InitializeStateUpdaterOnSceneLoad;
                }
                else
                {
                    InitializeStateUpdater();
                }

                LoadInputDevice();
            }

            private void InitializeStateUpdater()
            {
#if !VARWIN_SDK
                if (_steamVRStateListener)
                {
                    UnityEngine.Object.Destroy(_steamVRStateListener);
                }

                _steamVRStateListener = new GameObject("SteamVR Head States Listener");
                _steamVRStateListener.transform.parent = InputAdapter.Instance.PlayerController.Nodes.Rig.Transform;
             //   _steamVRStateListener.AddComponent<SteamVRHeadsetStatesUpdater>();
#endif
            }

            private void InitializeStateUpdaterOnSceneLoad()
            {
                ProjectData.SceneLoaded -= InitializeStateUpdaterOnSceneLoad;
                InitializeStateUpdater();
            }

            public override GameObject GetBoundaries(Transform transform) => transform.gameObject;

            public override bool IsHeadsetOnHead() => true;//OpenVR.System.GetTrackedDeviceActivityLevel(0) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction;

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

            private void LoadInputDevice()
            {
                // experimental, maybe not work
                var devices = new List<InputDevice>();
                InputDevices.GetDevices(devices);
                _inputDevice = devices.FirstOrDefault();
            }

            ~VarwinXRPlayerTracking()
            {
                if (_steamVRStateListener)
                {
                    UnityEngine.Object.Destroy(_steamVRStateListener);
                    _steamVRStateListener = null;
                }
            }
        }
    }
}
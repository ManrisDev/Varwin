using UnityEngine;

namespace Varwin.PlatformAdapter
{
    public class TeleportPointer : MonoBehaviour, IBasePointer
    {
        public static bool TeleportEnabled = true;
        public static bool HideArcOnDisabledTeleport = false;

        public float Velocity = 10f;
        public float DotUp = 0.9f;

        private Arc _arc;
        protected Transform _pointerOrigin;
        private ControllerInput.ControllerEvents _events;

        private bool _active;
        private bool _click;

        private float _desktopArcOffsetModifier = 0.1f;
        private float _defaultArcOffsetModifier = 0.0f;

        private TeleportThroughCollisionChecker _checkerCollisionChecker;

        private TeleportThroughCollisionChecker CollisionChecker
        {
            get
            {
                if (!_checkerCollisionChecker)
                {
                    _checkerCollisionChecker = GetComponent<TeleportThroughCollisionChecker>();
                }

                return _checkerCollisionChecker;
            }
        }

        private GameObject _destinationReticle;

        private GameObject DestinationReticle
        {
            get
            {
                if (!_destinationReticle)
                {
                    _destinationReticle = Instantiate(Resources.Load<GameObject>("Teleport/DestinationReticle"));
                    _destinationReticle.gameObject.layer = LayerMask.NameToLayer("Highlight");
                }

                return _destinationReticle;
            }
        }

        public virtual bool CanRelease() => _events.IsTouchpadReleased() && CanTeleport();

        public virtual bool CanTeleport() => TeleportEnabled && _arc.IsArcValid();

        public virtual bool CanToggle() => _events.IsTouchpadPressed();
        public virtual bool CanPress() => false;

        public virtual void Toggle(bool value)
        {
            if (_active == value)
            {
                return;
            }

            _active = value;
            UpdateArc(_active);
        }

        public virtual void Toggle()
        {
            _active = !_active;
            UpdateArc(_active);
        }

        public virtual bool IsActive() => _active;

        protected virtual void UpdateArc(bool state)
        {
            if (state)
            {
                _arc.Show();

                return;
            }

            _arc.Hide();
            DestinationReticle.SetActive(false);
        }

        public virtual void Press()
        {
        }

        public virtual void Release()
        {
            TeleportPlayer();
            DestinationReticle.SetActive(false);
        }

        public virtual void Init()
        {
            var origins = transform.Find("PointerOrigins");

            if (origins)
            {
                if (DeviceHelper.IsOculus)
                {
                    _pointerOrigin = origins.Find("Oculus");
                }
                else if (DeviceHelper.IsWmr)
                {
                    _pointerOrigin = origins.Find("WMR");
                }
                else
                {
                    _pointerOrigin = origins.Find("Generic");
                }

                if (!_pointerOrigin)
                {
                    _pointerOrigin = transform;
                }
            }
            else
            {
                _pointerOrigin = transform;
            }

            _arc = GetComponent<Arc>();
            
            if (!_arc)
            {
                var arcGameObject = new GameObject("TeleportPointer");
                arcGameObject.transform.parent = transform;
                arcGameObject.transform.localPosition = Vector3.zero;
                arcGameObject.transform.localRotation = Quaternion.identity;
                arcGameObject.transform.localScale = Vector3.one;
                arcGameObject.layer = LayerMask.NameToLayer("Highlight");
                _arc = arcGameObject.AddComponent<Arc>();
            }
            
            _arc.ControllerHand = InputAdapter.Instance.PlayerController.Nodes.GetControllerHand(gameObject);

            _arc.TraceLayerMask = ~((1 << LayerMask.NameToLayer("Ignore Raycast"))
                                    | (1 << LayerMask.NameToLayer("Player"))
                                    | (1 << LayerMask.NameToLayer("Zones")));

            _events = InputAdapter.Instance.ControllerInput.ControllerEventFactory.GetFrom(gameObject);
        }

        protected virtual void DisableIfNeeded()
        {
        }

        public virtual void UpdateState()
        {
            DisableIfNeeded();

            Transform originTransform = _pointerOrigin ? _pointerOrigin : transform;
            var arcOffsetModifier = ProjectData.PlatformMode == PlatformMode.Desktop
                ? _desktopArcOffsetModifier
                : _defaultArcOffsetModifier;
            Vector3 arcPosition = originTransform.position - originTransform.forward * arcOffsetModifier;
            Vector3 forward = originTransform.forward;
            Vector3 pointerDir = forward;
            Vector3 arcVelocity = pointerDir * Velocity;
            float dotUp = Vector3.Dot(pointerDir, Vector3.up);
            float dotForward = Vector3.Dot(pointerDir, forward);
            bool pointerAtBadAngle = dotForward > 0 && dotUp > DotUp || dotForward < 0.0f && dotUp > 0.5f;

            bool badTeleporting = false;
            if (ProjectData.PlatformMode == PlatformMode.Desktop)
            {
                badTeleporting = IfBackwardTeleportation(_arc.PlayerTeleportPositionCandidate);
            }
            else if (CollisionChecker)
            {
                badTeleporting = !CollisionChecker.PossibleToTeleport;
            }

            badTeleporting |= !IfTeleportAreaFlatEnough(_arc.PlayerTeleportNormalCandidate);

            _arc.SetArcData(arcPosition,
                arcVelocity,
                true,
                pointerAtBadAngle,
                badTeleporting);

            bool hitSomeThing = false;

            if (!_active)
            {
                return;
            }

            hitSomeThing = (TeleportEnabled || !HideArcOnDisabledTeleport) && _arc.DrawArc(out RaycastHit hitInfo);

            if (hitSomeThing && _arc.IsArcValid())
            {
                DestinationReticle.SetActive(true);
                DestinationReticle.transform.position = _arc.PlayerTeleportPositionCandidate;
            }
            else
            {
                DestinationReticle.SetActive(false);
            }
        }

        protected virtual void TeleportPlayer()
        {
            Vector3 teleportPoint = _arc.PlayerTeleportPositionCandidate;
            InputAdapter.Instance.PlayerController.Teleport(teleportPoint);
        }

        protected virtual bool IfBackwardTeleportation(Vector3 supposedPos)
        {
            var cam = transform.parent;
            var playerTransform = cam.parent;

            var cameraForward = playerTransform.forward;
            var cameraPlane = new Plane(cameraForward, playerTransform.position);

            return !cameraPlane.GetSide(supposedPos);
        }

        protected virtual bool IfTeleportAreaFlatEnough(Vector3 normal)
        {
            var angle = Vector3.Angle(normal, Vector3.up);
            return angle < PlayerManager.TeleportAngleLimit;
        }
    }
}
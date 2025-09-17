using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.PlatformAdapter;

namespace Varwin.XR
{
    [ExecuteAlways]
    public class HeadCollisionFadeHandler : MonoBehaviour
    {
        private const float MaxIntersectionDistance = 0.05f;
        private const int MaxIntersectingColliders = 15;

        private static readonly int WorldDirectionParameterId = Shader.PropertyToID("_WorldDirection");
        private static readonly int ForceParameterId = Shader.PropertyToID("_Force");

        [SerializeField] private SphereCollider _collider;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private AnimationCurve _fadeCurve;
        [SerializeField] private LayerMask _ignoreMask;
        [SerializeField] private Transform _playerCollidersRoot;

        private Renderer _fadeRenderer;
        private MaterialPropertyBlock _propertyBlock;
        private Collider[] _allNeighbours;
        private Collider[] _matchedNeighbours;
        private HashSet<Collider> _selfIgnoreColliders;

        private int _interactionsCount;
        private bool _fading;
        private Vector3 _headEnterPoint;

        private bool Fading
        {
            get => _fading;
            set
            {
                if (_fading == value)
                {
                    return;
                }

                var isStartFading = !_fading && value;

                _fading = value;
                HeadCollisionFadeHelper.IsHeadFading = _fading;

                if (isStartFading)
                {
                    HeadCollisionFadeHelper.InvokeHeadCollisionStartedEvent(_matchedNeighbours);
                }
                else
                {
                    HeadCollisionFadeHelper.InvokeHeadCollisionEndedEvent();
                }
            }
        }

        private Collider[] Neighbours
        {
            get => _matchedNeighbours;
            set
            {
                _matchedNeighbours = value;
                HeadCollisionFadeHelper.CollidingColliders = _matchedNeighbours;
            }
        }

        private void Start()
        {
            _allNeighbours = new Collider[MaxIntersectingColliders];

            Initialize();

            SetFade(0, default);
        }

        private void Initialize()
        {
            if (!_headTransform)
            {
                _headTransform = transform;
            }

            if (!_collider)
            {
                _collider = gameObject.AddComponent<SphereCollider>();
                _collider.radius = MaxIntersectionDistance;
            }

            _collider.isTrigger = true;
            _propertyBlock = new();

            _ignoreMask = LayerMask.GetMask("Player", "Ignore Raycast", "UI", "Zones", "VRControllers", "PostProcessing");
            _selfIgnoreColliders = _playerCollidersRoot.GetComponentsInChildren<Collider>(true).ToHashSet();
        }

#if UNITY_EDITOR

        private void OnEnable()
        {
            Start();
        }

        public void OnDrawGizmos()
        {
            if (!_collider || !Fading)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, _headEnterPoint);
            Gizmos.DrawWireSphere(_collider.transform.position, 0.05f);
        }
#endif

        private void LateUpdate()
        {
            if (ProjectData.GameMode == GameMode.Edit || !_collider)
            {
                return;
            }

            var leftController = InputAdapter.Instance?.PlayerController.Nodes.LeftHand?.Controller;
            var rightController = InputAdapter.Instance?.PlayerController.Nodes.RightHand?.Controller;

            Neighbours = _allNeighbours[.._interactionsCount]
                .Where(otherCollider => !ShouldIgnoreCollider(otherCollider, leftController, rightController))
                .ToArray();

            _renderer.enabled = _matchedNeighbours.Length > 0;

            if (_matchedNeighbours.Length == 0)
            {
                Fading = false;

                SetFade(0, default);
                return;
            }

            if (!Fading)
            {
                _headEnterPoint = transform.position;
                Fading = true;
            }

            var distance = Vector3.Distance(transform.position, _headEnterPoint);
            var outDirection = _headEnterPoint - transform.position;
            var fadePower = _fadeCurve.Evaluate(Mathf.Clamp01(distance / MaxIntersectionDistance));

            SetFade(fadePower, outDirection.normalized);
        }

        private bool ShouldIgnoreCollider(
            Collider otherCollider,
            ControllerInteraction.ControllerSelf leftController,
            ControllerInteraction.ControllerSelf rightController
        )
        {
            if (_selfIgnoreColliders.Contains(otherCollider) || HeadCollisionFadeHelper.IsIgnoredCollider(otherCollider))
            {
                return true;
            }

            var grabbedWithRightHand = leftController != null && leftController.CheckIfColliderPresent(otherCollider);
            var grabbedWithLeftHand = rightController != null && rightController.CheckIfColliderPresent(otherCollider);

            return grabbedWithRightHand || grabbedWithLeftHand;
        }


        private void FixedUpdate()
        {
            _interactionsCount = Physics.OverlapSphereNonAlloc(transform.position, _collider.radius, _allNeighbours, ~_ignoreMask,
                QueryTriggerInteraction.Ignore);
        }

        private void SetFade(float force, Vector3 worldDirection)
        {
            _propertyBlock ??= new();

            _propertyBlock.SetFloat(ForceParameterId, force);
            _propertyBlock.SetVector(WorldDirectionParameterId, worldDirection);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
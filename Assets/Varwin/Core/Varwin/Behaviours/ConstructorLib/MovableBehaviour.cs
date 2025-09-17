using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    [Obsolete]
    public class MovableBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            return true;
        }
    }
    
    [Obsolete]
    [VarwinComponent(English: "Movement", Russian: "Передвижение", Chinese: "移動")]
    public class MovableBehaviour : VarwinBehaviour
    {
        public enum AxisDirection
        {
            [Item(English: "+X", Chinese: "+X")]
            PosX = 0,
            [Item(English: "+Y", Chinese: "+Y")]
            PosY,
            [Item(English: "+Z", Chinese: "+Z")]
            PosZ,
            [Item(English: "-X", Chinese: "-X")]
            NegX,
            [Item(English: "-Y", Chinese: "-Y")]
            NegY,
            [Item(English: "-Z", Chinese: "-Z")]
            NegZ
        }

        public enum MovementType
        {
            [Item(English: "Once", Russian: "Один раз", Chinese: "一次")]
            Once = 0,
            [Item(English: "Repeatedly", Russian: "Повторяясь", Chinese: "反复")]
            Repeat,
            [Item(English: "Back and forth", Russian: "Туда-сюда", Chinese: "來回")]
            PingPong
        }
        
        public delegate void MovementCompleteHandler(bool finished);
        public delegate void TargetReachedHandler(Wrapper target);
        public delegate void PathPointReachedHandler(int id, Wrapper point);
        public delegate void CollisionHandler(Wrapper target);
        
        private Rigidbody _body;
        private Rigidbody Body
        {
            get
            {
                if (!_body)
                {
                    _body = GetComponent<Rigidbody>();
                }

                return _body;
            }
        }
        
        private Collider[] _colliders;
        private Collider[] Colliders => _colliders ?? (_colliders = GetComponentsInChildren<Collider>());

        // movement
        private bool _forceStopMoving;
        private bool _pauseMovement;
        
        private float _targetObjectMinimalDistance = 0.1f;
        private bool _alwaysLookAtTargetWhileMoving = false;
        
        private Coroutine _moveTowardsCoroutine;
        private Coroutine _moveAlongPathCoroutine;

        private Coroutine _moveOnXCoroutine;
        private Coroutine _moveOnYCoroutine;
        private Coroutine _moveOnZCoroutine;

        private bool _moveOnXRunning;
        private bool _moveOnYRunning;
        private bool _moveOnZRunning;
        
        // rotation
        private bool _forceStopRotating;
        private bool _pauseRotating;
        
        private float _targetLookAtMinimalAngle = 2.0f;
        
        private Coroutine _lookAtCoroutine;
        
        private Coroutine _rotateOnXCoroutine;
        private Coroutine _rotateOnYCoroutine;
        private Coroutine _rotateOnZCoroutine;

        private bool _rotateOnXRunning;
        private bool _rotateOnYRunning;
        private bool _rotateOnZRunning;
        
        protected override void AwakeOverride()
        {
            
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }

        [Obsolete]
        [VarwinInspector(English:"Rotate player with object", Russian: "Поворачивать игрока вместе с объектом", Chinese: "使用對象旋轉播放器")]
        public bool RotatePlayerWithObject
        {
            get => _rotatePlayerWithObject;
            set => _rotatePlayerWithObject = value;
        }

        private bool _rotatePlayerWithObject;

        #region COLLISIONS

        public void OnGrabbedCollisionEnter(GameObject other) => OnCollisionEnter(other);
        
        public void OnGrabbedCollisionExit(GameObject other) => OnCollisionExit(other);

        private void OnCollisionEnter(Collision other) => OnCollisionEnter(other.gameObject);

        private void OnCollisionEnter(GameObject other)
        {
            if (ProjectData.GameMode == GameMode.Edit)
            {
                return;
            }
            
            var potentialObject = other.GetComponentInParent<VarwinObjectDescriptor>();

            if (potentialObject && potentialObject.Wrapper() != null)
            {
                OnCollisionStart?.Invoke(potentialObject.Wrapper());
            }
        }

        private void OnCollisionExit(Collision other) => OnCollisionExit(other.gameObject); 
        
        private void OnCollisionExit(GameObject other)
        {
            if (ProjectData.GameMode == GameMode.Edit)
            {
                return;
            }
            
            var potentialObject = other.GetComponentInParent<VarwinObjectDescriptor>();

            if (potentialObject && potentialObject.Wrapper() != null)
            {
                OnCollisionEnd?.Invoke(potentialObject.Wrapper());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (ProjectData.GameMode == GameMode.Edit)
            {
                return;
            }
            
            VarwinObjectDescriptor potentialObject = other.gameObject.GetComponentInParent<VarwinObjectDescriptor>();

            if (potentialObject && potentialObject.Wrapper() != null)
            {
                OnTriggerStart?.Invoke(potentialObject.Wrapper());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (ProjectData.GameMode == GameMode.Edit)
            {
                return;
            }
            
            VarwinObjectDescriptor potentialObject = other.gameObject.GetComponentInParent<VarwinObjectDescriptor>();

            if (potentialObject && potentialObject.Wrapper() != null)
            {
                OnTriggerEnd?.Invoke(potentialObject.Wrapper());
            }
        }

        [Obsolete]
        [EventGroup("CollisionEvents")]
        [Event(English:"on collision start", Russian: "началось столкновение", Chinese: "碰撞開始時")]
        public event CollisionHandler OnCollisionStart;

        [Obsolete]
        [EventGroup("CollisionEvents")]
        [Event(English:"on collision end", Russian: "столкновение закончилось", Chinese: "碰撞結束")]
        public event CollisionHandler OnCollisionEnd;
        
        [Obsolete]
        [EventGroup("TriggerEvents")]
        [Event(English:"object got inside of target object", Russian: "объект попал внутрь целевого", Chinese: "物件進入目標物件")]
        public event CollisionHandler OnTriggerStart;

        [Obsolete]
        [EventGroup("TriggerEvents")]
        [Event(English:"object got outside target object", Russian: "объект оказался снаружи целевого", Chinese: "對象超出了目標對象")]
        public event CollisionHandler OnTriggerEnd;
        
        #endregion

        #region MOVE BLOCKS

        [Obsolete]
        [Action(English: "Move in position", Russian: "Переместиться в позицию", Chinese: "移動到位")]
        [ArgsFormat(English:"x: {%}, y: {%}, z: {%}", Russian:" x:{%}, y: {%}, z: {%}", Chinese: "x: {%}, y: {%}, z: {%}")]
        public void TeleportToCoordinates(float x, float y, float z)
        {
            TeleportToPos(new Vector3(x, y, z));
        }
        
        [Obsolete]
        [Action(English: "Move in center of", Russian: "Переместиться в центр", Chinese: "移動到中心")]
        public void TeleportInCenterOfObject(Wrapper obj)
        {
            var destinationObj = obj.GetGameObject();

            if (!destinationObj)
            {
                return;
            }

            var destinationTransform = destinationObj.transform;
            TeleportToPos(destinationTransform.position);

            if (!_alwaysLookAtTargetWhileMoving)
            {
                transform.rotation = destinationTransform.rotation;
            }
        }

        private void TeleportToPos(Vector3 pos)
        {
            var delta = pos - transform.position;
            Teleport(delta);
        }

        private void Teleport(Vector3 delta)
        {
            var teleportList = new List<GameObject>();
            var oc = gameObject.GetWrapper().GetObjectController();

            if (oc.LockChildren)
            {
                ObjectController highestLocked = oc;

                while (highestLocked.Parent && highestLocked.Parent.LockChildren)
                {
                    highestLocked = highestLocked.Parent;
                }
                FillLockedList(highestLocked, ref teleportList);
            }
            else
            {
                teleportList.Add(gameObject);
            }
            
            foreach (var obj in teleportList)
            {
                obj.transform.Translate(delta, Space.World);
            }
        }

        private void FillLockedList(ObjectController highestLocked, ref List<GameObject> objects)
        {
            objects.Add(highestLocked.gameObject);
            
            foreach (var oc in highestLocked.Children)
            {
                FillLockedList(oc, ref objects);
            }
        }
        
        [Obsolete]
        [Action(English: "Set target stop distance", Russian: "Задать расстояние остановки перед целевым объектом", Chinese: "設置目標停止距離")]
        public void SetMinObjectDistance(float objectDistance)
        {
            _targetObjectMinimalDistance = objectDistance;
        }
        
        [Obsolete]
        [Action(English: "Move", Russian: "Перемещаться", Chinese: "移動")]
        [ArgsFormat(English:"in direction {%} at a speed of {%}m/s", Russian: "в направлении {%} со скоростью {%}м/сек", Chinese: "以 {%}m/s 的速度沿 {%} 方向")]
        public void MoveWithSpeed(AxisDirection axis, float speed)
        {
            MoveWithDuration(axis, -1, speed, MovementType.Once);
        }

        [Obsolete]
        [Action(English: "Move", Russian: "Перемещаться", Chinese: "移動")]
        [ArgsFormat(English:"in direction {%} to a distance of {%}m at a speed of {%}m/s {%}", Russian: "в направлении {%} на расстояние {%}м со скоростью {%}м/сек {%}", Chinese: "以 {%}m/s {%} 的速度沿 {%} 方向到 {%}m 的距離")]
        public void MoveWithDistance(AxisDirection axis, float distance, float speed, MovementType type)
        {
            MoveWithDuration(axis, distance / speed, speed, type);
        }

        [Obsolete]
        [Action(English: "Move", Russian: "Перемещаться", Chinese: "移動")]
        [ArgsFormat(English:"in direction {%} for {%}s at a speed of {%}m/s {%}", Russian: "в направлении {%} в течение {%}сек со скоростью {%}м/сек {%}", Chinese: "以 {%}m/s {%} 的速度在 {%} 方向 {%}s")]
        public void MoveWithDuration(AxisDirection axis, float duration, float speed, MovementType type)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            switch (axis)
            {
                case AxisDirection.NegX:
                case AxisDirection.PosX:
                    StartMoveInDirectionCoroutine(ref _moveOnXCoroutine, ref _moveOnXRunning, axis, speed, duration, type);
                    break;
                
                case AxisDirection.NegY:
                case AxisDirection.PosY:
                    StartMoveInDirectionCoroutine(ref _moveOnYCoroutine, ref _moveOnYRunning, axis, speed, duration, type);
                    break;
                
                case AxisDirection.NegZ:
                case AxisDirection.PosZ:
                    StartMoveInDirectionCoroutine(ref _moveOnZCoroutine, ref _moveOnZRunning, axis, speed, duration, type);
                    break;
            }
        }

        private void StartMoveInDirectionCoroutine(ref Coroutine coroutine, ref bool moveRunning, AxisDirection axis, float speed, float duration, MovementType type)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            moveRunning = true;

            coroutine = StartCoroutine(MoveInDirectionCoroutine(axis, speed, duration, type));
        }
       
        [Obsolete]
        [Action(English: "Move", Russian: "Перемещаться", Chinese: "移動")]
        [ArgsFormat(English: "towards the object {%} at a speed of {%}m/s without stopping {%}", Russian: "в сторону объекта {%} со скоростью {%}м/сек не прекращая {%}", Chinese: "以 {%}m/s 的速度向物體 {%} 前進，不停止 {%}")]
        public void MoveTowards(Wrapper wrapper, float speed, bool infinite)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            if(_moveTowardsCoroutine != null)
            {
                StopCoroutine(_moveTowardsCoroutine);
            }
            
            _moveTowardsCoroutine = StartCoroutine(MoveTowardsCoroutine(wrapper, speed, infinite));
        }

        [Obsolete]
        [Action(English: "Move", Russian: "Перемещаться", Chinese: "移動")]
        [ArgsFormat(English: "along the path {%} at a speed of {%}m/s {%}", Russian: "по маршруту {%} со скоростью {%}м/сек {%}", Chinese: "以 {%}m/s {%} 的速度沿著路徑 {%}")]
        public void MoveByPath(dynamic points, float speed, MovementType type)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
#if !NET_STANDARD_2_0
            List<dynamic> pointsList;
            
            if (points is List<dynamic>)
            {
                pointsList = points;
            }
            else if (points is DynamicWrapperCollection)
            {
                pointsList = new List<dynamic>(points);
            }
            else if(points is Wrapper)
            {
                pointsList = new List<dynamic> {points};
            }
            else
            {
                return;
            }
            
            List<Wrapper> wrappersList = new List<Wrapper>();

            foreach (dynamic point in pointsList)
            {
                Wrapper wrapper = point as Wrapper;

                if (wrapper != null)
                {
                    wrappersList.Add(wrapper);
                }
            }

            if (wrappersList.Count == 0)
            {
                return;
            }
            
            if(_moveAlongPathCoroutine != null)
            {
                StopCoroutine(_moveAlongPathCoroutine);
            }
            
            _moveAlongPathCoroutine = StartCoroutine(MoveAlongPath(wrappersList, speed, type));
#endif
        }
        
        [Obsolete]
        [ActionGroup("PauseContinueStopMove")]
        [Action(English: "Pause movement", Russian: "Приостановить перемещение", Chinese: "暫停運動")]
        public void PauseMovement()
        {
            _pauseMovement = true;
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopMove")]
        [Action(English: "Continue movement", Russian: "Возобновить перемещение", Chinese: "繼續運動")]
        public void ContinueMovement()
        {
            _pauseMovement = false;
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopMove")]
        [Action(English: "Stop movement", Russian: "Остановить перемещение", Chinese: "停止運動")]
        public void StopMovements()
        {
            _forceStopMoving = true;
        }

        [Obsolete]
        [Event(English: "movement complete", Russian: "движение завершено", Chinese: "運動完成")]
        public event MovementCompleteHandler MovementComplete;
        
        [Obsolete]
        [Event(English: "target reached", Russian: "целевой объект достигнут", Chinese: "達到目標")]
        public event TargetReachedHandler TargetReached;

        [Obsolete]
        [Event(English: "path point reached", Russian: "Точка маршрута достигнута", Chinese: "抵達路徑點")]
        public event PathPointReachedHandler PathPointReached;
        
        #endregion

        #region ROTATE BLOCKS

        [Obsolete]
        [ActionGroup("RotationWithObjects")]
        [Action(English: "Rotate in direction of", Russian: "Повернуться в сторону", Chinese: "旋轉方向")]
        public void RotateInDirection(Wrapper obj)
        {
            var targetGameObject = obj.GetGameObject();

            if (targetGameObject)
            {
                Body.transform.LookAt(targetGameObject.transform);
            }
        }
        
        [Obsolete]
        [Variable(English: "Always look at target object while moving", Russian: "Всегда поворачиваться к целевому объекту при движении", Chinese: "移動時始終注視目標物體")]
        public bool LookAtWhenMovingToTarget
        {
            set => _alwaysLookAtTargetWhileMoving = value;
        }
        
        [Obsolete]
        [ActionGroup("RotationWithObjects")]
        [Action(English: "Rotate as", Russian: "Повернуться так же, как", Chinese: "旋轉為")]
        public void RotateAsObject(Wrapper obj)
        {
            var targetGameObject = obj.GetGameObject();

            if (targetGameObject)
            {
                Body.transform.rotation = targetGameObject.transform.rotation;
            }
        }

        [Obsolete]
        [Action(English: "Turn around ", Russian: "Повернуть вокруг ")]
        [ArgsFormat(English:"x axis on angle {%}, y axis on angle {%}, z axis on angle {%}", Russian:"по оси X на угол {%}, по оси Y на угол {%}, по оси Z на угол{%}", Chinese: "x軸在角度{%}上，y軸在角度{%}上，z軸在角度{%}上")]
        public void RotateOnAxis(float xAngle, float yAngle, float zAngle)
        { 
            Quaternion rotation = Quaternion.Euler(xAngle, yAngle, zAngle);
            Body.transform.rotation = rotation;
        }
        
        [Obsolete]
        [Action(English: "Set min target object look at angle", Russian: "Задать минимальный угол поворота к целевому объекту", Chinese: "設置最小目標對像看角度")]
        public void SetMinObjectLookAtAngle(float angle)
        {
            _targetLookAtMinimalAngle = angle;
        }

        [Obsolete]
        [Action(English:"Rotate", Russian: "Вращаться", Chinese: "旋轉")]
        [ArgsFormat(English: "around {%} axis at a speed of {%} degree/s", Russian: "вокруг оси {%} со скоростью {%}градусов/сек", Chinese: "以 {%} 度/秒的速度繞 {%} 軸")]
        public void RotateWithSpeed(AxisDirection axis, float speed)
        {
            RotateWithDuration(axis, -1, speed, MovementType.Once);
        }

        [Obsolete]
        [Action(English:"Rotate", Russian: "Вращаться", Chinese: "旋轉")]
        [ArgsFormat(English: "around {%} axis for {%} sec at a speed of {%} degree/s {%}", Russian: "вокруг оси {%} в течение {%} с со скоростью {%}градусов/сек {%}", Chinese: "以 {%} 度/s {%} 的速度繞 {%} 軸持續 {%} 秒")]
        public void RotateWithDuration(AxisDirection axis, float duration, float speed, MovementType type)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            switch (axis)
            {
                case AxisDirection.NegX:
                case AxisDirection.PosX:
                    StartRotateAroundCoroutine(ref _rotateOnXCoroutine, ref _rotateOnXRunning, axis, speed, duration, type);
                    break;
                
                case AxisDirection.NegY:
                case AxisDirection.PosY:
                    StartRotateAroundCoroutine(ref _rotateOnYCoroutine, ref _rotateOnYRunning, axis, speed, duration, type);
                    break;
                
                case AxisDirection.NegZ:
                case AxisDirection.PosZ:
                    StartRotateAroundCoroutine(ref _rotateOnZCoroutine, ref _rotateOnZRunning, axis, speed, duration, type);
                    break;
            }
        }

        private void StartRotateAroundCoroutine(ref Coroutine coroutine, ref bool rotateRunning,  AxisDirection axis, float speed, float duration, MovementType type)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }

            rotateRunning = true;

            coroutine = StartCoroutine(RotateAroundCoroutine(axis, speed, duration, type));
        }

        [Obsolete]
        [Action(English:"Rotate", Russian: "Вращаться", Chinese: "旋轉")]
        [ArgsFormat(English: "around {%} axis for {%} degrees at a speed of {%} degree/s {%}", Russian: "вокруг оси {%} на {%} градусов со скоростью {%}градусов/сек {%}", Chinese: "以 {%} 度/秒 {%} 的速度繞 {%} 軸 {%} 度")]
        public void RotateWithAngle(AxisDirection axis, float angle, float speed, MovementType type)
        {
            RotateWithDuration(axis, angle/speed, speed, type);
        }

        [Obsolete]
        [Action(English:"Look at", Russian: "Повернуться", Chinese: "看著")]
        [ArgsFormat(English:"the object {%} at a speed of {%} degree/s without stopping {%}", Russian: "к объекту {%} со скоростью {%}градусов/сек не прекращая {%}", Chinese: "物體 {%} 以 {%} 度/s 的速度不停{%}")]
        public void LookAt(Wrapper target, float speed, bool infinite)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            
            if(_lookAtCoroutine != null)
            {
                StopCoroutine(_lookAtCoroutine);
            }
            
            _lookAtCoroutine = StartCoroutine(LookAtCoroutine(target, speed, infinite));
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopRotation")]
        [Action(English: "Pause Rotation", Russian: "Приостановить вращение", Chinese: "暫停旋轉")]
        public void PauseRotation()
        {
            _pauseRotating = true;
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopRotation")]
        [Action(English: "Continue Rotation", Russian: "Возобновить вращение", Chinese: "繼續旋轉")]
        public void ContinueRotation()
        {
            _pauseRotating = false;
        }

        [Obsolete]
        [ActionGroup("PauseContinueStopRotation")]
        [Action(English: "Stop Rotation", Russian: "Остановить вращение", Chinese: "停止旋轉")]
        public void StopRotation()
        {
            _forceStopRotating = true;
        }
        
        [Obsolete]
        [Event(English: "rotation complete", Russian: "вращение завершено", Chinese: "旋轉完成")]
        public event MovementCompleteHandler RotationComplete;        
        
        [Obsolete]
        [Event(English: "look at finished", Russian: "поворот к объекту завершен", Chinese: "看成品")]
        public event TargetReachedHandler LookAtFinished;

        #endregion
        
        #region MOVE COROUTINES
        
        IEnumerator MoveInDirectionCoroutine(AxisDirection axis, float speed, float duration, MovementType type)
        {
            _forceStopMoving = false;
            Vector3 localDirection = VectorFromDirection(axis);
            Vector3 beginDirection = transform.position;
            
            float timer = 0.0f;

            bool movementComplete = false;
            bool movePlayer = IsTeleportable();
            
            while (!movementComplete)
            {
                while (_pauseMovement)
                {
                    if (_forceStopMoving)
                    {
                        SetMovingStateOnAxis(axis, false);
                        MovementComplete?.Invoke(false);
                        break;
                    }
                    yield return null;
                }
                
                if (_forceStopMoving)
                {
                    SetMovingStateOnAxis(axis, false);
                    MovementComplete?.Invoke(false);
                    break;
                }
                
                Vector3 deltaToMove = transform.rotation * localDirection * (speed * Time.deltaTime);

                Teleport(deltaToMove);
                timer += Time.deltaTime;

                movementComplete = timer >= duration && duration > 0.0f;
      
                if (movementComplete)
                {
                    switch (type)
                    {
                        case MovementType.Once:
                            SetMovingStateOnAxis(axis, false);
                            break;
                        
                        case MovementType.Repeat: 
                            Body.position = beginDirection;
                            timer = 0.0f;
                            movementComplete = false;
                            MovementComplete?.Invoke(false);
                            break;
                        
                        case MovementType.PingPong: 
                            timer = 0.0f;
                            localDirection = -localDirection;
                            movementComplete = false;
                            MovementComplete?.Invoke(false);
                            break;
                    }
                }
          
                if (movePlayer)
                {
                    if (IsPlayerOverObject())
                    {
                        MovePlayer(deltaToMove);
                    }
                }
                
                yield return null;
            }

            MovementComplete?.Invoke(!_moveOnXRunning && !_moveOnYRunning && !_moveOnZRunning);
        }

        private void SetMovingStateOnAxis(AxisDirection axis, bool value)
        {
            switch (axis)
            {
                case AxisDirection.NegX:
                case AxisDirection.PosX:
                    _moveOnXRunning = value;
                    break;
                
                case AxisDirection.NegY:
                case AxisDirection.PosY:
                    _moveOnYRunning = value;
                    break;
                
                case AxisDirection.NegZ:
                case AxisDirection.PosZ:
                    _moveOnZRunning = value;
                    break;
            }
        }

        IEnumerator MoveTowardsCoroutine(Wrapper wrapper, float speed, bool infinite)
        {
            _forceStopMoving = false;
            var targetReached = false;
            var movePlayer = IsTeleportable();

            var target = wrapper.GetGameObject().transform;
            
            while (infinite || !targetReached)
            {
                if (_forceStopMoving)
                {
                    break;
                }

                while (_pauseMovement)
                {
                    if (_forceStopMoving)
                    {
                        break;
                    }
                    yield return null;
                }
                
                var direction = (target.position - transform.position).normalized;
                var deltaToMove = targetReached ? Vector3.zero : direction * (speed * Time.deltaTime);

                if (movePlayer)
                {
                    if (IsPlayerOverObject())
                    {
                        MovePlayer(deltaToMove);
                    }
                }
                
                Teleport(deltaToMove);
   
                if (_alwaysLookAtTargetWhileMoving)
                {
                    Body.transform.LookAt(target);
                }

                targetReached = Vector3.Distance(transform.position, target.position) <= _targetObjectMinimalDistance;

                yield return null;
            }
            
            TargetReached?.Invoke(wrapper);
        }

        IEnumerator MoveAlongPath(List<Wrapper> path, float speed, MovementType type)
        {
            var currentPoint = 0;

            var movementComplete = false;
            var movePlayer = IsTeleportable();

            while (!movementComplete)
            {
                if (_forceStopMoving)
                {
                    break;
                }

                while (_pauseMovement)
                {
                    if (_forceStopMoving)
                    {
                        break;
                    }
                    yield return null;
                }

                var currentTarget = path[currentPoint].GetGameObject().transform.position;
                
                if (Vector3.Distance(transform.position, currentTarget) <= _targetObjectMinimalDistance)
                {
                    PathPointReached?.Invoke(currentPoint + 1, path[currentPoint]);
                    
                    currentPoint++;

                    if (currentPoint >= path.Count)
                    {
                        movementComplete = true;
                    }
                }

                if (movementComplete)
                {
                    switch (type)
                    {
                        case MovementType.Once:
                            break;
                        
                        case MovementType.Repeat: 
                            currentPoint = 0;
                            movementComplete = false;
                            MovementComplete?.Invoke(false);
                            break;
                        
                        case MovementType.PingPong: 
                            currentPoint = 0;
                            path.Reverse();
                            movementComplete = false;
                            MovementComplete?.Invoke(false);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    }
                }
                else
                {
                    var direction = (currentTarget - transform.position).normalized;
                    var deltaToMove = direction * (speed * Time.deltaTime);
                
                    if (movePlayer)
                    {
                        if (IsPlayerOverObject())
                        {
                            MovePlayer(deltaToMove);
                        }
                    }
                
                    Teleport(deltaToMove);
                
                    if (_alwaysLookAtTargetWhileMoving)
                    {
                        Body.transform.LookAt(currentTarget);
                    }
                }
                
                yield return null;
            }
            
            MovementComplete?.Invoke(true);
        }

        #endregion

        #region ROTATE COROUTINES

        IEnumerator RotateAroundCoroutine(AxisDirection axis, float speed, float duration, MovementType type)
        {
            _forceStopRotating = false;

            bool rotationComplete = false;
            float timer = 0.0f;
            bool movePlayer = IsTeleportable() && _rotatePlayerWithObject;

            Quaternion beginRotation = transform.rotation;
            Vector3 localAxis = VectorFromDirection(axis);

            while (!rotationComplete)
            {
                while (_pauseRotating)
                {
                    if (_forceStopRotating)
                    {
                        SetRotatingStateOnAxis(axis, false);
                        RotationComplete?.Invoke(false);
                        break;
                    }
                    yield return null;
                }
                
                if (_forceStopRotating)
                {
                    SetRotatingStateOnAxis(axis, false);
                    RotationComplete?.Invoke(false);
                    break;
                }

                Quaternion deltaRotation = Quaternion.AngleAxis(speed * Time.deltaTime, localAxis);

                if (movePlayer)
                {
                    if (IsPlayerOverObject())
                    {
                        yield return new WaitForEndOfFrame();
                        PlayerManager.CurrentRig.SetRotation(PlayerManager.CurrentRig.Rotation * deltaRotation);
                    }
                }
                
                Body.rotation *= deltaRotation;
                Body.transform.rotation = Body.rotation;
                
                timer += Time.deltaTime;
                
                rotationComplete = timer >= duration && duration > 0.0f;

                if (rotationComplete)
                {
                    switch (type)
                    {
                        case MovementType.Once:
                            SetRotatingStateOnAxis(axis, false);
                            break;
                        
                        case MovementType.Repeat: 
                            Body.rotation = beginRotation;
                            timer = 0.0f;
                            rotationComplete = false;
                            RotationComplete?.Invoke(false);
                            break;
                        
                        case MovementType.PingPong: 
                            timer = 0.0f;
                            localAxis = -localAxis;
                            rotationComplete = false;
                            RotationComplete?.Invoke(false);
                            break;
                    }
                }

                yield return null;
            }
            
            RotationComplete?.Invoke(!_rotateOnXRunning && !_rotateOnYRunning && !_rotateOnZRunning);
        }

        private void SetRotatingStateOnAxis(AxisDirection axis, bool value)
        {
            switch (axis)
            {
                case AxisDirection.NegX:
                case AxisDirection.PosX:
                    _rotateOnXRunning = value;
                    break;
                
                case AxisDirection.NegY:
                case AxisDirection.PosY:
                    _rotateOnYRunning = value;
                    break;
                
                case AxisDirection.NegZ:
                case AxisDirection.PosZ:
                    _rotateOnZRunning = value;
                    break;
            }
        }
        
        IEnumerator LookAtCoroutine(Wrapper wrapper, float speed, bool infinite)
        {
            _forceStopRotating = false;
            bool movePlayer = IsTeleportable() && _rotatePlayerWithObject;
            bool targetReached = false;
            
            Transform target = wrapper.GetGameObject().transform;

            while (!targetReached)
            {
                while (_pauseRotating)
                {
                    if (_forceStopRotating)
                    {
                        break;
                    }

                    yield return null;
                }

                if (_forceStopRotating)
                {
                    break;
                }
                
                Quaternion lookOnLook = Quaternion.LookRotation(target.position - transform.position);
                Quaternion deltaRotation = Quaternion.Slerp(transform.rotation, lookOnLook, speed * Time.deltaTime);
                
                if (movePlayer)
                {
                    if (IsPlayerOverObject())
                    {
                        yield return new WaitForEndOfFrame();
                        PlayerManager.CurrentRig.SetRotation(PlayerManager.CurrentRig.Rotation * deltaRotation);
                    }
                }
                
                Body.rotation = deltaRotation;
                Body.transform.rotation = deltaRotation;

                if (!infinite)
                {
                    targetReached = Quaternion.Angle(Body.rotation, lookOnLook) <= _targetLookAtMinimalAngle;
                }

                yield return null;
            }
            
            LookAtFinished?.Invoke(wrapper);
        }

        #endregion
        
        public Vector3 VectorFromDirection(AxisDirection direction)
        {
            switch (direction)
            {
                case AxisDirection.PosX:
                    return new Vector3(1.0f, 0.0f, 0.0f);

                case AxisDirection.PosY:
                    return new Vector3(0.0f, 1.0f, 0.0f);
                
                case AxisDirection.PosZ:
                    return new Vector3(0.0f, 0.0f, 1.0f);
                
                case AxisDirection.NegX:
                    return new Vector3(-1.0f, 0.0f, 0.0f);

                case AxisDirection.NegY:
                    return new Vector3(0.0f, -1.0f, 0.0f);
                
                case AxisDirection.NegZ:
                    return new Vector3(0.0f, 0.0f, -1.0f);
            }

            return Vector3.zero;
        }
        
        private bool IsTeleportable()
        {
            InteractableBehaviour interactableBehaviour = GetComponent<InteractableBehaviour>();

            if (interactableBehaviour)
            {
                return interactableBehaviour.Teleportable && interactableBehaviour.Collision;
            }

            return false;
        }

        private bool IsPlayerOverObject()
        {
            Vector3 playerPosition = PlayerManager.CurrentRig.Position;

            if (playerPosition.y < transform.position.y)
            {
                return false;
            }
            
            Ray playerRay = new Ray(playerPosition + Vector3.up, Vector3.down);

            foreach (Collider objectCollider in Colliders)
            {
                if (objectCollider.bounds.IntersectRay(playerRay))
                {
                    return true;
                }
            }
            

            return false;
        }

        private void MovePlayer(Vector3 deltaToMove)
        {
            var playerMove = deltaToMove;

            if (PlayerManager.UseGravity)
            {
                playerMove.y = 0;
            }
                        
            PlayerManager.CurrentRig.Position += playerMove;
        }
    }
}
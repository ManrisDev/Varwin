using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using static Varwin.TypeValidationUtils;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class PhysicsBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool IsDisabledBehaviour(GameObject targetGameObject)
        {
            return IsDisabledBehaviour(targetGameObject, BehaviourType.Physics);
        }
    }
    
    [RequireComponentInChildren(typeof(Rigidbody))]
    [RequireComponentInChildren(typeof(Collider))]
    [VarwinComponent(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
    public class PhysicsBehaviour : ConstructorVarwinBehaviour, ISwitchModeSubscriber
    {
        #region Enums

        public enum Relativeness
        {
            [Item(English:"object",Russian:"объекта",Chinese:"物件",Kazakh:"объект",Korean:"객체")]
            Self,

            [Item(English:"world",Russian:"мира",Chinese:"世界",Kazakh:"әлем",Korean:"세계")]
            World
        }

        public enum Gravity
        {
            [Item(English:"affected by gravity",Russian:"подчиняется гравитации",Chinese:"受重力影響",Kazakh:"гравитацияға бағынады",Korean:"중력의 영향 받음")]
            GravityOn,

            [Item(English:"does not affected by gravity",Russian:"не подчиняется гравитации",Chinese:"不受重力影響",Kazakh:"гравитацияға бағынбайды",Korean:"중력의 영향 받지 않음")]
            GravityOff
        }

        public enum Kinematic
        {
            [Item(English:"object is static",Russian:"объект статичен",Chinese:"物件為靜態",Kazakh:"объект статикалық",Korean:"객체는 정지 상태")]
            Kinematic,

            [Item(English:"object is not static",Russian:"объект не статичен",Chinese:"物件非靜態",Kazakh:"объект статикалық емес",Korean:"객체는 정지 상태가 아님")]
            NonKinematic
        }

        public enum Obstacle
        {
            [Item(English:"object is obstacle",Russian:"объект является препятствием",Chinese:"物件不可穿越",Kazakh:"объект кедергі болып табылады",Korean:"객체는 장애물")]
            Obstacle,

            [Item(English:"object is not obstacle",Russian:"объект не является препятствием",Chinese:"物件非不可穿越",Kazakh:"объект кедергі емес",Korean:"객체가 장애물이 아님")]
            NonObstacle,
        }

        #endregion

        public delegate void CommonForceHandler();

        public delegate void TriggerHandler([Parameter(English:"target object",Russian:"целевой объект",Chinese:"目標物件",Kazakh:"мақсатты объект",Korean:"목표 객체")] Wrapper target);

        private readonly List<int> _currentForceRoutines = new();
        private int _routineId;

        private readonly BehaviourState _currentState = new();

        private readonly List<Collider> _nonTriggerColliders = new();

        private Rigidbody _thisRigidbody;
        private Collider _thisCollider;

        private Vector3 _lastFrameVelocity;
        private bool _isKinematic;
        private bool _isKinematicInitialized;
        private bool _useGravity;
        private bool _isGravityInitialized;

        #region VarwinInspector

        public Rigidbody Rigidbody
        {
            get
            {
                if (!_thisRigidbody)
                {
                    _thisRigidbody = GetComponentInChildren<Rigidbody>();
                }

                return _thisRigidbody;
            }
        }

        public Collider Collider
        {
            get
            {
                if (_thisCollider)
                {
                    return _thisCollider;
                }
                
                InitColliders();
                return _thisCollider;
            }
        }

        [VarwinInspector(English:"Mass",Russian:"Масса",Chinese:"質量",Kazakh:"Масса",Korean:"질량")]
        public float MassInspector
        {
            set => Rigidbody.mass = value;
            get => Rigidbody.mass;
        }
        
        [VarwinInspector(English:"Bounciness",Russian:"Пружинистость",Chinese:"彈性",Kazakh:"Серіппелілік",Korean:"탄력")]
        public float BouncinessInspector
        {
            set => Collider.material.bounciness = Mathf.Clamp01(value);
            get => Collider.material.bounciness;
        }
        
        [VarwinInspector(English:"Linear drag",Russian:"Линейное сопротивление",Chinese:"线性阻力",Kazakh:"Сызықтық кедергі",Korean:"선형 드래그")]
        public float LinearDrag
        {
            set => Rigidbody.drag = value;
            get => Rigidbody.drag;
        }
        
        [VarwinInspector(English:"Angular drag",Russian:"Угловое сопротивление",Chinese:"旋转阻力",Kazakh:"Бұрыштық кедергі",Korean:"회전 드래그")]
        public float AngularDrag
        {
            set => Rigidbody.angularDrag = value;
            get => Rigidbody.angularDrag;
        }
        
        [VarwinInspector(English:"Use gravity",Russian:"Гравитация",Chinese:"受重力影響",Kazakh:"Гравитация",Korean:"중력 적용")]
        public bool GravityInspector
        {
            get
            {
                if (!_isGravityInitialized)
                {
                    _useGravity = Rigidbody.useGravity;
                    _isGravityInitialized = true;
                }
                return _useGravity;
            }
            set
            {
                if (_useGravity == value)
                {
                    return;
                }
                
                _useGravity = value;
                TrySetPhysicsSettings();
            }
        }
        
        [VarwinInspector(English:"Static",Russian:"Статичный",Chinese:"靜態物件",Kazakh:"Статикалық",Korean:"고정")]
        public bool KinematicInspector
        {
            get
            {
                if (!_isKinematicInitialized)
                {
                    _isKinematic = Rigidbody.isKinematic;
                    _isKinematicInitialized = true;
                }

                return _isKinematic;
            }
            set
            {
                if (_isKinematic == value)
                {
                    return;
                }
                
                _isKinematic = value;
                TrySetPhysicsSettings();
            }
        }

        [VarwinInspector(English:"Is obstacle",Russian:"Препятствие",Chinese:"不可穿越",Kazakh:"Кедергі",Korean:"장애물")]
        public bool ObstacleInspector
        {
            set => ObstacleSetter = value ? Obstacle.Obstacle : Obstacle.NonObstacle;
            get
            {
                if (_nonTriggerColliders == null)
                {
                    InitColliders();
                }
                
                return _nonTriggerColliders.Any(col => !col.isTrigger && !(col is MeshCollider {convex: false}));
            }
        }

        #endregion

        #region Actions

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Instantly applies force to the object in the direction of the specified vector in the selected coordinate system. The value is measured in kg*m/s.",Russian:"Мгновенно прикладывает силу к объекту в направлении заданного вектора в выбранной системе координат. Величина измеряется в кг*м/с.",Chinese:"瞬間以一個方向對一個物件施力。該值以 kg*m/s 為單位測量。",Kazakh:"Таңдалған координаттар жүйесінде берілген вектор бағытында объектіге лезде күш салады. Шама кг*м/с-пен өлшенеді.",Korean:"선택한 좌표계에서 지정된 벡터 방향으로 물체에 즉시 힘을 가합니다. 값은 kg*m/s 단위로 측정됩니다.")]
        [Action(English:"apply force of",Russian:"приложить силу величиной",Chinese:"施力",Kazakh:"шамамен күш салу",Korean:"에(게) 다음의 힘을 가함 힘:")]
        [ArgsFormat(English:"{%} in direction of {%} relative to the {%}",Russian:"{%} в направлении {%} относительно {%}",Chinese:"{%}朝向{%}，相對於{%}",Kazakh:"{%}-ға қатысты {%} бағытында {%}",Korean:"{%} 방향:{%} 기준:{%}")]
        public void ApplyForceInDirection(
            [SourceTypeContainer(typeof(float))] dynamic forceValue,
            [SourceTypeContainer(typeof(Vector3))] dynamic forceDirection,
            [SourceTypeContainer(typeof(Relativeness))] dynamic relativeness)
        {
            if (!Rigidbody)
            {
                return;
            }

            string methodName = nameof(ApplyForceInDirection);

            if (!ValidateMethodWithLog(this, forceValue, methodName, 0, out float convertedForceValue))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, forceDirection, methodName, 1, out Vector3 convertedForceDirection))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, relativeness, methodName, 2, out Relativeness convertedRelativeness))
            {
                return;
            }

            var force = convertedForceDirection.normalized * convertedForceValue;

            switch (convertedRelativeness)
            {
                case Relativeness.Self:
                    Rigidbody.AddRelativeForce(force, ForceMode.Impulse);
                    break;
                case Relativeness.World:
                    Rigidbody.AddForce(force, ForceMode.Impulse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(convertedRelativeness), convertedRelativeness, null);
            }
        }

        [Obsolete]
        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Instantly applies force to the object in the direction of the specified vector in the selected coordinate system. The value is measured in kg*m/s.",Russian:"Мгновенно прикладывает силу к объекту в направлении заданного вектора в выбранной системе координат. Величина измеряется в кг*м/с.",Chinese:"瞬間以一個方向對一個物件施力。該值以 kg*m/s 為單位測量。",Kazakh:"Таңдалған координаттар жүйесінде берілген вектор бағытында объектіге лезде күш салады. Шама кг*м/с-пен өлшенеді.",Korean:"선택한 좌표계에서 지정된 벡터 방향으로 물체에 즉시 힘을 가합니다. 값은 kg*m/s 단위로 측정됩니다.")]
        [Action(English:"apply force of",Russian:"приложить силу величиной",Chinese:"施力",Kazakh:"шамамен күш салу",Korean:"에(게) 다음의 힘을 가함 힘:")]
        [ArgsFormat(English:"{%} in direction of {%} relative to the {%}",Russian:"{%} в направлении {%} относительно {%}",Chinese:"{%}朝向{%}，相對於{%}",Kazakh:"{%}-ға қатысты {%} бағытында {%}",Korean:"{%} 방향:{%} 기준:{%}")]
        public void ApplyForceInDirectionRelativeTo(
            [SourceTypeContainer(typeof(float))] dynamic forceValue,
            [SourceTypeContainer(typeof(Vector3))] dynamic forceDirection,
            [SourceTypeContainer(typeof(Relativeness))] dynamic relativeness)
        {
            if (!Rigidbody)
            {
                return;
            }

            string methodName = nameof(ApplyForceInDirectionRelativeTo);

            if (!ValidateMethodWithLog(this, forceValue, methodName, 0, out float convertedForceValue))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, forceDirection, methodName, 1, out Vector3 convertedForceDirection))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, relativeness, methodName, 2, out Relativeness convertedRelativeness))
            {
                return;
            }

            var force = convertedForceDirection.normalized * convertedForceValue;

            switch (relativeness)
            {
                case Relativeness.Self:
                    Rigidbody.AddRelativeForce(convertedForceDirection, ForceMode.Impulse);
                    break;
                case Relativeness.World:
                    Rigidbody.AddForce(force, ForceMode.Impulse);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(convertedRelativeness), convertedRelativeness, null);
            }
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Applies a force to an object in the direction of a specified vector in the selected coordinate system for the specified time. The value is measured in kg*m/s.",Russian:"Прикладывает силу к объекту в направлении заданного вектора в выбранной системе  координат в течение указанного времени. Величина измеряется в кг*м/с.",Chinese:"以指定的方向向量和時間對物件施力。該值以 kg*m/s 為單位測量。",Kazakh:"Таңдалған координаттар жүйесінде берілген вектор бағытында объектіге күш салады. Шама кг*м/с-пен өлшенеді.",Korean:"지정된 시간 동안 선택된 좌표계에서 지정된 벡터 방향으로 객체에 힘을 가합니다. 값은 kg*m/s 단위로 측정 됩니다.")]
        [Action(English:"apply force of",Russian:"приложить силу величиной",Chinese:"施力",Kazakh:"шамамен күш салу",Korean:"에(게) 다음의 힘을 가함 힘:")]
        [ArgsFormat(English:"{%} in direction of {%} for {%} s. relative to the {%}",Russian:"{%} в направлении {%} в течение {%} с. относительно {%}",Chinese:"{%}朝向{%}，持續{%}秒，相對於{%}",Kazakh:"{%} бағытында {%} с. ішінде {%}-ға қатысты {%}",Korean:"{%} 방향:{%} 시간:{%}s 기준:{%}")]
        public IEnumerator StartApplyingForceInDirectionRelativeTo(
            [SourceTypeContainer(typeof(float))] dynamic forceValue, 
            [SourceTypeContainer(typeof(Vector3))] dynamic forceDirection,
            [SourceTypeContainer(typeof(float))] dynamic duration,
            [SourceTypeContainer(typeof(Relativeness))] dynamic relativeness)
        {
            string methodName = nameof(StartApplyingForceInDirectionRelativeTo);

            if (!ValidateMethodWithLog(this, forceValue, methodName, 0, out float convertedForceValue))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, forceDirection, methodName, 1, out Vector3 convertedForceDirection))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, duration, methodName, 2, out float convertedDuration))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, relativeness, methodName, 3, out Relativeness convertedRelativeness))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentForceRoutines.Add(routineId);

            _currentState.IsPerforming = true;

            var force = convertedForceDirection.normalized * convertedForceValue;

            var travelTime = 0f;

            while (travelTime <= convertedDuration && _currentState.IsPerforming)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                switch (convertedRelativeness)
                {
                    case Relativeness.Self:
                        Rigidbody.AddRelativeForce(force);
                        break;
                    case Relativeness.World:
                        Rigidbody.AddForce(force);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(convertedRelativeness), convertedRelativeness, null);
                }

                yield return WaitForEndOfFrame;

                travelTime += Time.fixedDeltaTime;
            }

            _currentForceRoutines.Remove(routineId);
            OnForceApplyFinished?.Invoke();
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Controls any force application to object. A paused force application can be continued with the “Continue” block.",Russian:"Управляет действием любой силы на объект. Приостановленное действие силы можно возобновить блоком “Продолжить”.",Chinese:"控制對物件施加的任何作用力，可藉由繼續方塊來恢復暫停中的作用力",Kazakh:"Кез келген күштің объектіге әсер етуін басқарады. Тоқтатыла тұрған күштің әсер етуін “Жалғастыру” блогымен қайта бастауға болады.",Korean:"객체에 가해지는 힘을 제어합니다. 일시 중지된 힘의 적용은 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("PhysicsControl")]
        [Action(English:"stop any force application",Russian:"остановить действие любой силы",Chinese:"停止任何施力",Kazakh:"кез келген күштің әсер етуін тоқтату",Korean:"에 대한 모든 힘의 적용을 정지")]
        public void StopAnyForce()
        {
            _currentState.IsPerforming = false;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(
English:"Controls any force application to object. A paused force application can be continued with the “Continue” block.",Russian:"Управляет действием любой силы на объект. Приостановленное действие силы можно возобновить блоком “Продолжить”.",Chinese:"控制對物件施加的任何作用力，可藉由繼續方塊來恢復暫停中的作用力",Kazakh:"Кез келген күштің объектіге әсер етуін басқарады. Тоқтатыла тұрған күштің әсер етуін “Жалғастыру” блогымен қайта бастауға болады.",Korean:"객체에 가해지는 힘을 제어합니다. 일시 중지된 힘의 적용은 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("PhysicsControl")]
        [Action(English:"pause any force application",Russian:"приостановить действие любой силы",Chinese:"暫停任何施力",Kazakh:"кез келген күштің әсер етуін тоқтата тұру",Korean:"에 대한 모든 힘의 적용을 일시 정지")]
        public void PauseAnyForce()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(
English:"Controls any force application to object. A paused force application can be continued with the “Continue” block.",Russian:"Управляет действием любой силы на объект. Приостановленное действие силы можно возобновить блоком “Продолжить”.",Chinese:"控制對物件施加的任何作用力，可藉由繼續方塊來恢復暫停中的作用力",Kazakh:"Кез келген күштің объектіге әсер етуін басқарады. Күштің тоқтатыла тұрған әсер етуін “Жалғастыру” блогымен қайта бастауға болады.",Korean:"물체에 가해지는 힘을 제어합니다. 일시 중지된 힘의 적용은 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("PhysicsControl")]
        [Action(English:"continue any force application",Russian:"продолжить действие любой силы",Chinese:"恢復任何施力",Kazakh:"кез келген күштің әсер етуін жалғастыру",Korean:"에 대한 모든 힘의 적용을 계속")]
        public void ContinueAnyForce()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Variables

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English:"value of the physical property mass",Russian:"величина физического свойства масса",Chinese:"質量的數值",Kazakh:"масса физикалық қасиетінің шамасы",Korean:"의 물리적 특성 질량 값")]
        public float MassGetter => Rigidbody.mass;

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English:"value of the physical property bounciness",Russian:"величина физического свойства пружинистость",Chinese:"彈性的數值",Kazakh:"серіппелілік физикалық қасиетінің шамасы",Korean:"의 물리적 특성 탄력 값")]
        public float BouncinessGetter => Collider.material.bounciness;

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English:"value of the physical property acceleration",Russian:"величина физического свойства ускорение",Chinese:"加速度的數值",Kazakh:"үдеу физикалық қасиетінің шамасы",Korean:"의 물리적 특성 가속도 값")]
        public float AccelerationGetter => Acceleration;

        public float Acceleration { private set; get; }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English:"value of the physical property speed",Russian:"величина физического свойства скорость",Chinese:"速度的數值",Kazakh:"жылдамдық физикалық қасиетінің шамасы",Korean:"의 물리적 특성 속력 값")]
        public float SpeedGetter => Rigidbody.velocity.magnitude;

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English:"value of the physical property angular speed",Russian:"величина физического свойства угловая скорость",Chinese:"角速度的數值",Kazakh:"бұрыштық жылдамдық физикалық қасиетінің шамасы",Korean:"의 물리적 특성 각속도 값")]
        public float AngularSpeedGetter => Rigidbody.angularVelocity.magnitude;
        
        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property linear drag", Russian: "величина физического свойства линейное сопротивление", Chinese: "线性阻力的数值", Kazakh: "сызықтық кедергі физикалық қасиетінің шамасы", Korean: "물리적 특성 선형 드래그의 값")]
        public float LinearDragGetter => Rigidbody.drag;

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Returns the value of the selected physical property of the object.",Russian:"Возвращает величину выбранного физического свойства объекта.",Chinese:"回傳物件的其中一項物理性質的數值",Kazakh:"Объектінің таңдалған физикалық қасиетінің шамасын қайтарады.",Korean:"객체의 선택된 물리적 특성 값을 반환합니다.")]
        [VariableGroup("PhysicsValueGet")]
        [Variable(English: "value of the physical property angular drag", Russian: "величина физического свойства угловое сопротивление", Chinese: "旋转阻力的数值", Kazakh: "бұрыштық кедергі физикалық қасиетінің шамасы", Korean: "물리적 특성 회전 드래그의 값")]
        public float AngularDragGetter => Rigidbody.angularDrag;

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets the value of one of the physical properties of the object.",Russian:"Задает величину одного из физических свойств объекта.",Chinese:"設定物件的其中一項物理性質",Kazakh:"Объектінің физикалық қасиеттерінің бірінің шамасын белгілейді.",Korean:"객체의 물리적 특성 중 하나의 값을 설정합니다.")]
        [VariableGroup("PhysicsValueSet")]
        [Variable(English:"value of the physical property mass",Russian:"величина физического свойства масса",Chinese:"質量的數值",Kazakh:"масса физикалық қасиетінің шамасы",Korean:"의 물리적 특성 질량 값")]
        [SourceTypeContainer(typeof(float))]
        public dynamic MassSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(MassSetter), out float convertedValue))
                {
                    return;
                }

                Rigidbody.mass = convertedValue;
            }
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets the value of one of the physical properties of the object.",Russian:"Задает величину одного из физических свойств объекта.",Chinese:"設定物件的其中一項物理性質",Kazakh:"Объектінің физикалық қасиеттерінің бірінің шамасын белгілейді.",Korean:"객체의 물리적 특성 중 하나의 값을 설정합니다.")]
        [VariableGroup("PhysicsValueSet")]
        [Variable(English:"value of the physical property bounciness",Russian:"величина физического свойства пружинистость",Chinese:"彈性的數值",Kazakh:"серіппелілік физикалық қасиетінің шамасы",Korean:"의 물리적 특성 탄력 값")]
        [SourceTypeContainer(typeof(float))]
        public dynamic BouncinessSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(BouncinessSetter), out float convertedValue))
                {
                    return;
                }

                Collider.material.bounciness = Mathf.Clamp01(convertedValue);
            }
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets the value of one of the physical properties of the object.",Russian:"Задает величину одного из физических свойств объекта.",Chinese:"設定物件的其中一項物理性質",Kazakh:"Объектінің физикалық қасиеттерінің бірінің шамасын белгілейді.",Korean:"객체의 물리적 특성 중 하나의 값을 설정합니다.")]
        [VariableGroup("PhysicsValueSet")]
        [Variable(English: "value of the physical property linear drag", Russian: "величина физического свойства линейное сопротивление", Chinese: "线性阻力的数值", Kazakh: "сызықтық кедергі физикалық қасиетінің шамасы", Korean: "물리적 특성 선형 드래그의 값")]
        [SourceTypeContainer(typeof(float))]
        public dynamic LinearDragSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(LinearDragSetter), out float convertedValue))
                {
                    return;
                }

                Rigidbody.drag = convertedValue;
            }
        }
        
        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets the value of one of the physical properties of the object.",Russian:"Задает величину одного из физических свойств объекта.",Chinese:"設定物件的其中一項物理性質",Kazakh:"Объектінің физикалық қасиеттерінің бірінің шамасын белгілейді.",Korean:"객체의 물리적 특성 중 하나의 값을 설정합니다.")]
        [VariableGroup("PhysicsValueSet")]
        [Variable(English: "value of the physical property angular drag", Russian: "величина физического свойства угловое сопротивление", Chinese: "旋转阻力的数值", Kazakh: "бұрыштық кедергі физикалық қасиетінің шамасы", Korean: "물리적 특성 회전 드래그의 값")]
        [SourceTypeContainer(typeof(float))]
        public dynamic AngularDragSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(AngularDragSetter), out float convertedValue))
                {
                    return;
                }

                Rigidbody.angularDrag = convertedValue;
            }
        }
        
        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets whether gravity affects the object.",Russian:"Задает, воздействует ли гравитация на объект.",Chinese:"設定物件是否受重力影響",Kazakh:"Гравитацияның объектіге әсер ететін-етпейтінін белгілейді.",Korean:"중력이 객체에 영향을 미치는지 여부를 설정합니다.")]
        [Variable(English:"physical property",Russian:"физическое свойство",Chinese:"物理屬性",Kazakh:"физикалық қасиет",Korean:"의 물리적 특성")]
        [SourceTypeContainer(typeof(Gravity))]
        public dynamic GravitySetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(GravitySetter), out Gravity convertedValue))
                {
                    return;
                }

                Rigidbody.useGravity = convertedValue == Gravity.GravityOn;
            }
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets whether the object is static. If the object is static, there are no physical forces acting on it.",Russian:"Задает статичность указанного объекта. Если объект статичный, никакие физические силы не воздействуют на него.",Chinese:"設定物件是否為靜態物件。如果物件為靜態，則不受任何物理作用力影響。",Kazakh:"Көрсетілген объектінің статикалығын белгілейді. Егер объект статикалық болса, оған ешқандай физикалық күштер әсер етпейді.",Korean:"객체가 고정 상태인지 여부를 설정합니다. 물체가 고정 상태면 물체에 작용하는 물리적 힘이 없습니다.")]
        [Variable(English:"physical property",Russian:"физическое свойство",Chinese:"物理屬性",Kazakh:"физикалық қасиет",Korean:"의 물리적 특성")]
        [SourceTypeContainer(typeof(Kinematic))]
        public dynamic KinematicSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(KinematicSetter), out Kinematic convertedValue))
                {
                    return;
                }

                Rigidbody.isKinematic = convertedValue == Kinematic.Kinematic;
            }
        }

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"Sets whether the specified object is an obstacle for the player and other objects.",Russian:"Задает, является ли указанный объект препятствием для игрока и других объектов.",Chinese:"設定物件是否可被穿越",Kazakh:"Көрсетілген объект ойыншы мен басқа объектілер үшін кедергі болып табылатын-табылмайтынын белгілейді.",Korean:"지정된 객체가 플레이어 및 기타 객체에 대한 장애물인지 여부를 설정합니다.")]
        [Variable(English:"physical property",Russian:"физическое свойство",Chinese:"物理屬性",Kazakh:"физикалық қасиет",Korean:"의 물리적 특성")]
        [SourceTypeContainer(typeof(Obstacle))]
        public dynamic ObstacleSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(ObstacleSetter), out Obstacle convertedValue))
                {
                    return;
                }

                if (_nonTriggerColliders == null)
                {
                    InitColliders();
                }

                foreach (var col in _nonTriggerColliders)
                {
                    col.isTrigger = convertedValue == Obstacle.NonObstacle;
                }
            }
        }
        
        [VarwinSerializable]
        public bool BackwardCompatibilityIsInitialized { get; set; }

        #endregion

        #region Checkers

        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(
English:"Returns true if a force is currently being applied to the specified object. Otherwise, returns “false“",Russian:"Возвращает “истину”, если сила действует на указанный объект в данный момент. В противном случае возвращает “ложь”",Chinese:"若指定物件正被施加作用力則回傳值為真；反之，回傳為假",Kazakh:"Егер көрсетілген объектіге күш осы сәтте әсер етсе, “шындықты” қайтарады. Олай болмаған жағдайда “өтірікті” қайтарады.",Korean:"지정된 개체에 힘이 적용되고 있는 중이면 참(true)을 반환함. 그렇지 않으면 거짓(false)을 반환함.")]
        [Checker(English:"is affected by force at the moment",Russian:"подвержен приложению силы в данный момент",Chinese:"正在受外力影響",Kazakh:"осы сәтте күш салынуына бейім",Korean:"힘에 영향을 받았다면")]
        public bool IsAffectedByForceNow()
        {
            return _currentForceRoutines.Count > 0;
        }

        #endregion

        #region Events

        private event TriggerHandler _physicsTriggerEntered;
        private event TriggerHandler _physicsTriggerExited;
        
        [LogicGroup(English:"Physics",Russian:"Физика",Chinese:"物理",Kazakh:"Физика",Korean:"물리")]
        [LogicTooltip(English:"The event is triggered when the force has finished being applied to the object. The object for which the event was triggered is passed to the parameter.",Russian:"Событие срабатывает, когда сила перестает действовать на указанный объект. В параметр передается объект, для которого сработало событие.",Chinese:"此事件會在施加在物件上的力結束時觸發。觸發事件的物件會被傳送給參數",Kazakh:"Күш көрсетілген объектіге әсер етуін тоқтатқан кезде оқиға іске қосылады. Параметрге сол үшін оқиға іске қосылған объект беріледі.",Korean:"힘이 물체에 적용되는 것이 완료되면 이벤트가 작동됩니다. 이벤트가 작동된 개체가 매개변수에 전달됩니다.")]
        [LogicEvent(English:"the force has finished being applied to the object",Russian:"сила перестала действовать на объект",Chinese:"此事件會在施加在物件上的力結束時觸發。觸發事件的物件會被傳送給參數",Kazakh:"күш объектіге әсер етуін тоқтатты",Korean:"힘이 객체에 적용되는 것이 완료되었을 때")]
        public event CommonForceHandler OnForceApplyFinished;

        [LogicGroup(English: "Physics", Russian: "Физика", Chinese: "物理", Korean: "물리")]
        [LogicTooltip(English: "The event is triggered when the specified object gets inside or outside another object. The parameters include the specified object and the object inside which the specified object got in or out.", Russian: "Событие срабатывает, когда указанный объект попал внутрь или вышел из другого объекта. В параметры передаются указанный объект и объект, внутрь которого попал или вышел указанный.", Chinese: "此事件會在指定物件進出其他物件時觸發。參數包含指定物件和物件進出的對象", Korean: "지정된 객체가 다른 객체의 내부 또는 외부를 거칠 때 이벤트가 작동됩니다. 매개변수에는 지정된 객체와 지정된 객체가 들어갔거나 나간 객체가 포함됩니다.")]
        [EventGroup("PhysicsTriggerEvents")]
        [LogicEvent(English: "object got inside of target object", Russian: "объект попал внутрь целевого объекта", Chinese: "物件進入目標物件", Korean: "객체가 목표 객체 내부에 들어감")]
        [Obsolete]
        public event TriggerHandler OnPhysicsTriggerEnter
        {
            add
            {
                if (_physicsTriggerEntered == null)
                {
                    CollisionDispatcher.AddCollisionStartHandler(gameObject.GetWrapper(), GameStateData.GetWrapperCollection().All, TriggerEnter);    
                }
                
                _physicsTriggerEntered += value;
            }
            remove => _physicsTriggerEntered -= value;
        }


        [LogicGroup(English: "Physics", Russian: "Физика", Chinese: "物理", Korean: "물리")]
        [LogicTooltip(
            English: "The event is triggered when the specified object gets inside or outside another object. The parameters include the specified object and the object inside which the specified object got in or out.",
            Russian: "Событие срабатывает, когда указанный объект попал внутрь или вышел из другого объекта. В параметры передаются указанный объект и объект, внутрь которого попал или вышел указанный.",
            Chinese: "此事件會在指定物件進出其他物件時觸發。參數包含指定物件和物件進出的對象",
            Korean: "지정된 객체가 다른 객체의 내부 또는 외부를 거칠 때 이벤트가 작동됩니다. 매개변수에는 지정된 객체와 지정된 객체가 들어갔거나 나간 객체가 포함됩니다.")]
        [EventGroup("PhysicsTriggerEvents")]
        [LogicEvent(English: "object got outside of target object", Russian: "объект вышел наружу целевого объекта", Chinese: "物件穿出目標物件", Korean: "객체가 목표 객체 외부로 나감")]
        [Obsolete]
        public event TriggerHandler OnPhysicsTriggerExit
        {
            add
            {
                if (_physicsTriggerExited == null)
                {
                    CollisionDispatcher.AddCollisionEndHandler(gameObject.GetWrapper(), GameStateData.GetWrapperCollection().All, TriggerExit);    
                }
                
                _physicsTriggerExited += value;
            }
            remove => _physicsTriggerExited -= value;
        }

        #endregion

        #region PrivateHelpers

        public void OnEnable()
        {
            TrySetPhysicsSettings();
        }

        public void OnDisable()
        {
            StopAllCoroutines();
        }
        
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            
            if (!BackwardCompatibilityIsInitialized)
            {
                var prevInteractableBehaviour = Wrapper.GetBehaviour<InteractableBehaviour>();
                if (prevInteractableBehaviour && prevInteractableBehaviour.Mass > Mathf.Epsilon)
                {
                    MassInspector = prevInteractableBehaviour.Mass;
                    BouncinessInspector = prevInteractableBehaviour.Bounciness;
                    KinematicInspector = prevInteractableBehaviour.IsStatic;
                    ObstacleInspector = prevInteractableBehaviour.Collision;
                    GravityInspector = prevInteractableBehaviour.Gravity;
                    
                }

                BackwardCompatibilityIsInitialized = true;
                ObjectController.RefreshInspector();
            }
        }

        private void FixedUpdate()
        {
            var currentVelocity = Rigidbody.velocity;

            Acceleration = ((currentVelocity - _lastFrameVelocity) / Time.fixedDeltaTime).magnitude;

            _lastFrameVelocity = currentVelocity;
        }

        private IEnumerator TriggerEnter(object sender, object target)
        {
            if (target is Wrapper targetWrapper)
            {
                _physicsTriggerEntered?.Invoke(targetWrapper);    
            }
            
            yield break;
        }
        
        private IEnumerator TriggerExit(object sender, object target)
        {
            if (target is Wrapper targetWrapper)
            {
                _physicsTriggerExited?.Invoke(targetWrapper);    
            }
            
            yield break;
        }

        public void TrySetPhysicsSettings()
        {
            if (!ProjectData.IsPlayMode)
            {
                return;
            }
            
            ObjectController controller = gameObject.GetWrapper().GetObjectController();
            if (controller == null || controller.Parent is {LockChildren: true})
            {
                return;
            }
            
            Rigidbody.useGravity = GravityInspector;
            Rigidbody.isKinematic = KinematicInspector;
        }
        
        #endregion

        private void InitColliders()
        {
            var allColliders = GetComponentsInChildren<Collider>(true);
            _thisCollider = allColliders[0]; // TODO how to be sure this is the "main" collider?
            foreach (var childCollider in allColliders)
            {
                if (childCollider.isTrigger || childCollider is CharacterController)
                {
                    continue;
                }

                _nonTriggerColliders.Add(childCollider);
            }
            
            if (_thisCollider && _thisCollider.attachedRigidbody == Rigidbody && !_nonTriggerColliders.Contains(_thisCollider))
            {
                _nonTriggerColliders.Add(_thisCollider);
            }
        }

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            TrySetPhysicsSettings();
        }
    }
}

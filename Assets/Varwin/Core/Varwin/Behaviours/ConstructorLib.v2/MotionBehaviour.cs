using System;
using System.Collections;
using System.Collections.Generic;
using SmartLocalization;
using UnityEngine;
using Varwin.Log;
using Varwin.Public;
using static Varwin.TypeValidationUtils;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class MotionBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool IsDisabledBehaviour(GameObject targetGameObject)
        {
            return IsDisabledBehaviour(targetGameObject, BehaviourType.Motion);
        }
    }
    
    [VarwinComponent(English:"Motion",Russian:"Движение",Chinese:"動作",Kazakh:"Қозғалыс",Korean:"움직임")]
    public class MotionBehaviour : ConstructorVarwinBehaviour
    {
        public enum LockRotationRules
        {
            [Item(English:"do not rotate",Russian:"не поворачивать",Chinese:"鎖定旋轉",Kazakh:"бұрмау",Korean:"회전 금지")] 
            Lock,
            [Item(English:"rotate only in the horizontal axis",Russian:"поворот только по горизонтальной оси",Chinese:"鎖定於橫軸旋轉",Kazakh:"тек көлденең ось бойынша бұрылыс",Korean:"수평축으로만 회전")] 
            OnlyHorizontal,
            [Item(English:"rotate on all axes",Russian:"поворот по всем осям",Chinese:"允許所有軸向旋轉",Kazakh:"барлық осьтер бойынша бұрылыс",Korean:"모든 축으로 회전")] 
            AllowAll
        }

        private readonly List<int> _currentMotionRoutines = new();
        private int _routineId;

        private readonly BehaviourState _currentState = new();
        private float _minimumTargetStopDistance;

        public delegate void CommonMovementHandler();

        public delegate void ToWrapperMovementHandler([Parameter(English:"target object",Russian:"целевой объект",Chinese:"目標物件",Kazakh:"мақсатты объект",Korean:"목표 객체")] Wrapper target);

        public delegate void ToVectorMovementHandler([Parameter(English:"target coordinates",Russian:"целевые координаты",Chinese:"目標座標",Kazakh:"мақсатты координаттар",Korean:"목표 좌표")] Vector3 target);

        public delegate void WayPointMovementHandler(
            [Parameter(English:"waypoint number",Russian:"номер точки в маршруте",Chinese:"導航點編號",Kazakh:"маршруттағы нүктенің нөмірі",Korean:"웨이포인트 번호")] int wayPointIndex,
            [Parameter(English:"waypoint reached",Russian:"достигнутая точка",Chinese:"抵達導航點",Kazakh:"маршруттағы нүктесі жетті",Korean:"웨이포인트 도달")] dynamic waypoint);
        #region Actions

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Instantly moves the specified object to a position set by world space coordinates.",Russian:"Мгновенно перемещает указанный объект в позицию, заданную с помощью координат в мировом пространстве.",Chinese:"瞬間移動指定的物件至指定的空間座標",Kazakh:"Көрсетілген объектіні әлемдік кеңстіктегі координаттар көмегімен берілген позицияға лезде орын ауыстырады.",Korean:"지정된 객체를 세계 공간 좌표에 의해 설정된 위치로 즉시 이동시킵니다")]
        [Action(English:"set position",Russian:"задать позицию",Chinese:"設定位置",Kazakh:"позицияны беру",Korean:"의 위치 설정")]
        public void SetPosition([SourceTypeContainer(typeof(Vector3))] dynamic targetPosition)
        {
            if (!ValidateMethodWithLog(this, targetPosition, nameof(SetPosition), 0, out Vector3 convertedPosition))
            {
                return;
            }

            transform.position = convertedPosition;
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Instantly moves the specified object to the coordinates of the second object.",Russian:"Мгновенно перемещает указанный объект по координатам второго объекта.",Chinese:"瞬間移動指定的物件至另一個物件的座標",Kazakh:"Көрсетілген объектіні екінші объектінің координаттары бойынша лезде орын ауыстырады.",Korean:"지정된 객체를 두 번째 객체의 좌표로 즉시 이동시킵니다.")]
        [Action(English:"instantly move to the center of object",Russian:"мгновенно переместиться в центр объекта",Chinese:"瞬間移動至物件中點",Kazakh:"объектінің центріне лезде орын ауыстыру",Korean:"을(를) 다음 객체의 중심으로 즉시 이동")]
        public void TeleportTo([SourceTypeContainer(typeof(Wrapper))] dynamic targetObject)
        {
            if (!ValidateMethodWithLog(this, targetObject, nameof(TeleportTo), 0, out Wrapper convertedTargetObject))
            {
                return;
            }

            Transform destinationTransform = convertedTargetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            SetPosition(destinationTransform.position);
            OnAnyMovementFinished?.Invoke();
            OnToWrapperMovementFinished?.Invoke(convertedTargetObject);
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Starts the moving process of the specified object in the direction of the selected axis at the specified speed. The movement continues until it is stopped by the movement stop block. Use negative speed values to change the moving direction.",Russian:"Запускает процесс перемещения указанного объекта в направлении выбранной оси с заданной скоростью. Перемещение продолжается, пока оно не будет остановлено блоком завершения перемещения. Чтобы изменить направление перемещения, используйте отрицательные значение скорости.",Chinese:"以指定的方向和速度移動物件，直到被停止移動方塊停止前此動作都將持續。使用負的速度數值以改變移動方向。",Kazakh:"Көрсетілген объектінің таңдалған ось бағытында берілген жылдамдықпен орын ауыстыру процесін іске қосады. Орын ауыстыру блогы тоқтатқанша орын ауыстыру жалғаса береді. Орын ауыстырудың бағытын өзгерту үшін жылдамдықтың теріс мәнін пайдаланыңыз.",Korean:"지정된 객체가 선택한 축 방향으로 지정한 속도로 이동을 시작합니다. 장애물에 의해 정지될 때까지 이동은 계속됩니다. 이동 방향을 변경하려면 음수 값을 사용합니다.")]
        [Action(English:"move in direction of axis",Russian:"перемещаться в направлении оси",Chinese:"沿著指定軸向移動",Kazakh:"ось бағытында орын ауыстыру",Korean:"이(가) 다음 축의 방향으로 이동")]
        [ArgsFormat(English:"{%} at a speed of {%} m/s",Russian:"{%} со скоростью {%} м/с",Chinese:"{%}以每秒{%}公尺的速度",Kazakh:"{%} м/с жылдамдығымен {%}",Korean:"{%} 속도:{%}m/s")]
        public void MoveByAxisWithSpeed([SourceTypeContainer(typeof(Axis))] dynamic axis, [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            if (!ValidateMethodWithLog(this, axis, nameof(MoveByAxisWithSpeed), 0, out Axis convertedAxis))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, speed, nameof(MoveByAxisWithSpeed), 1, out float convertedSpeed))
            {
                return;
            }

            StartCoroutine(MoveByAxisWithSpeedCoroutine(convertedAxis, convertedSpeed));
        }

        private IEnumerator MoveByAxisWithSpeedCoroutine(Axis axis, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = EnumToVector(axis) * speed;

            while (_currentState.IsMoving)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;
            }

            _currentMotionRoutines.Remove(routineId);
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Starts the moving process of the specified object in the direction of the selected axis by the specified distance at the specified speed. The movement continues until the object covers the distance. Use negative speed values to change the moving direction.",Russian:"Запускает процесс перемещения указанного объекта в направлении выбранной оси на заданное расстояние с заданной скоростью. Перемещение продолжается, пока объект не преодолеет расстояние. Чтобы изменить направление перемещения, используйте отрицательные значение скорости.",Chinese:"以指定的方向、距離和速度移動物件，直到物件移動指定的距離之前此動作都將持續。使用負的速度數值以改變移動方向。",Kazakh:"Көрсетілген объектіні таңдалған ось бағытында берілген жылдамдықпен берілген қашықтыққа орын ауыстыру процесін іске қосады. Объект ара қашықтықты еңсергенше орын ауыстыру жалғасады. Орын ауыстырудың бағытын өзгерту үшін жылдамдықтың теріс мәнін пайдаланыңыз.",Korean:"지정된 객체가 선택한 축 방향으로 지정한 속도로 지정된 거리만큼 이동을 시작합니다. 객체가 해당 거리를 이동할 때 까지 계속됩니다. 이동 방향을 변경하려면 음수 값을 사용합니다.")]
        [Action(English:"move in direction of axis",Russian:"перемещаться в направлении оси",Chinese:"沿著指定軸向移動",Kazakh:"ось бағытында орын ауыстыру",Korean:"이(가) 다음 축의 방향으로 이동")]
        [ArgsFormat(English:"{%} to a distance of {%} m at a speed of {%} m/s",Russian:"{%} на расстояние {%} м со скоростью {%} м/с",Chinese:"{%}距離{%}公尺，並以每秒{%}公尺的速度",Kazakh:"{%} м/с жылдамдығымен {%}ара қашықтығына {%}",Korean:"{%} 거리:{%}m 속도:{%}m/s")]
        public IEnumerator MoveByAxisAtDistance([SourceTypeContainer(typeof(Axis))] dynamic axis, [SourceTypeContainer(typeof(float))] dynamic distance,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            string methodName = nameof(MoveByAxisAtDistance);

            if (!ValidateMethodWithLog(this, axis, methodName, 0, out Axis convertedAxis))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, distance, methodName, 1, out float convertedDistance))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 2, out float convertedSpeed))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = EnumToVector(convertedAxis) * convertedSpeed;
            var startPosition = transform.position;

            while (Vector3.Distance(startPosition, transform.position) < convertedDistance && _currentState.IsMoving)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;
            }

            _currentMotionRoutines.Remove(routineId);
            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Starts the moving process of the specified object in the direction of the selected axis for the specified time at the specified speed. Moving continues until the time has run out. Use negative speed values to change the moving direction.",Russian:"Запускает процесс перемещения указанного объекта в направлении выбранной оси в течение указанного времени с заданной скоростью. Перемещение продолжается, пока не истечет время. Чтобы изменить направление перемещения, используйте отрицательные значение скорости.",Chinese:"以指定的方向、時間和速度移動物件，直到時間經過為止此動作都將持續。使用負的速度數值以改變移動方向。",Kazakh:"Көрсетілген объектінің таңдалған ось бағытында берілген жылдамдықпен берілген қашықтыққа орын ауыстыру процесін іске қосады. Орын ауыстыру уақыт таусылғанша жалғасады. Орын ауыстырудың бағытын өзгерту үшін жылдамдықтың теріс мәнін пайдаланыңыз.",Korean:"지정된 객체가 선택한 축 방향으로 지정된 속도로 지정된 거리만큼 이동을 시작합니다. 시간이 다 될 때까지 이동이 계속됩니다. 이동 방향을 변경하려면 음수 값을 사용합니다.")]
        [Action(English:"move in direction of axis",Russian:"перемещаться в направлении оси",Chinese:"沿著指定軸向移動",Kazakh:"ось бағытында орын ауыстыру",Korean:"이(가) 다음 축의 방향으로 이동")]
        [ArgsFormat(English:"{%} for {%} s. at a speed of {%} m/s",Russian:"{%} в течение {%} с. со скоростью {%} м/с",Chinese:"{%}在{%}秒內，並以每秒{%}公尺的速度",Kazakh:"{%} м/с жылдамдығымен {%} с. ішінде {%}",Korean:"{%} 시간:{%}s 속도:{%}m/s")]
        public IEnumerator MoveByAxisByTime([SourceTypeContainer(typeof(Axis))] dynamic axis, [SourceTypeContainer(typeof(float))] dynamic duration,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            string methodName = nameof(MoveByAxisByTime);

            if (!ValidateMethodWithLog(this, axis, methodName, 0, out Axis convertedAxis))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, duration, methodName, 1, out float convertedDuration))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 2, out float convertedSpeed))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var velocity = EnumToVector(convertedAxis) * convertedSpeed;
            var travelTime = 0f;

            while (travelTime <= convertedDuration && _currentState.IsMoving)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetPosition(transform.position + velocity * Time.deltaTime);
                yield return WaitForEndOfFrame;

                travelTime += Time.deltaTime;
            }

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Starts the moving process of the specified object in the direction of the second object at the specified speed. The movement continues until the specified object reaches the second object.",Russian:"Запускает процесс перемещения указанного объекта в направлении второго объекта с заданной скоростью. Перемещение продолжается, пока указанный объект не достигнет второго объекта.",Chinese:"以指定的速度面向另一個物件來移動指定的物件，直到抵達第二個物件以前此動作都將持續。",Kazakh:"Көрсетілген объектінің екінші объект бағытында берілген жылдамдықпен орын ауыстыру процесін іске қосады. Объект екінші объектіге жетпейінше, орын ауыстыру жалғаса береді.",Korean:"지정된 객체가 선택한 속도로 두 번째 객체 방향으로 이동을 시작합니다. 지정된 객체가 두 번째 객체에 도달할 때까지 이동이 계속됩니다.")]
        [Action(English:"move to object",Russian:"перемещаться к объекту",Chinese:"移動至物件",Kazakh:"объектіге қарай орын ауыстыру",Korean:"이(가) 다음 객체로 이동")]
        [ArgsFormat(English:"{%} at a speed of {%} m/s",Russian:"{%} со скоростью {%} м/с",Chinese:"{%}以每秒{%}公尺的速度",Kazakh:"{%} м/с жылдамдығымен {%}",Korean:"{%} 속도:{%}m/s")]
        public IEnumerator MoveToObjectAtSpeed([SourceTypeContainer(typeof(Wrapper))] dynamic targetObject, [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            string methodName = nameof(MoveToObjectAtSpeed);

            if (!ValidateMethodWithLog(this, targetObject, methodName, 0, out Wrapper convertedTargetObject))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            var destinationTransform = convertedTargetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            yield return MoveToPointAtSpeed(destinationTransform.position, convertedSpeed);
            OnToWrapperMovementFinished?.Invoke(convertedTargetObject);
            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Starts the moving process of the specified object in the direction of the specified coordinates at the specified speed. The movement continues until the object reaches the coordinates.",Russian:"Запускает процесс перемещения указанного объекта в направлении указанных координат с заданной скоростью. Перемещение продолжается, пока объект не достигнет координат.",Chinese:"將指定物件以指定的速度沿著指定座標的方向移動，直到抵達指定座標前移動都將持續",Kazakh:"Көрсетілген объектінің көрсетілген координаттар бағытында берілген жылдамдықпен орын ауыстыру процесін іске қосады. Объект координаттарға жетпейінше, орын ауыстыру жалғаса береді.",Korean:"지정된 객체가 선택한 좌표의 방향으로 지정된 속도로 이동을 시작합니다. 물체가 좌표에 도달할 때까지 이동은 계속됩니다.")]
        [Action(English:"move by coordinates",Russian:"перемещаться к координатам",Chinese:"移動至座標",Kazakh:"координаттарға қарай орын ауыстыру",Korean:"이(가) 다음 좌표로 이동")]
        [ArgsFormat(English:"{%} at a speed of {%} m/s",Russian:"{%} со скоростью {%} м/с",Chinese:"{%}以每秒{%}公尺的速度",Kazakh:"{%} м/с жылдамдығымен {%}",Korean:"{%} 속도:{%}m/s")]
        public IEnumerator MoveToCoordinatesAtSpeed([SourceTypeContainer(typeof(Vector3))] dynamic destination, [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            string methodName = nameof(MoveToCoordinatesAtSpeed);

            if (!ValidateMethodWithLog(this, destination, methodName, 0, out Vector3 convertedDestination))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            yield return MoveToPointAtSpeed(convertedDestination, convertedSpeed);
            OnToVectorMovementFinished?.Invoke(convertedDestination);
            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Starts the moving process of the specified object along the path at the specified speed. A route is a list of objects or coordinates in world space defined by vectors.",Russian:"Запускает процесс перемещения указанного объекта по маршруту с указанной скоростью. Маршрут представляет собой список объектов или координат в мировом пространстве, заданных векторами.",Chinese:"將指定物件以指定的速度沿著路徑移動，移動路徑是一列物件或一串以向量定義的空間座標。",Kazakh:"Көрсетілген объектінің көрсетілген жылдамдықпен маршрут бойынша орын ауыстыру процесін іске қосады. Маршрут әлемдік кеңістіктегі объектілердің немесе координаттардың векторлармен берілген тізімі болып табылды.",Korean:"지정된 객체가 선택한 속도로 경로를 따라 이동을 시작합니다. 경로는 벡터로 정의된 세계 공간의 객체 또는 좌표 목록입니다.")]
        [Action(English:"move along the path",Russian:"перемещаться по маршруту",Chinese:"沿路徑移動",Kazakh:"маршрут бойынша орын аустыру",Korean:"이(가) 다음 경로를 따라 이동")]
        [ArgsFormat(English:"{%} at a speed of {%} m/s",Russian:"{%} со скоростью {%} м/с",Chinese:"{%}以每秒{%}公尺的速度",Kazakh:"{%} м/с жылдамдығымен {%}",Korean:"{%} 속도:{%}m/s")]
        public IEnumerator MoveAlongThePath(dynamic path, [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            string methodName = nameof(MoveToCoordinatesAtSpeed);

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            if (path is not IEnumerable list)
            {
                var localizedError = LanguageManager.Instance.CurrentlyLoadedCulture.languageCode.ToLower() == "ru" 
                    ? $"Параметр 'путь' должен представлять собой список объектов или координат"
                    : $"The 'path' parameter must be a list of objects or coordinates";

                Debug.LogError(localizedError);
                yield break;    
            }
            
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            var currentWayPointIndex = 1;
            foreach (var pathItem in list)
            {
                var currentTarget = Vector3.zero;
                switch (pathItem)
                {
                    case Vector3 vector:
                        currentTarget = vector;
                        break;
                    case Wrapper wrapper:
                        var targetObject = wrapper.GetGameObject();

                        if (targetObject == null)
                        {
                            break;
                        }

                        currentTarget = targetObject.transform.position;

                        break;
                    default:
                        yield break;
                }

                yield return MoveToPointAtSpeed(currentTarget, convertedSpeed);
                
                if (!_currentState.IsMoving)
                {
                    _currentMotionRoutines.Remove(routineId);
                    OnAnyMovementFinished?.Invoke();
                    yield break;                    
                }
                
                OnPathTargetedMovementFinished?.Invoke(currentWayPointIndex, pathItem);
                currentWayPointIndex++;
            }

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
        }
        
        [LogicGroup(English: "Motion", Russian: "Перемещение", Chinese: "動作", Korean: "움직임")]
        [LogicTooltip(English: "Starts the moving process of the specified object along the path at the specified speed. A route is a list of objects or coordinates in world space defined by vectors.", Russian: "Запускает процесс перемещения указанного объекта по маршруту с указанной скоростью. Маршрут представляет собой список объектов или координат в мировом пространстве, заданных векторами.", Chinese: "將指定物件以指定的速度沿著路徑移動，移動路徑是一列物件或一串以向量定義的空間座標。", Korean: "지정된 객체가 선택한 속도로 경로를 따라 이동을 시작합니다. 경로는 벡터로 정의된 전역 공간의 객체 또는 좌표 목록입니다.")]
        [Action(English: "move along the path", Russian: "перемещаться по маршруту", Chinese: "沿路徑移動", Korean: "경로를 따라 이동")]
        [ArgsFormat(English: "{%} at a speed of {%} m/s", Russian: "{%} со скоростью {%} м/с", Chinese: "{%}以每秒{%}公尺的速度", Korean: "{%} m/s의 속도로 {%}")]
        [Obsolete]
        public IEnumerator MoveAlongPath(List<dynamic> path, [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            string methodName = nameof(MoveToCoordinatesAtSpeed);

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            var currentWayPointIndex = 1;
            foreach (var pathItem in path)
            {
                var currentTarget = Vector3.zero;
                switch (pathItem)
                {
                    case Vector3 vector:
                        currentTarget = vector;
                        break;
                    case Wrapper wrapper:
                        var targetObject = wrapper.GetGameObject();

                        if (targetObject == null)
                        {
                            break;
                        }

                        currentTarget = targetObject.transform.position;

                        break;
                    default:
                        yield break;
                }

                yield return MoveToPointAtSpeed(currentTarget, convertedSpeed);
                
                if (!_currentState.IsMoving)
                {
                    _currentMotionRoutines.Remove(routineId);
                    OnAnyMovementFinished?.Invoke();
                    yield break;                    
                }
                
                OnPathTargetedMovementFinished?.Invoke(currentWayPointIndex, pathItem);
                currentWayPointIndex++;
            }

            _currentMotionRoutines.Remove(routineId);

            OnAnyMovementFinished?.Invoke();
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Controls any movement. The paused movement can be resumed with the “Continue” block.",Russian:"Управляет любым перемещением. Приостановленное движение можно возобновить блоком “Продолжить”.",Chinese:"控制任何運動。 可以使用“繼續”塊恢復暫停的運動。",Kazakh:"Кез келген орын ауыстыруды басқарады. Тоқтатыла тұрған қозғалысты “Жалғастыру” блогымен қайта бастауға болады.",Korean:"모든 이동을 제어합니다. 일시 정지된 이동은 \" 계속 \" 블록을 통해 다시 시작할 수 있습니다.")]
        [ActionGroup("MotionControl")]
        [Action(English:"stop any movement",Russian:"завершить любое перемещение",Chinese:"停止任何移動",Kazakh:"кез келген орын ауыстыруды аяқтау",Korean:"의 모든 이동을 정지")]
        public void StopAnyMovement()
        {
            _currentState.IsMoving = false;
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Controls any movement. The paused movement can be resumed with the “Continue” block.",Russian:"Управляет любым перемещением. Приостановленное движение можно возобновить блоком “Продолжить”.",Chinese:"控制任何運動。 可以使用“繼續”塊恢復暫停的運動。",Kazakh:"Кез келген орын ауыстыруды басқарады. Тоқтатыла тұрған қозғалысты “Жалғастыру” блогымен қайта бастауға болады.",Korean:"모든 이동을 제어합니다. 일시 정지된 이동은 \" 계속 \" 블록을 통해 다시 시작할 수 있습니다.")]
        [ActionGroup("MotionControl")]
        [Action(English:"pause any movement",Russian:"приостановить любое перемещение",Chinese:"暫停任何移動",Kazakh:"кез келген орын ауыстыруды тоқтата тұру",Korean:"의 모든 이동을 일시정지")]
        public void PauseAnyMovement()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Controls any movement. The paused movement can be resumed with the “Continue” block.",Russian:"Управляет любым перемещением. Приостановленное движение можно возобновить блоком “Продолжить”.",Chinese:"控制任何運動。 可以使用“繼續”塊恢復暫停的運動。",Kazakh:"Кез келген орын ауыстыруды басқарады. Тоқтатыла тұрған қозғалысты “Жалғастыру” блогымен қайта бастауға болады.",Korean:"모든 이동을 제어합니다. 일시 정지된 이동은 \" 계속 \" 블록을 통해 다시 시작할 수 있습니다.")]
        [ActionGroup("MotionControl")]
        [Action(English:"continue any movement",Russian:"продолжить любое перемещение",Chinese:"繼續任何移動",Kazakh:"кез келген орын ауыстыруды жалғастыру",Korean:"의 모든 이동을 계속")]
        public void ContinueAnyMovement()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Checkers

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Returns true if the specified object is moving. Otherwise returns false.",Russian:"Возвращает “истину”, если указанный объект перемещается в данный момент. В противном случае возвращает “ложь”.",Chinese:"如果指定物件正在移動則回傳為真；反之，為假",Kazakh:"Егер көрсетілген объект осы сәтте орын ауыстырып жатса, “шындықты” қайтарады. Олай болмаған жағдайда “өтірікті” қайтарады.",Korean:"지정된 객체가 움직이면 참(true)을 반환함. 그렇지 않으면 거짓(false)을 반환함.")]
        [Checker(English:"is moving at the moment",Russian:"перемещается в данный момент",Chinese:"現在正在移動",Kazakh:"осы сәтте орын ауыстыруда",Korean:"이(가) 지금 움직이고 있다면")]
        public bool IsMovingNow()
        {
            return _currentMotionRoutines.Count > 0;
        }

        #endregion

        #region Variables

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Returns the position of the specified object along the selected axis in world coordinates.",Russian:"Возвращает позицию указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳指定物件的X座標",Kazakh:"Көрсетілген объектінің әлемдік координаттардағы позициясын таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 지정한 객체의 위치를 선택한 축을 따라 반환합니다")]
        [VariableGroup("GetPositionByAxis")]
        [Variable(English:"position on X axis",Russian:"позиция по оси X",Chinese:"X座標",Kazakh:"Х осі бойынша позиция",Korean:"의 X축 상 위치")]
        public float PositionX => transform.position.x;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Returns the position of the specified object along the selected axis in world coordinates.",Russian:"Возвращает позицию указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳指定物件的X座標",Kazakh:"Көрсетілген объектінің әлемдік координаттардағы позициясын таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 지정한 객체의 위치를 선택한 축을 따라 반환합니다")]
        [VariableGroup("GetPositionByAxis")]
        [Variable(English:"position on Y axis",Russian:"позиция по оси Y",Chinese:"Y座標",Kazakh:"Y осі бойынша позиция",Korean:"의 Y축 상 위치")]
        public float PositionY => transform.position.y;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Returns the position of the specified object along the selected axis in world coordinates.",Russian:"Возвращает позицию указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳指定物件的X座標",Kazakh:"Көрсетілген объектінің әлемдік координаттардағы позициясын таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 지정한 객체의 위치를 선택한 축을 따라 반환합니다")]
        [VariableGroup("GetPositionByAxis")]
        [Variable(English:"position on Z axis",Russian:"позиция по оси Z",Chinese:"Z座標",Kazakh:"Z осі бойынша позиция",Korean:"의 Z축 상 위치")]
        public float PositionZ => transform.position.z;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Returns the position of the specified object in world coordinates as a vector [x; y; z]",Russian:"Возвращает позицию указанного объекта в мировых координатах в виде вектора  [x;  y;  z]",Chinese:"以向量形式回傳目標物件在世界座標系中的位置",Kazakh:"Көрсетілген объектінің әлемдік координаттардағы позициясын [x; y; z] векторы түрінде қайтарады",Korean:"세계 좌표에서 지정된 객체의 위치를 벡터 [x; y; z]로 반환합니다.")]
        [Variable(English:"position",Russian:"позиция",Chinese:"座標",Kazakh:"позиция",Korean:"의 위치")]
        public Vector3 Position => transform.position;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Sets the side of the object that it will be facing in the direction of the move.",Russian:"Задает сторону объекта, которой он будет направлен в сторону перемещения. Значение для настройки перемещения объекта “вперёд лицом”: (x: 0; y: 0; z: 1)",Chinese:"設定物件面對移動方向的面",Kazakh:"Объектінің орын ауыстыру бағытына қарайтын жағын белгілейді.",Korean:"객체가 이동 방향을 마주보도록 객체의 측면을 설정합니다.")]
        [Variable(English:"front side while moving",Russian:"лицевая сторона при перемещении",Chinese:"正面向前移動",Kazakh:"орын ауыстыру кезіндегі беткі жақ",Korean:"이(가) 이동 시 바라보는 방향")]
        [SourceTypeContainer(typeof(Vector3))]
        public dynamic MovementFaceDirection
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(MovementFaceDirection), out Vector3 convertedValue))
                {
                    return;
                }

                transform.forward = convertedValue.normalized;
            }
        }

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Sets the minimum distance between the specified and target objects for movement to be considered complete. The block calculates the distance between the centers of the objects, so using a 0 value is not recommended.",Russian:"Задает минимальное расстояние между заданным и целевым объектами, чтобы движение к нему считалось завершенным. Вычисляется расстояние между центрами объектов, поэтому использование значения 0 не рекомендуется.",Chinese:"設定指定對象和目標對象之間的最小距離，使移動被視為完成。此方塊計算對象的中心點之間的距離，因此並不建議將數值設為0",Kazakh:"Берілген және мақсатты объектілер арасындағы ең аз қашықтықты оған қарай қозғалыс аяқталды деп саналуы үшін белгілейді. Объектілердің центрлері арасындағы ара қашықтық есептеліп шығарылады, сондықтан 0 мәнін пайдалану ұсынылмайды.",Korean:"이동이 완료된 것으로 간주되는 지정된 객체와 목표 객체 사이의 최소 거리를 설정합니다. 블록은 객체들의 중심 간 거리를 계산하므로 0의 값을 사용하는 것은 권장되지 않습니다.")]
        [Variable(
English:"minimum stop distance in front of target object",Russian:"минимальное расстояние остановки перед целевым объектом",Chinese:"至目標物件最小停止距離",Kazakh:"мақсатты объектінің алдындағы тоқтаудың ең аз ара қашықтығы",Korean:"의 목표 객체 앞 최소 정지 거리")]
        [ArgsFormat(English:"{%} m.",Russian:"{%} м.",Chinese:"{%}米",Kazakh:"{%} м.",Korean:"{%} m")]
        [SourceTypeContainer(typeof(float))]
        public dynamic MinimumTargetStopDistance
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(MovementFaceDirection), out float convertedValue))
                {
                    return;
                }

                _minimumTargetStopDistance = convertedValue;
            }
        }

        #endregion

        #region Functions

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Returns the straight-line distance from the specified object to the second object. The distance is returned in meters as a real number.",Russian:"Возвращает расстояние по прямой от указанного объекта до второго объекта. Расстояние возвращается в метрах в виде вещественного числа.",Chinese:"回傳指定物件到另一個物件的直線距離，回傳時將以公尺為單位",Kazakh:"Көрсетілген объектіден екінші объектіге дейінгі түзу бойымен ара қашықтықты қайтарады. Ара қашықтық заттық сан түрінде метрмен қайтарылады.",Korean:"지정된 객체에서 두 번째 객체까지의 직선 거리를 반환합니다. 거리는 미터 단위의 실수(a real number)로 반환됩니다.")]
        [Function(English:"distance to object",Russian:"расстояние до объекта",Chinese:"到物件之間的距離",Kazakh:"объектіге дейінгі ара қашықтық",Korean:"과(와) 다음 객체까지의 거리")]
        public float GetDistanceTo([SourceTypeContainer(typeof(Wrapper))] dynamic target)
        {
            if (!ValidateMethodWithLog(this, target, nameof(GetDistanceTo), 0, out Wrapper convertedTarget))
            {
                return 0f;
            }

            var targetObject = convertedTarget.GetGameObject();

            return !targetObject ? 0 : Vector3.Distance(transform.position, targetObject.transform.position);
        }
        
        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(
English:"Returns the straight-line distance from the specified object to world coordinates specified with a vector. The distance is returned in meters as a real number.",Russian:"Возвращает расстояние по прямой от указанного объекта до мировых координат, указанных с помощью вектора. Расстояние возвращается в метрах в виде вещественного числа.",Chinese:"回傳指定物件到另一個世界座標的直線距離，回傳時將以公尺為單位",Kazakh:"Көрсетілген объектіден вектор көмегімен көрсетілген әлемдік координаттарға дейінгі түзу бойымен ара қашықтықты қайтарады. Ара қашықтық заттық сан түрінде метрмен қайтарылады.",Korean:"지정된 객체에서 벡터로 지정된 세계 좌표까지의 직선 거리를 반환합니다. 거리는 미터 단위의 실수로 반환됩니다")]
        [Function(English:"distance to coordinates",Russian:"расстояние до координат",Chinese:"到座標之間的距離",Kazakh:"координаттарға дейінгі ара қашықтық",Korean:"과(와) 다음 좌표까지의 거리")]
        public float GetDistanceToVector([SourceTypeContainer(typeof(Vector3))] dynamic target)
        {
            if (!ValidateMethodWithLog(this, target, nameof(GetDistanceToVector), 0, out Vector3 convertedTarget))
            {
                return 0f;
            }

            return Vector3.Distance(transform.position, convertedTarget);
        }
       
        #endregion

        #region Events

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"The event is triggered when the specified object completes any movement. The movement is considered completed if the object has reached the target position or if the movement has been stopped by the corresponding block. The object for which the event was triggered is passed to the parameter.",Russian:"Событие срабатывает, когда указанный объект завершает любое перемещение. Перемещение считается завершенным, если объект достиг целевой позиции, или если перемещение было остановлено соответствующим блоком. В параметр передается объект, для которого сработало событие.",Chinese:"此事件會在指定物件完成任何移動時觸發，當物件抵達目標位置或被相應的方塊停止時會被視為完成移動。觸發事件的物件會被傳送給參數。",Kazakh:"Көрсетілген объект кез келген орын ауыстыруды аяқтаған кезде оқиға іске қосылады. Егер объект мақсатты позицияға жетсе немесе орын ауыстыруды тиісті блок тоқтатса, орын ауыстыру аяқталды деп саналады. Параметрге сол үшін оқиға іске қосылған объект беріледі.",Korean:"지정된 객체가 이동을 완료하면 이벤트가 작동(trigger) 됩니다. 물체가 목표 위치에 도달했거나 해당 블록에 의해 이동이 중지된 경우 이동이 완료된 것으로 간주됩니다. 이벤트가 작동한 객체는 매개변수에 전달됩니다.")]
        [LogicEvent(English:"completed any movement",Russian:"завершил любое перемещение",Chinese:"完成任何移動",Kazakh:"кез келген орын ауыстыруды аяқтады",Korean:"모든 이동이 완료되었을 때")]
        public event CommonMovementHandler OnAnyMovementFinished;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"The event is triggered when the specified object completes moving to the target object. The object for which the event was triggered (moving object) and the object to which the movement was completed (target object) are passed in parameters.",Russian:"Событие срабатывает, когда указанный объект завершает перемещение к целевому объекту. В параметры передается объект, у которого сработало событие (перемещающийся объект), а также объект, к которому было завершено перемещение (целевой объект).",Chinese:"此事件會在指定物件移動至目標物件時觸發，觸發事件的物件 (移動中的物件) 和目標物件會被傳送給參數",Kazakh:"Көрсетілген объект мақсатты объектіге орын ауыстыруды аяқтаған кезде, оқиға іске қосылады. Параметрге оқиғасы іске қосылған объект (орын ауыстырып келе жатқан объект), сондай-ақ оған қарай орын ауыстыру аяқталған объект (мақсатты объект) беріледі.",Korean:"지정된 객체가 목표 객체로 이동을 완료하면 이벤트가 작동됩니다. 이벤트가 발생한 객체(움직이는 객체)와 이동이 완료된 객체(목표 객체)를 매개변수로 전달합니다.")]
        [LogicEvent(English:"completed moving to target object",Russian:"завершил перемещение к целевому объекту",Chinese:"完成移動到目標對象",Kazakh:"мақсатты координаттарға қарай орын ауыстыруды аяқтады",Korean:"목표 객체로 이동이 완료되었을 때")]
        public event ToWrapperMovementHandler OnToWrapperMovementFinished;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"The event is triggered when the specified object completes moving to the target coordinates. The object for which the event was triggered (moving object) and the coordinates as vector to which the movement was completed (target coordinates) are passed in parameters.",Russian:"Событие срабатывает, когда указанный объект завершает перемещение к целевым координатам. В параметры передается объект, у которого сработало событие (перемещающийся объект), а также координаты, в виде вектора, к которым было завершено перемещение (целевые координаты).",Chinese:"此事件會在指定物件移動至目標座標時觸發。觸發事件的物件 (移動中的物件) 和目標位置的向量座標會被傳送給參數。",Kazakh:"Көрсетілген объект мақсатты координаттарға орын ауыстыруды аяқтаған кезде, оқиға іске қосылады. Параметрге оқиғасы іске қосылған объект (орын ауыстырып келе жатқан объект), сондай-ақ оларға қарай орын ауыстыру аяқталған координаттар (мақсатты координаттар) вектор түрінде беріледі.",Korean:"지정된 객체가 목표 좌표로 이동을 완료하면 이벤트가 작동됩니다. 이벤트가 발생한 객체(움직이는 객체)와 이동이 완료된 벡터 좌표(목표 좌표)를 매개변수로 전달합니다.")]
        [LogicEvent(English:"completed moving to",Russian:"завершил движение к целевым координатам",Chinese:"完成移動至…",Kazakh:"мақсатты координаттарға қарай қозғалысты аяқтады",Korean:"이동이 완료되었을 때")]
        public event ToVectorMovementHandler OnToVectorMovementFinished;

        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"The event is triggered when the specified object moving along the path reaches a waypoint on the path. The parameters pass the object for which the event was triggered, the number of the waypoint, and the point reached.",Russian:"Событие срабатывает, когда указанный объект, двигающийся по маршруту, достигает очередную точки маршрута. В параметры передается объект, у которого сработало событие, номер точки в маршруте, а также достигнутая точка.",Chinese:"此事件會在指定物件沿著路徑前進抵達導航點時觸發。傳送的參數包含觸發事件的物件、導航點的數量和抵達的導航點。",Kazakh:"Маршрутпен қозғалып келе жатқан көрсетілген объект маршруттың кезекті нүктесіне жеткен кезде, оқиға іске қосылады. Параметрге оқиғасы іске қосылған объект, маршруттағы нүктенің нөмірі, сондай-ақ жеткен нүкте беріледі.",Korean:"경로를 따라 이동하는 지정된 객체가 경로 상 웨이포인트에 도달하면 이벤트가 작동됩니다. 이벤트가 작동한 객체, 웨이포인트 수, 도달한 지점을 매개변수로 전달합니다.")]
        [LogicEvent(English:"reached a waypoint",Russian:"достиг точки маршрута",Chinese:"抵達導航點",Kazakh:"маршрут нүктесіне жетті",Korean:"웨이포인트에 도달했을 때")]
        public event WayPointMovementHandler OnPathTargetedMovementFinished;

        #endregion

        #region PrivateHelpers
        
        private IEnumerator MoveToPointAtSpeed(Vector3 destination, float speed)
        {
            var routineId = _routineId++;
            _currentMotionRoutines.Add(routineId);

            _currentState.IsMoving = true;

            var distanceToObject = Vector3.Distance(destination, transform.position);
            var velocity = (destination - transform.position) / distanceToObject * (speed * Time.deltaTime);

            while (distanceToObject > velocity.magnitude)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                if (!_currentState.IsMoving)
                {
                    _currentMotionRoutines.Remove(routineId);
                    yield break;
                }

                var thisPosition = transform.position;

                distanceToObject = Vector3.Distance(destination, thisPosition);
                velocity = (destination - thisPosition) / distanceToObject * (speed * Time.deltaTime);

                if (distanceToObject > float.Epsilon)
                {
                    SetPosition(transform.position + velocity);
                }

                yield return WaitForEndOfFrame;
            }

            SetPosition(destination);

            _currentMotionRoutines.Remove(routineId);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion
        
        [LogicGroup(English:"Motion",Russian:"Перемещение",Chinese:"動作",Kazakh:"Орын ауыстыру",Korean:"움직임")]
        [LogicTooltip(English:"Motion Axis",Russian:"Ось перемещения",Chinese:"移動軸",Kazakh:"Орын ауыстыру осі",Korean:"이동축")]
        public enum Axis
        {
            [Item(English:"X",Russian:"X",Chinese:"X",Kazakh:"X",Korean:"X")] X,
            [Item(English:"Y",Russian:"Y",Chinese:"Y",Kazakh:"Y",Korean:"Y")] Y,
            [Item(English:"Z",Russian:"Z",Chinese:"Z",Kazakh:"Z",Korean:"Z")] Z,
        }
        
        protected Vector3 EnumToVector(Axis axisDirection)
        {
            return axisDirection switch
            {
                Axis.X => transform.right,
                Axis.Y => transform.up,
                Axis.Z => transform.forward,
                _ => Vector3.zero
            };
        }
    }
}

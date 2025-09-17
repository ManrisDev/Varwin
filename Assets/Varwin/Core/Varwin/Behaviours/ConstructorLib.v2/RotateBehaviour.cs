using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Varwin.Public;
using static Varwin.TypeValidationUtils;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class RotateBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool IsDisabledBehaviour(GameObject targetGameObject)
        {
            return IsDisabledBehaviour(targetGameObject, BehaviourType.Rotate);
        }
    }
    
    [VarwinComponent(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
    public class RotateBehaviour : ConstructorVarwinBehaviour
    {
        private readonly List<int> _currentRotateRoutines = new();
        private int _routineId;

        private readonly BehaviourState _currentState = new();

        public delegate void CommonRotationHandler();

        public delegate void ToWrapperRotationHandler([Parameter(English:"target object",Russian:"целевой объект",Chinese:"目標物件",Kazakh:"мақсатты объект",Korean:"목표 객체")] Wrapper target);

        public delegate void ToVectorRotationHandler([Parameter(English:"target rotation",Russian:"целевой поворот",Chinese:"目標旋轉角度",Kazakh:"мақсатты бұрылыс",Korean:"목표 회전")] Vector3 target);

        #region Actions

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Instantly sets the rotation of the object in degrees along the three axes. The rotation is counted relative to the world coordinates.",Russian:"Мгновенно задает поворот указанного объекта в градусах по трем осям. Поворот считается относительно мировых координат.",Chinese:"設定物件在三個軸向的旋轉角度。旋轉的角度與世界座標相關",Kazakh:"Көрсетілген объектінің үш ось бойынша бұрылуын градуспен лезде белгілейді. Бұрылыс әлемдік координаттарға қатысты есептеледі.",Korean:"세 개의 축을 따라 객체의 회전을 각도 단위로 즉시 설정합니다. 회전은 세계 좌표를 기준으로 계산됩니다.")]
        [Action(English:"set rotation",Russian:"задать поворот",Chinese:"設定旋轉角度",Kazakh:"бұрылысты белгілеу",Korean:"을(를) 다음 각도로 회전 설정")]
        public void SetRotation([SourceTypeContainer(typeof(Vector3))] dynamic eulerAngles)
        {
            if (!ValidateMethodWithLog(this, eulerAngles, nameof(SetRotation), 0, out Vector3 convertedEulerAngles))
            {
                return;
            }

            SetTargetRotation(Quaternion.Euler(convertedEulerAngles));
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Instantly rotates the object by the specified angle along the selected axis.",Russian:"Мгновенно поворачивает объект на указанный угол по выбранной оси.",Chinese:"瞬間以指定角度沿著指定軸向旋轉物件",Kazakh:"Объектіні таңдалған ось бойынша көрсетілген бұрышқа лезде бұрады.",Korean:"선택한 축을 따라 지정된 각도만큼 객체를 즉시 회전합니다")]
        [Action(English:"instantly rotate",Russian:"мгновенно повернуться на",Chinese:"旋轉",Kazakh:"-ға лезде бұрылу",Korean:"을(를) 즉시 다음 기준으로 회전. 각도:")]
        [ArgsFormat(English:"{%} degrees on axis {%}",Russian:"{%} градусов по оси {%}",Chinese:"{%}度，於{%}軸",Kazakh:"{%} осі бойынша {%} градус",Korean:"{%}˚ 축:{%}")]
        public void RotateByDegreesOnAxis([SourceTypeContainer(typeof(float))] dynamic angle, [SourceTypeContainer(typeof(Axis))] dynamic axis)
        {
            var methodName = nameof(RotateByDegreesOnAxis);
            if (!ValidateMethodWithLog(this, angle, methodName, 0, out float convertedAngle))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, axis, methodName, 1, out Axis convertedAxis))
            {
                return;
            }

            SetTargetRotation(Quaternion.AngleAxis(convertedAngle, transform.rotation * EnumToVector(convertedAxis)) * transform.rotation);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение", Chinese: "旋轉", Korean: "회전")]
        [LogicTooltip(English: "Instantly rotates the object to the selected object.", Russian: "Мгновенно поворачивает объект к другому выбранному объекту.", Chinese: "瞬間將物件轉向選擇的物件", Korean: "선택한 객체로 즉시 회전합니다.")]
        [Action(English: "instantly rotate to object", Russian: "мгновенно повернуться к объекту", Chinese: "瞬間轉向物件", Korean: "객체로 즉시 회전합니다.")]
        [Obsolete]
        public void RotateToObject([SourceTypeContainer(typeof(Wrapper))] dynamic targetObject)
        {
            if (!ValidateMethodWithLog(this, targetObject, nameof(RotateToObject), 0, out Wrapper convertedWrapper))
            {
                return;
            }

            var destinationTransform = convertedWrapper.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            transform.LookAt(destinationTransform);
        }
        
        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Instantly rotates the object to the selected object.",Russian:"Мгновенно поворачивает объект к другому выбранному объекту.",Chinese:"瞬間將物件轉向選擇的物件",Kazakh:"Объектіні екінші таңдалған объектіге лезде бұрады.",Korean:"선택한 객체로 즉시 객체를 회전합니다")]
        [Action(English:"instantly rotate to object",Russian:"мгновенно повернуться к объекту",Chinese:"瞬間轉向物件",Kazakh:"объектіге лезде бұрылу",Korean:"을(를) 즉시 다음 객체 기준으로 회전")]
        public void RotateToObjectAtAxis(
            [SourceTypeContainer(typeof(Wrapper))] dynamic targetObject,
            [SourceTypeContainer(typeof(RotationToObjectAroundAxis))] dynamic axis)
        {
            var methodName = nameof(RotateToObjectAtAxis);

            if (!ValidateMethodWithLog(this, targetObject, methodName, 0, out Wrapper convertedWrapper))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, axis, methodName, 1, out RotationToObjectAroundAxis convertedAxis))
            {
                return;
            }
            
            var destinationTransform = convertedWrapper.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            transform.rotation = GetRotationToObject(transform, destinationTransform, convertedAxis);
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Instantly rotates the object in the same way as the selected object.",Russian:"Мгновенно задает объекту параметры вращения другого выбранного объекта.",Chinese:"瞬間以選擇物件的旋轉角度旋轉物件",Kazakh:"Таңдалған екінші объектінің айналу параметрлерін объектіге лезде белгілейді.",Korean:"선택한 객체와 동일한 축을 기준으로 객체를 즉시 회전합니다")]
        [Action(English:"Instantly rotate the same way as object",Russian:"Мгновенно повернуться также, как объект",Chinese:"瞬間旋轉相同的角度",Kazakh:"Лезде объект сияқты бұрылу",Korean:"을(를) 다음 객체와 동일한 방식으로 회전")]
        public void RotateAsObject([SourceTypeContainer(typeof(Wrapper))] dynamic targetObject)
        {
            if (!ValidateMethodWithLog(this, targetObject, nameof(RotateAsObject), 0, out Wrapper convertedWrapper))
            {
                return;
            }

            var destinationTransform = convertedWrapper.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                return;
            }

            SetTargetRotation(destinationTransform.rotation);
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Starts rotation of the specified object around the selected local axis with the specified speed. The rotation continues until it is stopped by the rotation stop block. Use negative speed values to change the rotation direction.",Russian:"Запускает вращение указанного объекта вокруг выбранной локальной оси с заданной скоростью. Вращение происходит, пока оно не будет остановлено блоком остановки вращения. Чтобы изменить направление вращения, используйте отрицательные значения скорости.",Chinese:"以指定速度沿著選擇的本地座標軸旋轉物件，直到被停止旋轉方塊停下來以前旋轉都將持續。使用負的速度數值來改變旋轉的方向",Kazakh:"Көрсетілген объектінің таңдалған локалды осьті берілген жылдамдықпен айналуын іске қосады. Айналу оны айналуды тоқтату блогы тоқтатқанша жалғаса береді. Айналу бағытын өзгерту үшін жылдамдықтың теріс мәндерін пайдаланыңыз.",Korean:"지정된 속도로 선택된 로컬 축을 중심으로 지정된 객체의 회전을 시작합니다. 회전 정지 블록에 의해 정지될 때까지 회전은 계속됩니다. 회전 방향을 변경하려면 속도 값을 음수로 조정하세요")]
        [Action(English:"rotate around the axis",Russian:"вращаться вокруг оси",Chinese:"沿著軸旋轉",Kazakh:"осьті айналу",Korean:"이(가) 다음 축을 중심으로 회전")]
        [ArgsFormat(English:"{%} at a speed of {%} degrees/s.",Russian:"{%} со скоростью {%} градусов/сек.",Chinese:"{%}，並以每秒{%}度的速度",Kazakh:"{%} градус/сек. жылдамдығымен {%}",Korean:"{%} 회전속도: {%} rad/s")]
        public void RotateAroundAxisWithSpeed([SourceTypeContainer(typeof(Axis))] dynamic axis, [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(RotateAroundAxisWithSpeed);

            if (!ValidateMethodWithLog(this, axis, methodName, 0, out Axis convertedAxis))
            {
                return;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                return;
            }

            StartCoroutine(RotateAroundAxisWithSpeedCoroutine(convertedAxis, convertedSpeed));
        }

        public IEnumerator RotateAroundAxisWithSpeedCoroutine(Axis axis, float speed)
        {
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            while (_currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                transform.RotateAround(transform.position, EnumToVector(axis), speed*Time.deltaTime);
                
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Starts rotation of the object around the selected axis for the specified time with the specified speed. Use negative speed values to change the rotation direction.",Russian:"Запускает вращение объекта вокруг выбранной оси в течение указанного времени с заданной скоростью. Для изменения направления вращения используйте отрицательные значения скорости.",Chinese:"以指定軸向、時間和速度旋轉物件。使用負的速度數值來改變旋轉的方向",Kazakh:"Объектінің таңдалған осьті көрсетілген уақыт ішінде берілген жылдамдықпен айналуын іске қосады. Айналу бағытын өзгерту үшін жылдамдықтың теріс мәндерін пайдаланыңыз.",Korean:"지정된 시간 동안 지정된 속도로 선택된 축을 중심으로 객체의 회전을 시작합니다. 회전 방향을 변경하려면 음수 값을 사용하세요")]
        [Action(English:"rotate around the axis",Russian:"вращаться вокруг оси",Chinese:"沿著軸旋轉",Kazakh:"осьті айналу",Korean:"이(가) 다음 축을 중심으로 회전")]
        [ArgsFormat(
English:"{%} for {%} s. at a speed of {%} degrees/s.",Russian:"{%} в течение {%} сек. со скоростью {%} градусов/сек.",Chinese:"{%}，持續{%}秒，並以每秒{%}度的速度",Kazakh:"{%} градус/сек. жылдамдығымен {%} сек. ішінде {%}",Korean:"{%} 시간:{%}s 회전속도:{%} rad/s")]
        public IEnumerator RotateAroundAxisForDurationWithSpeed(
            [SourceTypeContainer(typeof(Axis))] dynamic axis,
            [SourceTypeContainer(typeof(float))] dynamic duration,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(RotateAroundAxisForDurationWithSpeed);

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
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var travelTime = 0f;

            while (travelTime <= convertedDuration && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                transform.RotateAround(transform.position, EnumToVector(convertedAxis), convertedSpeed * Time.deltaTime);

                travelTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Starts rotation of the object around the selected axis of another object with a specified speed.",Russian:"Запускает вращение объекта вокруг выбранной оси другого объекта с заданной скоростью.",Chinese:"以指定速度啟動物件繞另一物件的選取軸旋轉。",Kazakh:"Объектінің берілген жылдамдықпен екінші объектінің таңдалған осін айналуын іске қосады.",Korean:"지정된 속도로 다른 객체의 선택된 축을 중심으로 객체의 회전을 시작합니다")]
        [Action(English:"rotate around the axis",Russian:"вращаться вокруг оси",Chinese:"沿著軸旋轉",Kazakh:"осьті айналу",Korean:"이(가) 다음 객체의 축을 중심으로 회전. 축:")]
        [ArgsFormat(English:"{%} of the object {%} at a speed of {%} degrees/s.",Russian:"{%} объекта {%} со скоростью {%} градусов/сек.",Chinese:"{%}在物件{%}上，並以每秒{%}度的速度",Kazakh:"{%} градус/сек. жылдамдығымен {%} объектісінің {%}",Korean:"{%} 객체:{%} 회전속도:{%}rad/s")]
        public void RotateAroundAnotherObjectAxisWithSpeed(
            [SourceTypeContainer(typeof(Axis))] dynamic axis,
            [SourceTypeContainer(typeof(Wrapper))] dynamic targetObject,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(RotateAroundAnotherObjectAxisWithSpeed);

            if (!ValidateMethodWithLog(this, axis, methodName, 0, out Axis convertedAxis))
            {
                return;
            }
            
            if (!ValidateMethodWithLog(this, targetObject, methodName, 1, out Wrapper convertedTargetObject))
            {
                return;
            }
            
            if (!ValidateMethodWithLog(this, speed, methodName, 2, out float convertedSpeed))
            {
                return;
            }

            StartCoroutine(RotateAroundAnotherObjectAxisWithSpeedCoroutine(convertedAxis, convertedTargetObject, convertedSpeed));
        }

        private IEnumerator RotateAroundAnotherObjectAxisWithSpeedCoroutine(Axis axis, Wrapper targetObject, float speed)
        {
            var destinationTransform = targetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }
            
            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            while (_currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                transform.RotateAround(destinationTransform.position, EnumToVector(axis), speed*Time.deltaTime);
                
                yield return WaitForEndOfFrame;
            }

            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
            OnToWrapperRotationFinished?.Invoke(targetObject);
        }

        [LogicGroup(English: "Rotation", Russian: "Вращение", Chinese: "旋轉", Korean: "회전")]
        [LogicTooltip(English: "Starts rotation of the object to another selected object at a specified speed.", Russian: "Запускает вращение объекта к другому выбранному объекту с указанной скоростью.", Chinese: "以指定速度將物件面向另一個物件", Korean: "지정된 속도로 객체의 회전을 다른 선택한 객체로 시작합니다.")]
        [Action(English: "rotate to object", Russian: "повернуться к объекту", Chinese: "轉向物件", Korean: "객체로 회전")]
        [ArgsFormat(English: "{%} at a speed of {%} degrees/s.", Russian: "{%} со скоростью {%} градусов/сек.", Chinese: "{%}，並以每秒{%}度的速度", Korean: "초당 {%}도의 속도로 {%}")]
        [Obsolete]
        public IEnumerator LookAtObjectWithSpeed(
            [SourceTypeContainer(typeof(Wrapper))] dynamic targetObject,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(LookAtObjectWithSpeed);

            if (!ValidateMethodWithLog(this, targetObject, methodName, 0, out Wrapper convertedTargetObject))
            {
                yield break;
            }
            
            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedAxis))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var destinationTransform = convertedTargetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            var thisTransform = transform;

            var startRotation = thisTransform.rotation;
            var targetRotation = Quaternion.LookRotation(destinationTransform.position - transform.position);

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, convertedAxis);

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime < rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetTargetRotation(Quaternion.RotateTowards(transform.rotation, targetRotation,  convertedAxis * Time.deltaTime));
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            SetTargetRotation(targetRotation);
            
            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
            OnToWrapperRotationFinished?.Invoke(convertedTargetObject);
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Starts rotation of the object to another selected object at a specified speed.",Russian:"Запускает вращение объекта к другому выбранному объекту с указанной скоростью.",Chinese:"以指定速度將物件面向另一個物件",Kazakh:"Объектінің екінші таңдалған объектіге көрсетілген жылдамдықпен айналуын іске қосады.",Korean:"지정된 속도로 다른 선택된 객체를 향해 객체의 회전을 시작합니다")]
        [Action(English:"rotate to object",Russian:"повернуться к объекту",Chinese:"轉向物件",Kazakh:"объектіге бұрылу",Korean:"이(가) 다음 객체로 회전")]
        [ArgsFormat(English:"{%} at a speed of {%} degrees/s {%}",Russian:"{%} со скоростью {%} градусов/с {%}",Chinese:"{%}，並以每秒{%}度的速度 {%}",Kazakh:"{%} жылдамдығы: {%} градус/с {%}",Korean:"{%} 회전속도:{%}rad/s 축:{%}")]
        public IEnumerator LookAtObjectWithSpeedAtAxis(
            [SourceTypeContainer(typeof(Wrapper))] dynamic targetObject, 
            [SourceTypeContainer(typeof(float))] dynamic speed,
            [SourceTypeContainer(typeof(RotationToObjectAroundAxis))] dynamic axis)
        {
            var methodName = nameof(LookAtObjectWithSpeedAtAxis);

            if (!ValidateMethodWithLog(this, targetObject, methodName, 0, out Wrapper convertedTargetObject))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, axis, methodName, 2, out RotationToObjectAroundAxis convertedAxis))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var destinationTransform = convertedTargetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            var startRotation = transform.rotation;
            var targetRotation = GetRotationToObject(transform, destinationTransform, convertedAxis);

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, convertedSpeed);

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime < rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetTargetRotation(Quaternion.RotateTowards(transform.rotation, targetRotation,  convertedSpeed * Time.deltaTime));
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }

            SetTargetRotation(targetRotation);
            
            _currentRotateRoutines.Remove(routineId);
            OnCommonRotationFinished?.Invoke();
            OnToWrapperRotationFinished?.Invoke(convertedTargetObject);
        }
        
        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Starts rotation of the object according to the parameters of the other selected object with the specified speed.",Russian:"Запускает вращение объекта в соответствии с параметрами другого выбранного объекта с указанной скоростью.",Chinese:"根據其他物件的參數設定角度並以指定的速度開始旋轉物件",Kazakh:"Объектінің көрсетілген жылдамдықпен екінші таңдалған объектінің параметрлеріне сәйкес айналуын іске қосады.",Korean:"선택한 다른 객체의 매개변수에 따라 지정된 속도로 객체의 회전을 시작합니다.")]
        [Action(English:"rotate the same way as object",Russian:"повернуться так же, как объект",Chinese:"以相同方式旋轉物件",Kazakh:"объект сияқты бұрылу",Korean:"이(가) 다음 객체와 동일 방식으로 회전")]
        [ArgsFormat(English:"{%} at a speed of {%} degrees/s.",Russian:"{%} со скоростью {%} градусов/сек.",Chinese:"{%}，並以每秒{%}度的速度",Kazakh:"{%} градус/сек. жылдамдығымен {%}",Korean:"{%} 회전속도:{%}rad/s")]
        public IEnumerator RotateAsObjectWithSpeed(
            [SourceTypeContainer(typeof(Wrapper))] dynamic targetObject,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(RotateAsObjectWithSpeed);

            if (!ValidateMethodWithLog(this, targetObject, methodName, 0, out Wrapper convertedTargetObject))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var destinationTransform = convertedTargetObject.GetGameObject()?.transform;

            if (destinationTransform == null)
            {
                yield break;
            }

            var thisTransform = transform;

            var startRotation = thisTransform.rotation;
            var targetRotation = destinationTransform.rotation;

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, convertedSpeed);

            if (rotationMaxTime == 0)
            {
                yield break;
            }

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime <= rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetTargetRotation(Quaternion.Lerp(startRotation, targetRotation, rotationCurrentTime / rotationMaxTime));
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            SetTargetRotation(targetRotation);
            _currentRotateRoutines.Remove(routineId);

            OnCommonRotationFinished?.Invoke();
            OnAsWrapperRotationFinished?.Invoke(convertedTargetObject);
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Starts rotation of the object to an angle in world axes defined by a vector with angles on each of the axes [0...360]. The rotation will be performed along the shortest path.",Russian:"Запускает вращение объекта к углу в мировых координатах, заданного вектором с углами по каждой из осей [0...360]. Поворот будет производится по наименьшему пути.",Chinese:"以世界座標的角度旋轉物件，由一個向量來定義每個軸各自的角度 [0...360]。旋轉時將以最短路徑呈現。",Kazakh:"Бұрыштары әрбір ось бойынша [0...360] векторымен берілген әлемдік координаттарда бұрышқа объектінің айналуын іске қосады. Бұрылыс ең аз жолмен жүзеге асырылады.",Korean:"해당 객체의 세계 좌표 축에서 각 축에 대한 각도[0...360]가 정의된 벡터에 따라 특정 각도로 회전을 시작합니다. 회전은 최단 경로를 따라 일어납니다.")]
        [Action(English:"rotate to the angle",Russian:"повернуться к углу",Chinese:"旋轉指定角度",Kazakh:"бұрышқа бұрылу",Korean:"이(가) 다음 각도로 회전")]
        [ArgsFormat(English:"{%} at a speed of {%} degrees/s",Russian:"{%} со скоростью {%} градусов/с",Chinese:"{%}，並以每秒{%}度的速度",Kazakh:"{%} градус/с жылдамдығымен {%}",Korean:"{%} 회전속도:{%}rad/s")]
        public IEnumerator RotateToVectorWithSpeed(
            [SourceTypeContainer(typeof(Vector3))] dynamic eulerAngles,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(RotateToVectorWithSpeed);

            if (!ValidateMethodWithLog(this, eulerAngles, methodName, 0, out Vector3 convertedEulerAngles))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, speed, methodName, 1, out float convertedSpeed))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var thisTransform = transform;

            var startRotation = thisTransform.rotation;
            var targetRotation = Quaternion.Euler(convertedEulerAngles);

            var rotationMaxTime = GetRotationFromToTime(startRotation, targetRotation, convertedSpeed);

            if (Mathf.Abs(rotationMaxTime) < Mathf.Epsilon)
            {
                yield break;
            }

            var rotationCurrentTime = 0f;

            while (rotationCurrentTime <= rotationMaxTime && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetTargetRotation(Quaternion.Lerp(startRotation, targetRotation, Mathf.Clamp01(rotationCurrentTime / rotationMaxTime)));
                rotationCurrentTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            SetTargetRotation(targetRotation);

            _currentRotateRoutines.Remove(routineId);

            OnCommonRotationFinished?.Invoke();
            OnToVectorRotationFinished?.Invoke(convertedEulerAngles);
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Starts rotation of the object around the selected local axis with a specified speed. Use negative speed values to change the rotation direction.",Russian:"Запускает вращение объекта вокруг выбранной локальной оси с заданной скоростью. Для изменения направления вращения используйте отрицательные значения скорости.",Chinese:"以指定速度沿著選擇的本地座標軸旋轉物件。使用負的速度數值可以改變旋轉的方向",Kazakh:"Объектінің таңдалған локалды осьті берілген жылдамдықпен айналуын іске қосады. Айналу бағытын өзгерту үшін жылдамдықтың теріс мәндерін пайдаланыңыз.",Korean:"지정된 속도로 선택한 로컬 축을 중심으로 객체의 회전을 시작합니다. 회전 방향을 변경하려면 음수 값을 사용하십시오")]
        [Action(English:"rotate around the axis",Russian:"повернуться вокруг оси",Chinese:"沿著軸旋轉",Kazakh:"осьті айнала бұрылу",Korean:"이(가) 다음 축을 중심으로 회전")]
        [ArgsFormat(English:"{%} by {%} degrees at a speed of {%} degrees/s.",Russian:"{%} на {%} градусов со скоростью {%} градусов/сек.",Chinese:"{%}軸，旋轉{%}度，並以每秒{%}度的速度",Kazakh:"{%} градус/сек. жылдамдығымен {%} градусқа {%}",Korean:"{%} 각도: {%} 회전속도:{%} rad/s")]
        public IEnumerator RotateAroundAxisByAngleWithSpeed(
            [SourceTypeContainer(typeof(Axis))] dynamic axis,
            [SourceTypeContainer(typeof(float))] dynamic angle,
            [SourceTypeContainer(typeof(float))] dynamic speed)
        {
            var methodName = nameof(RotateAroundAxisByAngleWithSpeed);

            if (!ValidateMethodWithLog(this, axis, methodName, 0, out Axis convertedAxis))
            {
                yield break;
            }
            
            if (!ValidateMethodWithLog(this, angle, methodName, 1, out float convertedAngle))
            {
                yield break;
            }
            
            if (!ValidateMethodWithLog(this, speed, methodName, 2, out float convertedSpeed))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentRotateRoutines.Add(routineId);

            _currentState.IsRotating = true;

            var currentRotationDoneAngle = 0d;
            var sign = Mathf.Sign(convertedAngle) * Mathf.Sign(convertedSpeed);
            convertedAngle = Mathf.Abs(convertedAngle);

            while (currentRotationDoneAngle < convertedAngle && _currentState.IsRotating)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                var rotationAmount = Mathf.Abs(convertedSpeed * Time.deltaTime);
                if (rotationAmount + currentRotationDoneAngle >= convertedAngle)
                {
                    rotationAmount = (float) (convertedAngle - currentRotationDoneAngle);
                    currentRotationDoneAngle = convertedAngle;
                }
                else
                {
                    currentRotationDoneAngle += rotationAmount;
                }
                
                transform.RotateAround(transform.position, EnumToVector(convertedAxis), sign * rotationAmount);
                yield return WaitForEndOfFrame;
            }
            
            _currentRotateRoutines.Remove(routineId);

            OnCommonRotationFinished?.Invoke();
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Controls any rotation. A paused rotation can be continued with the \"Continue\" block.",Russian:"Управляет любым вращением. Приостановленное вращение можно возобновить блоком «Продолжить».",Chinese:"控制任何旋轉。 可以使用 \\ 繼續暫停的旋轉",Kazakh:"Кез келген айналуды басқарады. Тоқтатыла тұрған айналуды \"Жалғастыру\" блогымен қайта бастауға болады.",Korean:"모든 회전을 제어합니다. 일시 정지된 회전은 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("RotateControl")]
        [Action(English:"stop any rotation",Russian:"завершить любое вращение",Chinese:"停止任何轉動",Kazakh:"кез келген айналуды аяқтау",Korean:"의 모든 회전을 정지")]
        public void StopAnyRotation()
        {
            _currentState.IsRotating = false;
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Controls any rotation. A paused rotation can be continued with the \"Continue\" block.",Russian:"Управляет любым вращением. Приостановленное вращение можно возобновить блоком «Продолжить».",Chinese:"控制任何旋轉。 可以使用 \\ 繼續暫停的旋轉",Kazakh:"Кез келген айналуды басқарады. Тоқтатыла тұрған айналуды “Жалғастыру” блогымен қайта бастауға болады.",Korean:"모든 회전을 제어합니다. 일시 정지된 회전은 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("RotateControl")]
        [Action(English:"pause any rotation",Russian:"приостановить любое вращение",Chinese:"暫停任何轉動",Kazakh:"кез келген айналуды тоқтата тұру",Korean:"의 모든 회전을 일시 정지")]
        public void PauseAnyRotation()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Controls any rotation. A paused rotation can be continued with the \"Continue\" block.",Russian:"Управляет любым вращением. Приостановленное вращение можно возобновить блоком «Продолжить».",Chinese:"控制任何旋轉。 可以使用 \\ 繼續暫停的旋轉",Kazakh:"Кез келген айналуды басқарады. Тоқтатыла тұрған айналуды “Жалғастыру” блогымен қайта бастауға болады.",Korean:"모든 회전을 제어합니다. 일시 정지된 회전은 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("RotateControl")]
        [Action(English:"continue any rotation",Russian:"продолжить любое вращение",Chinese:"繼續任何轉動",Kazakh:"кез келген айналуды жалғастыру",Korean:"의 모든 회전을 계속")]
        public void ContinueAnyRotation()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Checkers

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Returns “true” if the specified object is currently rotating. Otherwise it returns \"false\".",Russian:"Возвращает «истину», если указанный объект вращается в данный момент. В противном случае возвращает “ложь”",Chinese:"如果指定的對象當前正在旋轉，則返回“true”。 否則返回",Kazakh:"Егер көрсетілген объект осы сәтте айналып тұрса, «шындықты» қайтарады. Олай болмаған жағдайда «өтірікті» қайтарады.",Korean:"지정된 객체가 현재 회전 중이면 \" 참 ( true ) \"을 그렇지 않으면 \" 거짓 ( false ) \"을 반환합니다")]
        [Checker(English:"is rotating at the moment",Russian:"вращается в данный момент",Chinese:"正在旋轉中",Kazakh:"осы сәтте айналып тұр",Korean:"이(가) 회전하는 중 이라면")]
        public bool IsRotatingNow()
        {
            return _currentRotateRoutines.Count > 0;
        }

        #endregion

        #region Variables

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Returns the rotation angle of the object along the selected axis in world coordinates.",Russian:"Возвращает угол поворота указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳物件沿著指定軸向在世界座標系的旋轉角度",Kazakh:"Көрсетілген объектінің айналу бұрышын әлемдік координаттарда таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 선택된 축을 따라서 객체의 회전 각도를 반환합니다")]
        [VariableGroup("AngleOnAxis")]
        [Variable(English:"angle of rotation on the X axis",Russian:"угол поворота по оси X",Chinese:"X軸的旋轉角度",Kazakh:"X осі бойынша бұрылу бұрышы",Korean:"의 X축 기준 회전 각도")]
        public float AngleOnXAxis => transform.eulerAngles.x;

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Returns the rotation angle of the object along the selected axis in world coordinates.",Russian:"Возвращает угол поворота указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳物件沿著指定軸向在世界座標系的旋轉角度",Kazakh:"Көрсетілген объектінің бұрылу бұрышын әлемдік координаттарда таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 선택된 축을 따라서 객체의 회전 각도를 반환합니다")]
        [VariableGroup("AngleOnAxis")]
        [Variable(English:"angle of rotation on the Y axis",Russian:"угол поворота по оси Y",Chinese:"Y軸的旋轉角度",Kazakh:"Y осі бойынша бұрылу бұрышы",Korean:"의 Y축 기준 회전 각도")]
        public float AngleOnYAxis => transform.eulerAngles.y;

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Returns the rotation angle of the object along the selected axis in world coordinates.",Russian:"Возвращает угол поворота указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳物件沿著指定軸向在世界座標系的旋轉角度",Kazakh:"Көрсетілген объектінің бұрылу бұрышын әлемдік координаттарда таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 선택된 축을 따라서 객체의 회전 각도를 반환합니다")]
        [VariableGroup("AngleOnAxis")]
        [Variable(English:"angle of rotation on the Z axis",Russian:"угол поворота по оси Z",Chinese:"Z軸的旋轉角度",Kazakh:"Z осі бойынша бұрылу бұрышы",Korean:"의 Z축 기준 회전 각도")]
        public float AngleOnZAxis => transform.eulerAngles.z;

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Returns the rotation of the specified object in world coordinates as a vector.",Russian:"Возвращает поворот объекта в мировых координатах в виде вектора.",Chinese:"以向量回傳指定物件在世界座標系的旋轉角度",Kazakh:"Объектінің айналуын әлемдік координаттарда вектор түрінде қайтарады.",Korean:"지정된 객체의 회전을 세계 좌표에서 벡터로 반환합니다")]
        [Variable(English:"rotation",Russian:"поворот",Chinese:"旋轉",Kazakh:"бұрылыс",Korean:"회전")]
        public Vector3 Angle => transform.eulerAngles;

        #endregion

        #region Functions

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"Returns the angle of the object relative to another object along the selected axis.",Russian:"Возвращает угол поворота объекта относительно другого объекта по выбранной оси.",Chinese:"回傳物件相對於另一個物件沿著選擇的坐標軸之間的角度",Kazakh:"Объектінің екінші объектіге қатысты бұрылу бұрышын таңдалған ось бойынша қайтарады.",Korean:"선택한 축을 따라 다른 객체에 대한 객체의 각도를 반환합니다")]
        [Function(English:"the rotation angle along the axis",Russian:"угол поворота по оси",Chinese:"沿著軸的旋轉角度",Kazakh:"ось бойынша бұрылу бұрышы",Korean:"을(를) 다음 객체의 축을 따라 회전각으로 회전 축:")]
        [ArgsFormat(English:"{%} relative to the object {%}",Russian:"{%} относительно объекта {%}",Chinese:"{%}相對於物件{%}",Kazakh:"{%} объектісіне қатысты {%}",Korean:"{%} 객체:{%}")]
        public float AngleToObject(
            [SourceTypeContainer(typeof(Axis))] dynamic axis,
            [SourceTypeContainer(typeof(Wrapper))] dynamic targetObject)
        {
            var methodName = nameof(AngleToObject);

            if (!ValidateMethodWithLog(this, axis, methodName, 0, out Axis convertedAxis))
            {
                return 0f;
            }

            if (!ValidateMethodWithLog(this, targetObject, methodName, 1, out Wrapper convertedTargetObject))
            {
                return 0f;
            }

            var targetTransform = convertedTargetObject.GetGameObject()?.transform;

            var angle = 0f;

            if (targetTransform == null)
            {
                return angle;
            }

            var rotationToObject = (transform.rotation * Quaternion.Inverse(targetTransform.rotation)).eulerAngles;

            angle = convertedAxis switch
            {
                Axis.X => rotationToObject.x,
                Axis.Y => rotationToObject.y,
                Axis.Z => rotationToObject.z,
                _ => throw new ArgumentOutOfRangeException(nameof(convertedAxis), convertedAxis, null)
            };

            return angle;
        }

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Returns the rotation of an object relative to another object as a vector.",Russian:"Возвращает поворот объекта относительно другого объекта в виде вектора.",Chinese:"以向量回傳物件相對於另一個物件之間的旋轉角度",Kazakh:"Объектінің екінші объектіге қатысты айналуын вектор түрінде қайтарады.",Korean:"객체의 회전을 다른 객체를 기준으로 벡터로 반환합니다")]
        [Function(English:"rotation relative to the object",Russian:"поворот относительно объекта",Chinese:"相對於物件的旋轉角度",Kazakh:"объектіге қатысты бұрылыс",Korean:"을(를) 다음 객체에 대해 회전")]
        public Vector3 RotationToObject(Wrapper targetObject)
        {
            if (!ValidateMethodWithLog(this, targetObject, nameof(RotationToObject), 0, out Wrapper convertedTargetObject))
            {
                return Vector3.zero;
            }

            var targetTransform = convertedTargetObject.GetGameObject()?.transform;

            return targetTransform == null ? Vector3.zero : (transform.rotation * Quaternion.Inverse(targetTransform.rotation)).eulerAngles;
        }

        #endregion

        #region Events

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"The event is triggered when the object completes any rotation. The rotation is considered complete if the object has reached the rotation to the target point or if the rotation has been stopped by the corresponding block. The object for which the event was triggered is passed to the parameter.",Russian:"Событие срабатывает, когда указанный объект завершает любое вращение. Вращение считается завершенным, если объект достиг поворота к целевой точке, или если вращение было остановлено соответствующим блоком. В параметр передается объект, для которого сработало событие.",Chinese:"此事件會在物件完成任何旋轉的動作時觸發，當物件旋轉至目標點或旋轉的動作被相關的方塊中斷時",Kazakh:"Көрсетілген объект кез келген айналуды аяқтаған кезде оқиға іске қосылады. Егер объект мақсатты нүктеге бұрылса немесе егер айналуды тиісті блок тоқтатса, айналу аяқталды деп саналады. Параметрге оқиға сол үшін іске қосылған объект беріледі.",Korean:"객체가 회전을 완료하면 이벤트가 작동됩니다. 객체가 목표 지점까지 회전에 도달했거나 해당 블록에 의해 회전이 정지된 경우 회전이 완료된 것으로 간주됩니다. 이벤트가 작동된 객체는 매개변수에 전달됩니다.")]
        [LogicEvent(English:"completed any rotation",Russian:"завершил любое вращение",Chinese:"完成任何旋轉的動作",Kazakh:"кез келген айналуды аяқтады",Korean:"어떠한 회전이 완료되었을 때")]
        public event CommonRotationHandler OnCommonRotationFinished;

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"The event is triggered when the object has completed the rotation to the target object, or when it has rotated as the target object. The object for which the event was triggered and the object to which the rotation was completed (target object) are passed in parameters.",Russian:"Событие срабатывает, когда объект завершил поворот к целевому объекту, либо когда повернулся так же, как целевой объект. В параметры передается объект, для которого сработало событие, а также объект, к которому был завершен поворот (целевой объект).",Chinese:"此事件會在物件完成面向目標物件的動作時，或是與目標物件的角度相同時觸發。觸發事件的物件及目標物件會被傳送給參數。",Kazakh:"Объект мақсатты объектіге бұрылуды аяқтаған кезде немесе мақсатты объект сияқты бұрылған кезде Оқиға іске қосылады. Параметрге сол үшін оқиға іске қосылған объект, сондай-ақ оған қарай бұрылу аяқталған объект (мақсатты объект) беріледі.",Korean:"객체가 목표 객체로 회전을 완료하거나 대상 객체로 회전한 경우 이벤트가 작동합니다. 이벤트가 작동한 객체와 회전이 완료된 객체(목표 객체)를 매개변수로 전달합니다")]
        [EventGroup("ToWrapperRotation")]
        [LogicEvent(English:"has finished turning to the target site",Russian:"завершил поворот к целевому объекту",Chinese:"完成面向目標物件的動作",Kazakh:"мақсатты объектіге бұрылуды аяқтады",Korean:"다음 객체가 목표 객체로 회전하는 것을 완료했을 때")]
        public event ToWrapperRotationHandler OnToWrapperRotationFinished;

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"The event is triggered when the object has completed the rotation to the target object, or when it has rotated as the target object. The object for which the event was triggered and the object to which the rotation was completed (target object) are passed in parameters.",Russian:"Событие срабатывает, когда объект завершил поворот к целевому объекту, либо когда повернулся так же, как целевой объект. В параметры передается объект, для которого сработало событие, а также объект, к которому был завершен поворот (целевой объект).",Chinese:"此事件會在物件完成面向目標物件的動作時，或是與目標物件的角度相同時觸發。觸發事件的物件及目標物件會被傳送給參數。",Kazakh:"Объект мақсатты объектіге бұрылуды аяқтаған кезде немесе мақсатты объект сияқты бұрылған кезде оқиға іске қосылады. Параметрге сол үшін оқиға іске қосылған объект, сондай-ақ оған қарай бұрылу аяқталған объект (мақсатты объект) беріледі.",Korean:"객체가 목표 객체로 회전을 완료하거나 대상 객체로 회전한 경우 이벤트가 작동합니다. 이벤트가 작동한 객체와 회전이 완료된 객체(목표 객체)를 매개변수로 전달합니다")]
        [EventGroup("ToWrapperRotation")]
        [LogicEvent(English:"turned the same way as the target",Russian:"повернулся так же как целевой объект",Chinese:"完成旋轉至與目標物件角度相同的動作",Kazakh:"мақсатты объект сияқты бұрылды",Korean:"목표 객체와 동일한 방식으로 회전이 완료되었을 때")]
        public event ToWrapperRotationHandler OnAsWrapperRotationFinished;

        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(
English:"The event is triggered when the object completes the rotation to the target rotation. The object for which the event was triggered (rotating object) and the rotation, as a vector, to which the rotation was completed (target rotation) are passed in parameters.",Russian:"Событие срабатывает, когда объект завершает поворот к целевому вращению. В параметры передается объект, для которого сработало событие (вращающийся объект), а также вращение, в виде вектора, к которому был завершен поворот (целевое вращение).",Chinese:"此事件會在物件完成旋轉至目標角度的動作時觸發。觸發事件的物件 (旋轉中的物件)、物件的角度 (以向量表示) 以及完成旋轉時的角度 (目標角度)會被傳送給參數",Kazakh:"Объект мақсатты айналуға бұрылуды аяқтаған кезде оқиға іске қосылады. Параметрге сол үшін оқиға іске қосылған объект (айналып тұрған объект), сондай-ақ соған қарай бұрылу аяқталған айналу (мақсатты айналу) вектор түрінде беріледі.",Korean:"객체가 목표한 회전까지 회전을 완료하면 이벤트가 작동됩니다. 이벤트가 작동된 객체(회전 객체)와 회전이 완료된 회전(목표 회전)이 벡터로서 매개변수로 전달됩니다")]
        [LogicEvent(English:"completed rotating to the target rotation",Russian:"завершил поворот к целевому вращению",Chinese:"完成旋轉至目標角度的動作",Kazakh:"мақсатты айналуға бұрылысты аяқтады",Korean:"다음 객체가 목표 회전까지 회전을 완료하였을 때")]
        public event ToVectorRotationHandler OnToVectorRotationFinished;

        #endregion

        #region PrivateHelpers
        
        private void SetTargetRotation(Quaternion qAngle)
        {
            transform.rotation = qAngle;
        }

        private static float GetShortestRotationAngle(float from, float to)
        {
            while (from < 0)
            {
                from += 360;
            }

            while (to < 0)
            {
                to += 360;
            }

            while (from >= 360 - Mathf.Epsilon)
            {
                from -= 360;
            }
            
            while (to >= 360 - Mathf.Epsilon)
            {
                to -= 360;
            }

            var smallAngleTo = from == 0 && Math.Abs(to - 360) < 1;
            var smallAngleFrom = to == 0 && Math.Abs(from - 360) < 1;
            var smallDifference = Math.Abs(from - to) < 1;
            
            if (smallDifference || smallAngleFrom || smallAngleTo)
            {
                return 0;
            }

            var left = 360 - from + to;
            var right = from - to;

            if (from >= to)
            {
                return left <= right ? left : -right;
            }

            if (to > 0)
            {
                left = to - from;
                right = (360 - to) + from;
            }
            else
            {
                left = 360 - to + from;
                right = to - from;
            }

            return left <= right ? left : -right;
        }

        private static float GetRotationFromToTime(Quaternion from, Quaternion to, float speed)
        {
            var startRotationEuler = from.eulerAngles;
            var targetRotationEuler = to.eulerAngles;

            var shortestAngleX = GetShortestRotationAngle(startRotationEuler.x, targetRotationEuler.x);
            var shortestAngleY = GetShortestRotationAngle(startRotationEuler.y, targetRotationEuler.y);
            var shortestAngleZ = GetShortestRotationAngle(startRotationEuler.z, targetRotationEuler.z);

            return Mathf.Max(Mathf.Abs(shortestAngleX), Mathf.Abs(shortestAngleY), Mathf.Abs(shortestAngleZ)) / speed;
        }

        private static Quaternion GetRotationToObject(Transform rotatableTransform, Transform destinationTransform, RotationToObjectAroundAxis axis)
        {
            var direction = (destinationTransform.position - rotatableTransform.position).normalized;
            if (axis == RotationToObjectAroundAxis.All)
            {
                return Quaternion.LookRotation(direction);
            }

            var axisDirection = GetVectorAxis(axis);
            var localDirection = Quaternion.Inverse(rotatableTransform.rotation) * direction;
            var projectedDirection = Vector3.ProjectOnPlane(localDirection, axisDirection).normalized;
            var angle = Vector3.SignedAngle(Vector3.forward, projectedDirection, axisDirection);

            return rotatableTransform.rotation * Quaternion.AngleAxis(angle, axisDirection);
        }

        private static Vector3 GetVectorAxis(RotationToObjectAroundAxis axis)
        {
            return axis switch
            {
                RotationToObjectAroundAxis.LocalX => Vector3.right,
                RotationToObjectAroundAxis.LocalY => Vector3.up,
                RotationToObjectAroundAxis.All => Vector3.zero,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
            };
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion
        
        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Rotation Axis",Russian:"Ось вращения",Chinese:"旋轉軸",Kazakh:"Айналу осі",Korean:"회전축")]
        public enum Axis
        {
            [Item(English:"X",Russian:"X",Chinese:"X",Kazakh:"X",Korean:"X")] X,
            [Item(English:"Y",Russian:"Y",Chinese:"Y",Kazakh:"Y",Korean:"Y")] Y,
            [Item(English:"Z",Russian:"Z",Chinese:"Z",Kazakh:"Z",Korean:"Z")] Z,
        }
        
        [LogicGroup(English:"Rotation",Russian:"Вращение",Chinese:"旋轉",Kazakh:"Айналу",Korean:"회전")]
        [LogicTooltip(English:"Rotate to another object around a axis of rotation",Russian:"Поворот к объекту вокруг оси вращения",Chinese:"围绕对象的旋转轴旋转对象",Kazakh:"Айналу осін айнала объектіге бұрылу",Korean:"회전 축을 중심으로 다른 객체를 향해 회전합니다")]
        public enum RotationToObjectAroundAxis
        {
            [Item(English:"On all axes",Russian:"По всем осям",Chinese:"在所有轴上",Kazakh:"Барлық осьтер бойынша",Korean:"의 모든 축")] All,
            [Item(English:"Around the local X axis",Russian:"Вокруг локальной оси X",Chinese:"绕局部 X 轴",Kazakh:"X локалды осін айнала",Korean:"의 로컬 X축")] LocalX,
            [Item(English:"Around the local Y axis",Russian:"Вокруг локальной оси Y",Chinese:"绕局部 Y 轴",Kazakh:"Y локалды осін айнала",Korean:"의 로컬 X축")] LocalY,
        }
        
        protected static Vector3 EnumToVector(Axis axisDirection)
        {
            return axisDirection switch
            {
                Axis.X => Vector3.right,
                Axis.Y => Vector3.up,
                Axis.Z => Vector3.forward,
                _ => Vector3.zero
            };
        }
    }
}
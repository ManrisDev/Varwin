using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Varwin.Public;
using static Varwin.TypeValidationUtils;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class ScaleBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool IsDisabledBehaviour(GameObject targetGameObject)
        {
            return IsDisabledBehaviour(targetGameObject, BehaviourType.Scale);
        }
    }
    
    [VarwinComponent(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
    public class ScaleBehaviour : ConstructorVarwinBehaviour
    {
        private const float ScaleMinValue = 0.00001F;
        
        private readonly List<int> _currentScaleRoutines = new();
        private int _routineId;
        private readonly BehaviourState _currentState = new();
        
        public delegate void CommonScaleHandler();

        #region Actions

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(
English:"Instantly sets the scale of the specified object.",Russian:"Мгновенно задает масштаб указанного объекта.",Chinese:"瞬間設定指定物件的大小",Kazakh:"Көрсетілген объектінің масштабын лезде белгілейді.",Korean:"지정된 객체의 크기를 즉시 설정합니다.")]
        [Action(English:"set scale to",Russian:"задать масштаб",Chinese:"設定縮放比例為",Kazakh:"масштабты белгілеу",Korean:"의 배율 설정")]
        public void SetScale([SourceTypeContainer(typeof(Vector3))] dynamic targetScale)
        {
            if (!ValidateMethodWithLog(this, targetScale, nameof(SetScale), 0, out Vector3 convertedTargetScale))
            {
                return;
            }

            ValidateNonZeroScale(ref convertedTargetScale);

            var currentScale = transform.localScale;
            ScaleHierarchy(gameObject, new Vector3(
                convertedTargetScale.x / currentScale.x,
                convertedTargetScale.y / currentScale.y,
                convertedTargetScale.z / currentScale.z));
        }

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(
English:"Scales the object to the specified values within the specified time. The change is relative to the current (at the time the block is triggered) scale of the object.",Russian:"Масштабирует объект до заданных значений в течение указанного времени. Изменение происходит относительно текущего (на момент срабатывания блока)  масштаба объекта.",Chinese:"在指定的時間內縮放物件至指定的數值，此變動與物件當下的縮放比例相關 (在方塊觸發的時間點)",Kazakh:"Объектіні көрсетілген уақыт ішінде берілген мәндерге дейін масштабтайды. Өзгеріс объектінің ағымдағы (блок іске қосылған сәттегі) масштабына қатысты болады.",Korean:"지정된 시간 내에 지정된 값으로 객체의 배율을 조정합니다. 변경은 객체의 현재(블록이 작동될 때) 배율을 기준으로 합니다.")]
        [Action(English:"scale up to",Russian:"масштабировать до",Chinese:"縮放為",Kazakh:"дейін масштабтау",Korean:"을(를) 다음 좌표만큼 배율조정")]
        [ArgsFormat(English:"{%} for {%} s",Russian:"{%} в течение {%} с",Chinese:"{%}，在{%}秒內",Kazakh:"{%} с ішінде {%}",Korean:"{%} 시간:{%}s")]
        public IEnumerator ScaleUpToForTime([SourceTypeContainer(typeof(Vector3))] dynamic targetScale, [SourceTypeContainer(typeof(float))] dynamic duration)
        {
            var methodName = nameof(ScaleUpToForTime);

            if (!ValidateMethodWithLog(this, targetScale, methodName, 0, out Vector3 convertedTargetScale))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, duration, methodName, 1, out float convertedDuration))
            {
                yield break;
            }

            ValidateNonZeroScale(ref convertedTargetScale);
            
            var routineId = _routineId++;
            _currentScaleRoutines.Add(routineId);

            _currentState.IsScaling = true;

            var velocity = (convertedTargetScale - transform.localScale) / convertedDuration;

            var scalingTime = 0f;

            while (scalingTime <= convertedDuration && _currentState.IsScaling)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetScale(transform.localScale + velocity * Time.deltaTime);

                scalingTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            SetScale(convertedTargetScale);

            _currentScaleRoutines.Remove(routineId);

            OnCommonScalingFinished?.Invoke();
        }

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Scales the object by the specified number of times within the specified time. The change is relative to the current (at the time the block is triggered) scale of the object.",Russian:"Масштабирует объект в заданное количество раз в течение указанного времени. Изменение происходит относительно текущего (на момент срабатывания блока)  масштаба объекта.",Chinese:"在指定的時間內以指定的次數縮放物件，此變動與物件當下的縮放比例相關 (在方塊觸發的時間點)",Kazakh:"Объектіні көрсетілген уақыт ішінде берілген сан рет масштабтайды. Өзгеріс объектінің ағымдағы (блок іске қосылған сәттегі) масштабына қатысты болады.",Korean:"지정된 시간 내에 지정된 횟수만큼 객체의 배율을 조정합니다. 변경은 객체의 현재(블록이 작동될 때) 배율을 기준으로 합니다.")]
        [Action(English:"scale by a factor of",Russian:"масштабировать в",Chinese:"縮放多次",Kazakh:"-ға масштабтау",Korean:"을(를) 다음 요인만큼 배율 조정")]
        [ArgsFormat(English:"{%} for {%} s",Russian:"{%} раз в течение {%} с",Chinese:"{%}，在{%}秒內",Kazakh:"{%} с ішінде {%} рет",Korean:"{%} 시간:{%}s")]
        public IEnumerator ScaleByAFactorForTime([SourceTypeContainer(typeof(float))] dynamic scaleFactor, [SourceTypeContainer(typeof(float))] dynamic duration)
        {
            var methodName = nameof(ScaleByAFactorForTime);

            if (!ValidateMethodWithLog(this, scaleFactor, methodName, 0, out float convertedScaleFactor))
            {
                yield break;
            }

            if (!ValidateMethodWithLog(this, duration, methodName, 1, out float convertedDuration))
            {
                yield break;
            }

            var routineId = _routineId++;
            _currentScaleRoutines.Add(routineId);

            _currentState.IsScaling = true;

            var currentScale = transform.localScale;

            var targetScale = currentScale * convertedScaleFactor;
            ValidateNonZeroScale(ref targetScale);

            var velocity = (targetScale - currentScale) / convertedDuration;

            var scalingTime = 0f;

            while (scalingTime <= convertedDuration && _currentState.IsScaling)
            {
                while (_currentState.IsPaused)
                {
                    yield return null;
                }

                SetScale(transform.localScale + velocity * Time.deltaTime);

                scalingTime += Time.deltaTime;
                yield return WaitForEndOfFrame;
            }
            
            SetScale(targetScale);

            _currentScaleRoutines.Remove(routineId);

            OnCommonScalingFinished?.Invoke();
        }

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Controls any scaling. A paused scaling can be continued with the  “Continue” block.",Russian:"Управляет любым масштабированием. Приостановленное масштабирование можно возобновить блоком “Продолжить”.",Chinese:"控制任何的縮放動作。暫停中的縮放動作可被繼續方塊恢復",Kazakh:"Кез келген масштабтауды басқарады. Тоқтатыла тұрған масштабтауды  \"Жалғастыру\" блогымен қайта бастауға болады.",Korean:"모든 배율 조정을 제어합니다. 배율 조정이 일시 정지되는 경우 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("ScaleControl")]
        [Action(English:"stop any scaling",Russian:"завершить любое масштабирование",Chinese:"停止任何縮放",Kazakh:"кез келген масштабтауды аяқтау",Korean:"의 모든 배율조정을 정지")]
        public void StopAnyScaling()
        {
            _currentState.IsScaling = false;
        }

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Controls any scaling. A paused scaling can be continued with the  “Continue” block.",Russian:"Управляет любым масштабированием. Приостановленное масштабирование можно возобновить блоком “Продолжить”.",Chinese:"控制任何的縮放動作。暫停中的縮放動作可被繼續方塊恢復",Kazakh:"Кез келген масштабтауды басқарады. Тоқтатыла тұрған масштабтауды  \"Жалғастыру\" блогымен қайта бастауға болады.",Korean:"모든 배율 조정을 제어합니다. 배율 조정이 일시 정지되는 경우 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("ScaleControl")]
        [Action(English:"pause any scaling",Russian:"приостановить любое масштабирование",Chinese:"暫停任何縮放",Kazakh:"кез келген масштабтауды тоқтата тұру",Korean:"의 모든 배율조정을 일시정지")]
        public void PauseAnyScaling()
        {
            _currentState.IsPaused = true;
        }

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Controls any scaling. A paused scaling can be continued with the  “Continue” block.",Russian:"Управляет любым масштабированием. Приостановленное масштабирование можно возобновить блоком “Продолжить”.",Chinese:"控制任何的縮放動作。暫停中的縮放動作可被繼續方塊恢復",Kazakh:"Кез келген масштабтауды басқарады. Тоқтатыла тұрған масштабтауды  \"Жалғастыру\" блогымен қайта бастауға болады.",Korean:"모든 배율 조정을 제어합니다. 배율 조정이 일시 정지되는 경우 \" 계속 \" 블록을 사용하여 계속할 수 있습니다.")]
        [ActionGroup("ScaleControl")]
        [Action(English:"continue any scaling",Russian:"продолжить любое масштабирование",Chinese:"繼續任何縮放",Kazakh:"кез келген масштабтауды жалғастыру",Korean:"의 모든 배율조정을 계속")]
        public void ContinueAnyScaling()
        {
            _currentState.IsPaused = false;
        }

        #endregion

        #region Checkers


        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Returns “true” if the object is currently scaled. Otherwise it returns “false”.",Russian:"Возвращает “истину”, если объект масштабируется в данный момент. В противном случае возвращает “ложь”.",Chinese:"當物件正在縮放時回傳值為真；反之，回傳為假",Kazakh:"Егер объект осы сәтте масштабталып жатса, “шындықты” қайтарады. Олай болмаған жағдайда “өтірікті” қайтарады.",Korean:"객체의 배율이 현재 조정되어 있으면 \" 참 ( true ) \"을 반환함. 그렇지 않으면 \" 거짓 ( false ) \"을 반환함")]
        [Checker(English:"is scaling at the moment",Russian:"масштабируется в данный момент",Chinese:"正在縮放",Kazakh:"осы сәтте масштабталуда",Korean:"이(가) 배율조정 중이라면")]
        public bool IsScalingNow()
        {
            return _currentScaleRoutines.Count > 0;
        }

        #endregion

        #region Events

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"The event is triggered when the specified object completes any scaling. The scaling is considered complete if the object has reached the target scale or if the scaling was stopped by the corresponding block. The object for which the event was triggered is passed to the parameter.",Russian:"Событие срабатывает, когда указанный объект завершает любое масштабирование. Вращение считается завершенным, если объект достиг целевого масштаба, или если масштабирование было остановлено соответствующим блоком. В параметр передается объект, для которого сработало событие. ",Chinese:"此事件會在物件完成任何縮放的動作時觸發，縮放的動作會在物件縮放至目標比例時或被相關的方塊停止時視為完成。觸發事件的物件會被傳送給參數。",Kazakh:"Көрсетілген объект кез келген масштабтауды аяқтағанда оқиға іске қосылады. Егер объект мақсатты масштабқа жетсе немесе масштабтауды тиісті блок тоқтатса, айналу аяқталды деп саналады. Параметрге сол үшін оқиға іске қосылған объект беріледі.",Korean:"지정된 객체가 배율 조정을 완료하면 이벤트가 작동됩니다. 객체가 목표 크기에 도달했거나 해당 블록에 의해 배율 조정이 중지된 경우 회전이 완료된 것으로 간주됩니다. 이벤트가 작동한 객체가 매개변수에 전달됩니다.")]
        [LogicEvent(English:"completed any scaling",Russian:"завершил любое масштабирование",Chinese:"完成任何縮放",Kazakh:"кез келген масштабтауды аяқтады",Korean:"배율 조정이 완료되었을 때")]
        public event CommonScaleHandler OnCommonScalingFinished;

        #endregion

        #region Variables

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Returns the scale of the specified object along the selected axis in world coordinates.",Russian:"Возвращает масштаб указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳指定物件沿著選擇的座標軸在世界座標系的縮放比例",Kazakh:"Көрсетілген объектінің масштабын әлемдік координаттарда таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 선택한 축을 따라 지정된 객체의 배율을 반환합니다")]
        [VariableGroup("AxisScale")]
        [Variable(English:"scale on the axis X",Russian:"масштаб по оси X",Chinese:"沿著X軸的縮放比例",Kazakh:"X осі бойынша масштаб",Korean:"의 X축 상 배율")]
        public float ScaleX => transform.localScale.x;

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Returns the scale of the specified object along the selected axis in world coordinates.",Russian:"Возвращает масштаб указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳指定物件沿著選擇的座標軸在世界座標系的縮放比例",Kazakh:"Көрсетілген объектінің масштабын әлемдік координаттарда таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 선택한 축을 따라 지정된 객체의 배율을 반환합니다")]
        [VariableGroup("AxisScale")]
        [Variable(English:"scale on the axis Y",Russian:"масштаб по оси Y",Chinese:"沿著Y軸的縮放比例",Kazakh:"Y осі бойынша масштаб",Korean:"의 Y축 상 배율")]
        public float ScaleY => transform.localScale.y;

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Returns the scale of the specified object along the selected axis in world coordinates.",Russian:"Возвращает масштаб указанного объекта по выбранной оси в мировых координатах.",Chinese:"回傳指定物件沿著選擇的座標軸在世界座標系的縮放比例",Kazakh:"Көрсетілген объектінің масштабын әлемдік координаттарда таңдалған ось бойынша қайтарады.",Korean:"세계 좌표에서 선택한 축을 따라 지정된 객체의 배율을 반환합니다")]
        [VariableGroup("AxisScale")]
        [Variable(English:"scale on the axis Z",Russian:"масштаб по оси Z",Chinese:"沿著Z軸的縮放比例",Kazakh:"Z осі бойынша масштаб",Korean:"의 Z축 상 배율")]
        public float ScaleZ => transform.localScale.z;

        [LogicGroup(English:"Scale",Russian:"Масштабирование",Chinese:"縮放",Kazakh:"Масштабтау",Korean:"배율")]
        [LogicTooltip(English:"Returns the scale of the specified object in world coordinates as a vector.",Russian:"Возвращает масштаб указанного объекта в мировых координатах в виде вектора.",Chinese:"以向量回傳物件在世界座標系中的縮放比例",Kazakh:"Көрсетілген объектінің масштабын әлемдік координаттарда вектор түрінде қайтарады.",Korean:"지정된 객체의 세계 좌표 배율을 벡터로 반환합니다.")]
        [Variable(English:"scale",Russian:"масштаб",Chinese:"縮放",Kazakh:"масштаб",Korean:"의 배율")]
        public Vector3 Scale => transform.localScale;

        #endregion

        #region PrivateHelpers

        private static void ScaleHierarchy(GameObject gameObjectToScale, Vector3 scaleDelta)
        {
            foreach (var objectToScaleTransform in GetHierarchy(gameObjectToScale))
            {
                objectToScaleTransform.localScale = Vector3.Scale(objectToScaleTransform.localScale, scaleDelta);
            }
        }

        private static void ValidateNonZeroScale(ref Vector3 scale)
        {
            scale.x = Mathf.Approximately(0f, scale.x) ? ScaleMinValue : scale.x;
            scale.y = Mathf.Approximately(0f, scale.y) ? ScaleMinValue : scale.y;
            scale.z = Mathf.Approximately(0f, scale.z) ? ScaleMinValue : scale.z;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion
    }
}
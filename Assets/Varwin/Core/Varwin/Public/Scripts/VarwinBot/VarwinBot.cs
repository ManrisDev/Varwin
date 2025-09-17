using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Varwin.Public
{
    [RequireComponent(
        typeof(VarwinObjectDescriptor), 
        typeof(Animator), 
        typeof(CharacterController))]
    [DisallowMultipleComponent]
    [VarwinComponent(English:"Bot",Russian:"Бот",Chinese:"機器人",Kazakh:"Бот",Korean:"봇")]
    public class VarwinBot : MonoBehaviour, ISwitchModeSubscriber
    {
        public delegate void BotTargetReachedEventHandler([Parameter(English:"target object",Russian:"целевой объект",Chinese:"目標物件",Kazakh:"мақсатты объект",Korean:"목표 객체")] Wrapper target);
        [LogicEvent(English:"Target object reached",Russian:"Целевой объект достигнут",Chinese:"抵達目標物件",Kazakh:"Мақсатты объектіге жеттік",Korean:"대상 객체에 도달했을 때")]
        [EventCustomSender(English:"Bot",Russian:"Бот",Chinese:"機器人",Kazakh:"Бот",Korean:"봇")]
        public event BotTargetReachedEventHandler BotTargetReached;

        public delegate void BotPathPointEventHandler(
            [Parameter(English:"waypoint number",Russian:"номер точки в маршруте",Chinese:"導航點編號",Kazakh:"маршруттағы нүктенің нөмірі",Korean:"웨이포인트 번호")] int id, 
            [Parameter(English:"waypoint number",Russian:"номер точки в маршруте",Chinese:"導航點編號",Kazakh:"маршруттағы нүктенің нөмірі",Korean:"웨이포인트 번호")] Wrapper point);
        
        [LogicEvent(English:"Path point reached",Russian:"Точка пути достигнута",Chinese:"抵達路徑點",Kazakh:"Жол нүктесіне жеттік",Korean:"경로 지점에 도달했을 때")]
        [EventCustomSender(English:"Bot",Russian:"Бот",Chinese:"機器人",Kazakh:"Бот",Korean:"봇")]
        public event BotPathPointEventHandler BotPathPointReached;
        
        public enum MovementPace
        {
            [Item(English:"Walking",Russian:"Шагом",Chinese:"行走",Kazakh:"Адымдап",Korean:"걷기")]
            Walk,
            [Item(English:"Running",Russian:"Бегом",Chinese:"奔跑",Kazakh:"Жүгіріп",Korean:"달리기")]
            Run
        }
        
        public enum MovementDirection
        {
            [Item(English:"Forward",Russian:"Вперед",Chinese:"前進",Kazakh:"Алға",Korean:"전방")]
            Forward,
            [Item(English:"Backward",Russian:"Назад",Chinese:"後退",Kazakh:"Артқа",Korean:"후방")]
            Backward,
            [Item(English:"Left",Russian:"Влево",Chinese:"左行",Kazakh:"Солға",Korean:"왼쪽")]
            Left,
            [Item(English:"Right",Russian:"Вправо",Chinese:"右行",Kazakh:"Оңға",Korean:"오른쪽")]
            Right
        }
        
        public enum RotationDirection
        {
            [Item(English:"Clockwise",Russian:"По часовой стрелке",Chinese:"順時針",Kazakh:"Сағат тілімен",Korean:"시계방향")]
            Clockwise,
            [Item(English:"Counterclockwise",Russian:"Против часовой стрелки",Chinese:"逆時針",Kazakh:"Сағат тіліне қарсы",Korean:"시계 반대 방향")]
            Counterclockwise
        }
        
        public enum TextBubbleHideType
        {
            [Item(English:"Automatic",Russian:"Автоматически",Chinese:"自動",Kazakh:"Автоматты түрде",Korean:"자동")]
            Automatic,
            [Item(English:"Never",Russian:"Никогда",Chinese:"從不",Kazakh:"Ешқашан",Korean:"무시")]
            Never
        }
        
        [Obsolete]
        [HideInInspector]
        public List<VarwinBotCustomAnimation> CustomAnimations;
        
        public enum MovementType
        {
            None,
            Infinite,
            ForMeters,
            TowardsObject,
            SimplePath
        }

        private float _targetObjectMinDistance = 0.5f;
        private float _groundCheckMaxDistance = 2f;
        
        private Animator _animator;
        private Rigidbody _rigidbody;
        private CharacterController _characterController;

        public CharacterController CharacterController => _characterController;

        [Obsolete] private int _animationClipId;
        
        private GameObject _targetObject;
        private float _lastDistance;
        private int _currentPathPoint;

        private MovementType _currentMovementType;
        private MovementPace _currentMovementPace;
        private MovementDirection _currentMovementDirection;

        public MovementPace CurrentMovementPace => _currentMovementPace;
        public MovementType CurrentMovementType => _currentMovementType;

        public MovementDirection CurrentMovementDirection => _currentMovementDirection;

        private float _distanceMoved;
        private float _distanceRequired;
        private Vector3 _lastPosition;
        
        private float _angularVelocity;
        private float _rotationTime;
        private float _currentRotationTime;
        
        [Obsolete] private bool _repeatAnimation;
        [Obsolete] private RuntimeAnimatorController _animatorController;
        
        [Obsolete] private PlayableGraph _playableGraph;
        [Obsolete] private AnimationPlayableOutput _animationPlayableOutput;
        [Obsolete] private List<AnimationClipPlayable> _clipPlayables;
        
        private VarwinBotTextBubble _textBubble;
        
        [SerializeField] private bool _useCustomAnimatorController = false;
        
        private bool _showTextBubble;
        private CoroutineWrapper _currentMovementCoroutine = null;
        private CoroutineWrapper _currentMovementPathCoroutine = null;

        [Obsolete]
        public bool IsHumanoid => Animator && Animator.avatar && Animator.avatar.isHuman;

        [ActionGroup("CharacterControllerParameters")]
        [Action(English:"Set max traversable slope angle",Russian:"Задать максимальный угол подъема",Chinese:"設定最大可行走坡度",Kazakh:"Максималды көтеру бұрышын белгілеу",Korean:"의 최대 등반 가능한 경사각 설정")]
        public void SetMaxTraversableSlope(float slope)
        {
            _characterController.slopeLimit = slope;
        }
        
        [ActionGroup("CharacterControllerParameters")]
        [Action(English:"Set max step height",Russian:"Задать максимальную высоту шага",Chinese:"設定最大步伐高度",Kazakh:"Қадамның максималды биіктігін белгілеу",Korean:"의 최대 발걸음 높이 설정")]
        public void SetMaxStepHeight(float stepHeight)
        {
            _characterController.stepOffset = Mathf.Clamp(stepHeight, 0, _characterController.height);;
        }        
        
        [Action(English:"Set object stop distance",Russian:"Задать расстояние остановки перед объектом",Chinese:"設定碰到物件停止的最小距離",Kazakh:"Объектінің алдында тоқтау қашықтығын белгілеу",Korean:"의 객체 정지 거리")]
        public void SetMinObjectDistance(float objectDistance)
        {
            _targetObjectMinDistance = objectDistance;
        }
        
        [Action(English:"Rotate",Russian:"Повернуть",Chinese:"旋轉",Korean:"이(가) 회전")]
        [ArgsFormat(English:"for {%} degrees {%} in {%} sec",Russian:"на {%} градусов {%} за {%} сек",Chinese:"對物件{%}，旋轉{%}度於{%}秒內",Korean:"각도:{%} 회전방향:{%} 시간:{%}")]
        [Obsolete]
        public void RotateForDegrees(float angle, RotationDirection rotationDirection, float time)
        {
            StartCoroutine(RotateByAngle(angle, rotationDirection, time));
        }

        [Action(English:"Rotate",Russian:"Повернуть",Chinese:"旋轉",Kazakh:"Бұру",Korean:"이(가) 회전")]
        [ArgsFormat(English:"for {%} degrees {%} in {%} sec",Russian:"на {%} градусов {%} за {%} сек",Chinese:"對物件{%}，旋轉{%}度於{%}秒內",Kazakh:"{%} сек ішінде {%} {%} градусқа",Korean:"각도:{%} 회전방향:{%} 시간:{%}")]
        public IEnumerator RotateByAngle(float angle, RotationDirection rotationDirection, float time)
        {
            var angularVelocity = angle / time;

            if (rotationDirection == RotationDirection.Counterclockwise)
            {
                angularVelocity *= -1;
            }

            var startRotation = transform.rotation;
            var endRotation = transform.rotation * Quaternion.AngleAxis(angularVelocity, Vector3.up);

            var currentTime = time;
            
            while (currentTime > 0)
            {
                var t = currentTime / time;
                
                transform.rotation = Quaternion.Lerp(startRotation, endRotation, Mathf.Clamp01(1f - t));
                currentTime -= Time.deltaTime;
                yield return null;
            }

            transform.rotation = endRotation;
        }

        [Action(English:"Move",Russian:"Двигаться",Chinese:"移動",Kazakh:"Қозғалу",Korean:"이(가) 다음 방향으로 이동")]
        [ArgsFormat(English:"{%} at {%} pace until stopped",Russian:"{%} {%} до остановки",Chinese:"以 {%} 的速度 {%} 直到停止",Kazakh:"{%} {%} тоқтағанға дейін",Korean:"{%} {%} 페이스를 유지하여 멈출 때 까지")]
        public void MoveInfinite(MovementDirection movementDirection, MovementPace movementPace)
        {
            SetAnimatorParameters(movementDirection, movementPace);
        }
        
        [Action(English:"Move",Russian:"Передвинуться",Chinese:"移動",Korean:"이(가) 다음 방향으로 이동")]
        [ArgsFormat(English:"{%} at {%} pace for {%} m",Russian:"{%} {%} на расстояние {%} м",Chinese:"{%} 以 {%} 步速持續 {%} 米",Korean:"{%} {%}페이스로 {%} m")]
        [Obsolete]
        public void MoveForMeters(MovementDirection movementDirection, MovementPace movementPace, float distance)
        {
            StartCoroutine(MoveByMeters(movementDirection, movementPace, distance));
        }
        
        [Action(English:"Move",Russian:"Передвинуться",Chinese:"移動",Kazakh:"Жылжу",Korean:"이(가) 다음 방향으로 이동")]
        [ArgsFormat(English:"{%} at {%} pace for {%} m",Russian:"{%} {%} на расстояние {%} м",Chinese:"{%} 以 {%} 步速持續 {%} 米",Kazakh:"{%} ара қашықтығына {%} {%}",Korean:"{%} {%}페이스로 {%} m")]
        public IEnumerator MoveByMeters(MovementDirection movementDirection, MovementPace movementPace, float distance)
        {
            StopMovement();
            
            _currentMovementCoroutine = new CoroutineWrapper(MoveByMetersCoroutine(movementDirection, movementPace, distance));
            yield return _currentMovementCoroutine;
        }

        private IEnumerator MoveByMetersCoroutine(MovementDirection movementDirection, MovementPace movementPace, float distance)
        {
            var clampedDistance = Mathf.Clamp(distance, 0, distance);

            if (clampedDistance == 0)
            {
                StopMovement();
                yield break;
            }

            SetAnimatorParameters(movementDirection, movementPace);

            var sumDistance = 0f;
            var lastPosition = transform.position;

            while (sumDistance < distance)
            {
                sumDistance += Vector3.Distance(lastPosition, transform.position);
                lastPosition = transform.position;

                yield return null;
            }

            StopMovement();
        }

        [Action(English:"Move",Russian:"Двигаться",Chinese:"移動",Korean:"이(가) 다음 페이스로 이동")]
        [ArgsFormat(English:"at {%} pace towards object {%}",Russian:"{%} в сторону объекта {%}",Chinese:"以{%}的速度朝向物件{%}移動",Korean:"{%} 향하는 객체:{%}")]
        [Obsolete]
        public void MoveTowardsObject(MovementPace movementPace, Wrapper wrapper)
        {
            StartCoroutine(MoveToObject(movementPace, wrapper));
        }    
        
        [Action(English:"Move",Russian:"Двигаться",Chinese:"移動",Kazakh:"Қозғалу",Korean:"이(가) 다음 페이스로 이동")]
        [ArgsFormat(English:"at {%} pace towards object {%}",Russian:"{%} в сторону объекта {%}",Chinese:"以{%}的速度朝向物件{%}移動",Kazakh:"{%} объектісі жағына қарай {%}",Korean:"{%} 향하는 객체:{%}")]
        public IEnumerator MoveToObject(MovementPace movementPace, Wrapper wrapper)
        {
            StopMovement();
            
            _currentMovementCoroutine = new CoroutineWrapper(MoveToObjectCoroutine(movementPace, wrapper));
            yield return _currentMovementCoroutine;
        }

        private IEnumerator MoveToObjectCoroutine(MovementPace movementPace, Wrapper wrapper)
        {
            var targetObject = wrapper.GetGameObject();

            Animator.SetFloat("InputVertical", 1);

            var lastDistance = 0f;

            while (true)
            {
                var targetObjectPosition = targetObject.transform.position;
                var distance = Vector3.Distance(targetObjectPosition - Vector3.up * targetObjectPosition.y, transform.position - Vector3.up * transform.position.y);

                var targetPosition = new Vector3(targetObjectPosition.x, transform.position.y, targetObjectPosition.z);

                transform.LookAt(targetPosition);

                if (distance <= _targetObjectMinDistance)
                {
                    if (lastDistance > _targetObjectMinDistance)
                    {
                        var eventObjectWrapper = targetObject.GetWrapper();
                        BotTargetReached?.Invoke(eventObjectWrapper);

                        StopMovement();
                        yield break;
                    }
                }
                else
                {
                    float inputMagnitude = movementPace switch
                    {
                        MovementPace.Walk => 0.5f,
                        MovementPace.Run => 1f,
                        _ => 0
                    };

                    Animator.SetFloat("InputMagnitude", inputMagnitude);
                }

                lastDistance = distance;
                yield return null;
            }
        }

#if !NET_STANDARD_2_0
        [Action(English:"Move",Russian:"Двигаться",Chinese:"移動",Korean:"이(가) 다음 페이스로 이동")]
        [ArgsFormat(English:"at {%} pace along the path {%}",Russian:"{%} по маршруту {%}",Chinese:"以{%}的速度沿著路徑{%}移動",Korean:"{%} 이동경로:{%}")]
        [Obsolete]
        public void MoveByPathSimple(MovementPace movementPace, dynamic points)
        {
            StartCoroutine(MoveAlongPath(movementPace, points));
        }
#endif
        
        [Action(English:"Move",Russian:"Двигаться",Chinese:"移動",Kazakh:"Қозғалу",Korean:"이(가) 다음 페이스로 이동")]
        [ArgsFormat(English:"at {%} pace along the path {%}",Russian:"{%} по маршруту {%}",Chinese:"以{%}的速度沿著路徑{%}移動",Kazakh:"{%} маршруты бойынша {%}",Korean:"{%} 이동경로:{%}")]
        public IEnumerator MoveAlongPath(MovementPace movementPace, dynamic points)
        {
            StopMovement();
            
            _currentMovementCoroutine = new CoroutineWrapper(MoveAlongPathCoroutine(movementPace, points));
            _currentMovementPathCoroutine = _currentMovementCoroutine;
            
            yield return _currentMovementCoroutine;
        }

        private IEnumerator MoveAlongPathCoroutine(MovementPace movementPace, dynamic points)
        {
            var pointsList = points switch
            {
                Wrapper wrapper => new List<dynamic> {wrapper},
                IEnumerable list => list.Cast<dynamic>().ToList(),
                _ => null
            };

            if (pointsList == null || pointsList.Count == 0)
            {
                yield break;
            }

            var pointIndex = 0;
            var lastDistance = 0f;

            while (true)
            {
                var element = pointsList[pointIndex];

                if (element is not Wrapper wrapper)
                {
                    pointIndex++;

                    if (pointIndex >= pointsList.Count)
                    {
                        StopMovement();
                        yield break;
                    }

                    continue;
                }

                var targetObjectPosition = wrapper.GetGameObject().transform.position;

                Animator.SetFloat("InputVertical", 1);

                var distance = Vector3.Distance(targetObjectPosition - Vector3.up * targetObjectPosition.y, transform.position - Vector3.up * transform.position.y);
                var targetPosition = new Vector3(targetObjectPosition.x, transform.position.y, targetObjectPosition.z);

                transform.LookAt(targetPosition);

                if (distance <= _targetObjectMinDistance)
                {
                    if (lastDistance > _targetObjectMinDistance)
                    {
                        pointIndex++;
                        BotPathPointReached?.Invoke(pointIndex, wrapper);

                        if (pointIndex >= pointsList.Count)
                        {
                            StopMovement();
                            yield break;
                        }
                    }
                }
                else
                {
                    var inputMagnitude = movementPace switch
                    {
                        MovementPace.Walk => 0.5f,
                        MovementPace.Run => 1f,
                        _ => 0
                    };

                    Animator.SetFloat("InputMagnitude", inputMagnitude);
                }

                lastDistance = distance;
                yield return null;
            }
        }

        [ActionGroup("PathPauseUnpause")]
        [Action(English:"Pause movement along the path",Russian:"Приостановить движение по маршруту",Chinese:"暫停沿著路徑移動",Kazakh:"Маршрут бойынша қозғалысты тоқтата тұру",Korean:"이(가) 이동 경로를 따라 진행 중지")]
        public void PausePath()
        {
            if (_currentMovementPathCoroutine == null)
            {
                return;
            }
            
            Animator.SetFloat("InputHorizontal", 0);
            Animator.SetFloat("InputVertical", 0);
            Animator.SetFloat("InputMagnitude", 0);
                
            _currentMovementPathCoroutine.Pause();
        }
        
        [ActionGroup("PathPauseUnpause")]
        [Action(English:"Continue movement along the path",Russian:"Продолжить движение по маршруту",Chinese:"繼續沿著路徑移動",Kazakh:"Маршрут бойынша қозғаысты жалғастыру",Korean:"이(가) 이동 경로를 따라 계속 진행")]
        public void ContinuePath()
        {
            _currentMovementPathCoroutine?.Play();
        }
        
        [Action(English:"Stop motion",Russian:"Остановить движение",Chinese:"停止移動",Kazakh:"Қозғалысты тоқтату",Korean:"의 움직임 정지")]
        public void StopMovement()
        {
            _currentMovementCoroutine?.Stop();
            _currentMovementCoroutine = null;
            _currentMovementPathCoroutine = null;
            
            Animator.SetFloat("InputHorizontal", 0);
            Animator.SetFloat("InputVertical", 0);
            Animator.SetFloat("InputMagnitude", 0);
        }
        
        [Obsolete]
        public void PlayCustomAnimationOnce([UseValueList("VarwinBotCustomAnimationClips")] int clipId)
        {
            _repeatAnimation = false;
            
            PlayClipWithId(clipId);
        } 
        
        [Obsolete]
        public void PlayCustomAnimationRepeatedly([UseValueList("VarwinBotCustomAnimationClips")] int clipId)
        {
            _repeatAnimation = true;
            
            PlayClipWithId(clipId);
        }
        
        [Obsolete]
        public void StopCustomAnimation()
        {
            _playableGraph.Stop();
        }
        
        [Action(English:"Set text bubble hide type",Russian:"Задать тип скрывания говоримого текста",Chinese:"設定文字泡泡隱藏類型",Kazakh:"Айтылып жатқан мәтінді жасыру типін белгілеу",Korean:"의 말풍선 숨기기 유형 설정")]
        public void SetShowTextBubbleHideType(TextBubbleHideType hideType)
        {
            _textBubble.HideType = hideType;
        }

        [Action(English:"Set text bubble enabled",Russian:"Задать отображение говоримого текста",Chinese:"設定啟用文字泡泡",Kazakh:"Айтылып жатқан мәтіннің көрсетілуін белгілеу",Korean:"의 말풍선 활성화 설정")]
        public void SetShowTextBubble(bool show)
        {
            _textBubble.ShowTextBubble = show;
        }

        [VarwinInspector(English:"Show text bubble",Russian:"Отображать говоримый текст",Chinese:"顯示文字泡泡",Kazakh:"Айтылып жатқан мәтінді көрсету",Korean:"말풍선 표시")]
        public bool ShowTextBubble
        {
            get =>  _textBubble ? _textBubble && _textBubble.ShowTextBubble : true;
            set
            {
                if (_textBubble)
                {
                    _textBubble.ShowTextBubble = value;
                }
            }
        }
        
        [VarwinInspector(English:"Hide text automatically",Russian:"Скрывать текст автоматически",Chinese:"自動隱藏文字",Kazakh:"Мәтінді автоматты түрде жасыру",Korean:"텍스트 자동 숨기기")]
        public bool AutoHideTextBubble
        {
            get => _textBubble ? _textBubble.HideType == TextBubbleHideType.Automatic : true;
            set
            {
                if (_textBubble)
                {
                    _textBubble.HideType = value ? TextBubbleHideType.Automatic : TextBubbleHideType.Never;
                }
            }
        }
        
        [VarwinInspector(English:"The text bubble looks at the Player",Russian:"Отображаемый текст следит за игроком",Chinese:"文字氣泡看著玩家",Kazakh:"Көрсетілетін мәтін ойыншыны бақылауда",Korean:"플레이어 방향을 바라보는 말풍선")]
        public bool TextBubbleLookAtCamera
        {
            get =>  _textBubble && (_textBubble && _textBubble.LookAtCamera);
            set
            {
                if (_textBubble)
                {
                    _textBubble.LookAtCamera = value;
                }
            }
        }

        public Animator Animator => _animator ??= GetComponent<Animator>();

        [LogicEvent(English:"On speech completed",Russian:"Фраза произнесена",Chinese:"當演說結束",Kazakh:"Фраза айтылды",Korean:"대사가 끝났을 때")]
        [EventCustomSender(English:"Bot",Russian:"Бот",Chinese:"機器人",Kazakh:"Бот",Korean:"봇")]
        public event Action SpeechCompleted;
        
        [Action(English:"Say",Russian:"Сказать",Chinese:"說",Kazakh:"Айту",Korean:"말하기")]
        public void SayText(string text)
        {
            if (_textBubble)
            {
                _textBubble.ShowText(">", text);
            }
        }
        
        [Action(English:"Say",Russian:"Сказать",Chinese:"說",Kazakh:"Айту",Korean:"말하기")]
        [ArgsFormat(English:"header: {%} text: {%}",Russian:"заголовок: {%} текст: {%}",Chinese:"標題: {%} 文字: {%}",Kazakh:"тақырып: {%} мәтін: {%}",Korean:"제목: {%} 내용: {%}")]
        public void SayTextWith(string header, string text)
        {
            if (_textBubble)
            {
                _textBubble.ShowText(header, text);
            }
        }
        
        [Action(English:"Stop speaking",Russian:"Перестать говорить",Chinese:"停止說",Kazakh:"Сөйлеуді тоқтату",Korean:"의 말하기 정지")]
        public void StopSpeaking()
        {
            if (_textBubble)
            {
                _textBubble.HideText();
            }
        }

        private void Awake()
        {
            _textBubble = GetComponent<VarwinBotTextBubble>();

            _currentMovementType = MovementType.None;
            
            _rigidbody = GetComponent<Rigidbody>();
            _characterController = GetComponent<CharacterController>();

            if (!_useCustomAnimatorController)
            {
                Animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimatorControllers/VarwinBotController");
            }
            
            Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            
            _playableGraph = PlayableGraph.Create();
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", Animator);

            _clipPlayables = new List<AnimationClipPlayable>();

            foreach (VarwinBotCustomAnimation customAnimation in CustomAnimations)
            {
                var clipPlayable = AnimationClipPlayable.Create(_playableGraph, customAnimation.Clip);
                _clipPlayables.Add(clipPlayable);
            }
            
            _groundCheckMaxDistance = _characterController.height;

            if (!_textBubble)
            {
                GameObject bubbleObject = Instantiate(Resources.Load<GameObject>("TextBubble"));
                
                Transform head = Animator.GetBoneTransform(HumanBodyBones.Head);

                if (head)
                {
                    bubbleObject.transform.position = head.position + Vector3.up * 0.3f;
                }
                else
                {
                    bubbleObject.transform.position = transform.position + Vector3.up * 2.0f;
                }

                bubbleObject.transform.rotation = transform.rotation;
                
                bubbleObject.transform.SetParent(transform);

                _textBubble = gameObject.AddComponent<VarwinBotTextBubble>();
                _textBubble.Container = bubbleObject;
                _textBubble.HeaderText = bubbleObject.transform.Find("Canvas").Find("Header").GetComponent<TMP_Text>();
                _textBubble.MainText = bubbleObject.transform.Find("Canvas").Find("Text").GetComponent<TMP_Text>();
                _textBubble.ResetDefaultScale();
            }
            
            _textBubble.HideText();
            _textBubble.SpeechCompleted += () => SpeechCompleted?.Invoke();
        }

        private void Update()
        {
            Ray ray = new Ray(transform.position + 0.2f * _characterController.height * transform.localScale.y * Vector3.up, Vector3.down);
            RaycastHit groundHit;
            
            bool isGrounded;

            if (Physics.Raycast(ray, out groundHit, _groundCheckMaxDistance))
            {
                float groundDistance = Vector3.Distance(transform.position, groundHit.point);

                isGrounded = (groundDistance <= 0.5f * _characterController.height * transform.localScale.y);
            }
            else
            {
                isGrounded = false;
            }
            
            Animator.SetBool("IsGrounded", isGrounded);

            if (_playableGraph.IsPlaying())
            {
                float timeDiff = Mathf.Abs((float) _clipPlayables[_animationClipId].GetTime() - CustomAnimations[_animationClipId].Clip.length);

                if (timeDiff < Time.deltaTime)
                {
                    if (_repeatAnimation)
                    {
                        _clipPlayables[_animationClipId].SetTime(0);
                    }
                    else
                    {
                        _playableGraph.Stop();
                    }
                }
            }
        }

        public void OnSwitchMode(GameMode newMode, GameMode oldMode)
        {
            if (newMode == GameMode.Edit)
            {
                if (_textBubble)
                {
                    _textBubble.HideText();
                }

                StopMovement();
                
                Animator.applyRootMotion = false;
            }
            else
            {
                if (_textBubble)
                {
                    _textBubble.ResetDefaultScale();
                }
                
                Animator.applyRootMotion = true;
            }
        }
        
        public void MoveBot(MovementDirection movementDirection, MovementPace movementPace)
        {
            SetAnimatorParameters(movementDirection, movementPace);
        }

        private void SetAnimatorParameters(MovementDirection movementDirection, MovementPace movementPace)
        {
            float inputMagnitude = 0;
            float inputHorizontal = 0;
            float inputVertical = 0;

            inputMagnitude = movementPace switch
            {
                MovementPace.Walk => 0.5f,
                MovementPace.Run => 1f,
                _ => inputMagnitude
            };

            switch (movementDirection)
            {
                case MovementDirection.Forward:
                    inputVertical = 1f;
                    break;
                case MovementDirection.Backward:
                    inputVertical = -1f;
                    break;
                case MovementDirection.Left:
                    inputHorizontal = -1f;
                    break;
                case MovementDirection.Right:
                    inputHorizontal = 1f;
                    break;
            }
            
            Animator.SetFloat("InputHorizontal", inputHorizontal);
            Animator.SetFloat("InputVertical", inputVertical);
            Animator.SetFloat("InputMagnitude", inputMagnitude);
        }

        [Obsolete]
        private void PlayClipWithId(int clipId)
        {
            if (clipId == -1)
            {
                return;
            }
            
            _animationClipId = clipId;
            
            _animationPlayableOutput.SetSourcePlayable(_clipPlayables[_animationClipId]);
            _clipPlayables[_animationClipId].SetTime(0);
            _playableGraph.Play();
        }

        #region BACKWARD COMPATIBILITY CODE
        [Obsolete]
        public List<VarwinCustomAnimation> GetCustomAnimations()
        {
            List <VarwinCustomAnimation> customAnimations = new List<VarwinCustomAnimation>();
            foreach (var customAnimation in  CustomAnimations)
            {
                customAnimations.Add(customAnimation);
            }
            return customAnimations;
        }
        [Obsolete]
        public string GetCustomAnimationsValueListName()
        {
            return "VarwinBotCustomAnimationClips";
        }
        #endregion


    }
}
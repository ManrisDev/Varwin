using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Public;
using System;
using System.Collections;
using System.Linq;
using static Varwin.TypeValidationUtils;

namespace Varwin.Core.Behaviours.ConstructorLib
{
    public class InteractionBehaviourHelper : VarwinBehaviourHelper
    {
        public override bool CanAddBehaviour(GameObject gameObject, Type behaviourType)
        {
            if (!base.CanAddBehaviour(gameObject, behaviourType))
            {
                return false;
            }

            return true;
        }

        public override bool IsDisabledBehaviour(GameObject targetGameObject)
        {
            return IsDisabledBehaviour(targetGameObject, BehaviourType.Interaction);
        }
    }
    
    [RequireComponent(typeof(InteractableObjectBehaviour))]
    [RequireComponentInChildren(typeof(Rigidbody))]
    [RequireComponentInChildren(typeof(Collider))]
    [VarwinComponent(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
    public class InteractionBehaviour : ConstructorVarwinBehaviour,
        IGrabStartInteractionAware,
        IGrabEndInteractionAware,
        IUseStartInteractionAware,
        IUseEndInteractionAware,
        ITouchStartInteractionAware,
        ITouchEndInteractionAware
    {
        [VarwinSerializable]
        public bool BackwardCompatibilityIsInitialized { get; set; }
        
        private bool _isGrabbed;
        private bool _isUsed;
        private bool _isTouched;
        private bool _isTeleportArea;
        
        private ControllerInteraction.ControllerHand _grabbedHand;
        private ControllerInteraction.ControllerHand _usedHand;

        private const string TeleportableTag = "TeleportArea";
        private const string NotTeleportableTag = "NotTeleport";
        
        private InteractableObjectBehaviour _interactableObject;
        public InteractableObjectBehaviour InteractableObject
        {
            get
            {
                if (_interactableObject)
                {
                    return _interactableObject;
                }
                
                _interactableObject = gameObject.GetComponent<InteractableObjectBehaviour>();
                return _interactableObject;
            }
        }

        protected override void AwakeOverride()
        {
            base.AwakeOverride();
            _isTeleportArea = transform.Cast<Transform>().Any(child => child.tag.Equals(TeleportableTag));
        }
    
        
        private IEnumerator Start()
        {
            
            yield return new WaitForEndOfFrame();
            if (!BackwardCompatibilityIsInitialized)
            {
                var grabbable = false;
                var teleportable = false;
                var touchable = false;
                var usable = false;

                var prevInteractableBehaviour = Wrapper.GetBehaviour<InteractableBehaviour>();
                if (prevInteractableBehaviour)
                {
                    grabbable = prevInteractableBehaviour.Grabbable;
                    teleportable = prevInteractableBehaviour.Teleportable;
                    touchable = prevInteractableBehaviour.Touchable;
                    usable = prevInteractableBehaviour.Usable;
                }

                var interactableObjectBehaviour = Wrapper.GetGameObject().GetComponentInChildren<InteractableObjectBehaviour>();
                if (interactableObjectBehaviour)
                {
                    grabbable |= interactableObjectBehaviour.IsGrabbable;
                    touchable |= interactableObjectBehaviour.IsTouchable;
                    usable |= interactableObjectBehaviour.IsUsable;
                }

                GrabbableInspector = grabbable;
                TeleportationInspector = teleportable;
                TouchableInspector = touchable;
                UsableInspector = usable;
                
                BackwardCompatibilityIsInitialized = true;
                ObjectController.RefreshInspector();
            }
        }

        #region Enums

        public enum Teleportation
        {
            [Item(English:"can be teleported to",Russian:"можно перемещаться",Chinese:"可以被傳送",Kazakh:"телепортациялануға болады",Korean:"텔레포트를 허용")]
            TeleportEnable,
            [Item(English:"can’t be teleported to",Russian:"нельзя перемещаться",Chinese:"無法傳送到",Kazakh:"телепортациялануға болмайды",Korean:"텔레포트를 허용하지 않음")]
            TeleportDisable,
        }
        
        public enum GrabStatus
        {
            [Item(English:"was grabbed",Russian:"взят в руку",Chinese:"被抓取",Kazakh:"қолға алынды",Korean:"잡힘")]
            Grabbed,
            [Item(English:"was ungrabbed",Russian:"выпущена из руки",Chinese:"被放下",Kazakh:"қолдан босатылды",Korean:"놓음")]
            Ungrabbed,
        }
        
        public enum Grabbing
        {
            [Item(English:"can be grabbed",Russian:"можно взять в руку",Chinese:"可以被抓取",Kazakh:"қолға алуға болады",Korean:"잡을 수 있음")]
            GrabEnable,
            [Item(English:"can’t be grabbed",Russian:"нельзя взять в руку",Chinese:"不能被抓住",Kazakh:"қолға алуға болмайды",Korean:"잡을 수 없음")]
            GrabDisable,
        }
        
        public enum UseStatus
        {
            [Item(English:"started being used",Russian:"начал использоваться",Chinese:"使用中",Kazakh:"пайдаланыла бастады",Korean:"사용을 시작했을 때")]
            Used,
            [Item(English:"ended being used",Russian:"перестал использоваться",Chinese:"使用完畢",Kazakh:"пайдаланылуы тоқтатылды",Korean:"사용을 종료했을 때")]
            UnUsed,
        }
        
        public enum Using
        {
            [Item(English:"can be used",Russian:"можно использовать",Chinese:"能被使用",Kazakh:"пайдалануға болады",Korean:"사용할 수 있음")]
            UseEnable,
            [Item(English:"can’t be used",Russian:"нельзя использовать",Chinese:"無法使用",Kazakh:"пайдалануға болмайды",Korean:"사용할 수 없음")]
            UseDisable,
        }
        
        public enum Touching
        {
            [Item(English:"can be touched",Russian:"можно дотронуться",Chinese:"可以被觸碰",Kazakh:"қол тигізуге болады",Korean:"터치할 수 있음")]
            TouchEnable,
            [Item(English:"can’t be touched",Russian:"нельзя дотронуться",Chinese:"無法觸及",Kazakh:"қол тигізуге болмайды",Korean:"터치할 수 없음")]
            TouchDisable,
        }

        #endregion

        #region Checkers

        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Returns true if the specified object is currently in the hand. Otherwise it returns false.",Russian:"Возвращает истину, если объект находится в руке в данный момент. В противном случае возвращает ложь.",Chinese:"如果指定物件正在被玩家握在手中則回傳為真；反之，回傳為假。",Kazakh:"Егер объект осы сәтте қолда болса, шындықты қайтарады. Олай болмаған жағдайда өтірікті қайтарады.",Korean:"지정한 객체가 현재 손에 들려있을 경우 참(true)을 반환함. 그렇지 않을 경우 거짓(false)을 반환함")]
        [Checker(English:"is in the player's hand",Russian:"находится в руке игрока",Chinese:"正在被玩家握在手中",Kazakh:"ойыншының қолында тұр",Korean:"사용자의 손에 있다면")]
        public bool IsGrabbed()
        {
            return _isGrabbed;
        }
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Returns true if the specified object is currently used by the player. Otherwise it returns false.",Russian:"Возвращает истину, если указанный объект используется игроком в данный момент. В противном случае возвращает ложь.",Chinese:"如果指定物件正在被玩家使用則回傳為真；反之，回傳為假。",Kazakh:"Егер ойыншы объектіні осы сәтте пайдаланып жатса, шындықты қайтарады. Олай болмаған жағдайда өтірікті қайтарады.",Korean:"지정한 객체가 현재 사용자가 사용하고 있을 경우 참(true)을 반환함. 그렇지 않을 경우 거짓(false)을 반환함")]
        [Checker(English:"is used by the player",Russian:"используется игроком",Chinese:"正在被玩家使用",Kazakh:"ойыншы пайдалануда",Korean:"사용자에 의해 사용된다면")]
        public bool IsUsing()
        {
            return _isUsed;
        }
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Returns true if the object is touched at the moment. Otherwise it returns false.",Russian:"Возвращает истину, если до объекта дотрагиваются в данный момент. В противном случае возвращает ложь.",Chinese:"如果指定物件正在被觸碰則回傳為真；反之，回傳為假。",Kazakh:"Егер объектіге осы сәтте тиіп тұрса, шындықты қайтарады. Олай болмаған жағдайда өтірікті қайтарады.",Korean:"객체가 터치되면 참(true)을 반환함. 그렇지 않을 경우 거짓(false)을 반환함")]
        [Checker(English:"touches the player's hand",Russian:"касается руки игрока",Chinese:"正在觸碰玩家的手",Kazakh:"ойыншының қолына тиіп тұр",Korean:"사용자의 손이 터치된다면")]
        public bool IsTouching()
        {
            return _isTouched;
        }

        #endregion
        
        #region Variables

        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Sets whether the player can teleport or walk around the object.",Russian:"Задает, можно ли игроку телепортироваться или ходить по объекту.",Chinese:"設動玩家能否傳送或在物件周圍行走",Kazakh:"Ойыншының телепортациялануына немесе объектімен жүруіне болатын-болмайтынын белгілейді.",Korean:"사용자가 객체의 주변을 걸을지 텔레포트 할지 설정")]
        [Variable(English:" ",Russian:" ",Chinese:" ",Kazakh:" ",Korean:" ")]
        [SourceTypeContainer(typeof(Teleportation))]
        public dynamic TeleportationSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(TeleportationSetter), out Teleportation convertedValue))
                {
                    return;                    
                }

                _isTeleportArea = convertedValue == Teleportation.TeleportEnable;
                var targetTag = convertedValue == Teleportation.TeleportEnable ? TeleportableTag : NotTeleportableTag;
                SetTagForAllChildren(transform, targetTag);

                void SetTagForAllChildren(Transform t, string tag)
                {
                    t.tag = tag;
                    foreach (Transform child in t)
                    {
                        SetTagForAllChildren(child, tag);
                    }
                }
            }
        }

        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Sets whether the player can grab  the object.",Russian:"Задает, можно ли игроку брать объект в руки.",Chinese:"設置玩家是否可以抓取物體。",Kazakh:"Ойыншының объектіні алуына болатын-болмайтынын белгілейді.",Korean:"사용자가 객체를 잡을 수 있는지 설정")]
        [Variable(English:" ",Russian:" ",Chinese:" ",Kazakh:" ",Korean:" ")]
        [SourceTypeContainer(typeof(Grabbing))]
        public dynamic GrabbingSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(GrabbingSetter), out Grabbing convertedValue))
                {
                    return;
                }
                
                InteractableObject.IsGrabbable = convertedValue == Grabbing.GrabEnable;
            }
        }

        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Sets whether or not the player can interact with an object using the mechanics of using it (clicking on the object).",Russian:"Задает, можно ли игроку взаимодействовать с объектом с помощью механики использования (нажатия на объект).",Chinese:"設定玩家是否能透過使用的功能來與物件互動 (點選物件)",Kazakh:"Ойыншының пайдалану механикасы (объектіні басу) арқылы объектімен өзара әрекеттесуіне болатын-болмайтынын белгілейді.",Korean:"사용자가 객체를 클릭하는 방식의 매커니즘을 이용하여 객체와 상호작용 할 수 있는지 설정")]
        [Variable(English:" ",Russian:" ",Chinese:" ",Kazakh:" ",Korean:" ")]
        [SourceTypeContainer(typeof(Using))]
        public dynamic UsingSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(UsingSetter), out Using convertedValue))
                {
                    return;
                }

                InteractableObject.IsUsable = convertedValue == Using.UseEnable;
            }
        }

        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"Sets whether the player can interact with an object using touch mechanics.",Russian:"Задает, можно ли игроку взаимодействовать с объектом с помощью механики касания.",Chinese:"設定玩家是否能透過觸碰的功能來與物件互動",Kazakh:"Ойыншының жанасу механикасы арқылы объектімен өзара әрекеттесуіне болатын-болмайтынын белгілейді.",Korean:"사용자가 터치 메커니즘으로 객체와 상호작용 할 수 있는지 설정")]
        [Variable(English:" ",Russian:" ",Chinese:" ",Kazakh:" ",Korean:" ")]
        [SourceTypeContainer(typeof(Touching))]
        public dynamic TouchingSetter
        {
            set
            {
                if (!ValidateSetterWithLog(this, value, nameof(TouchingSetter), out Touching convertedValue))
                {
                    return;
                }

                InteractableObject.IsTouchable = convertedValue == Touching.TouchEnable;
            }
        }

        [VarwinInspector(English:"Сan teleport",Russian:"Можно перемещаться",Chinese:"可以傳送",Kazakh:"Телепортациялануға болады",Korean:"텔레포트할 수 있음")]
        public bool TeleportationInspector
        {
            get => _isTeleportArea;
            set => TeleportationSetter = value ? Teleportation.TeleportEnable : Teleportation.TeleportDisable;
        }

        [VarwinInspector(English:"Grabbable",Russian:"Можно брать в руку",Chinese:"可抓取",Kazakh:"Қолға алуға болады",Korean:"잡을 수 있음")]
        public bool GrabbableInspector
        {
            get => InteractableObject.IsGrabbable;
            set => InteractableObject.IsGrabbable = value;
        }

        [VarwinInspector(English:"Usable",Russian:"Можно использовать",Chinese:"可使用",Kazakh:"Пайдалануға болады",Korean:"사용할 수 있음")]
        public bool UsableInspector
        {
            get => InteractableObject.IsUsable;
            set => InteractableObject.IsUsable = value;
        }

        [VarwinInspector(English:"Touchable",Russian:"Можно дотронуться",Chinese:"可觸碰",Kazakh:"Қол тигізуге болады",Korean:"터치할 수 있음")]
        public bool TouchableInspector
        {
            get => InteractableObject.IsTouchable;
            set => InteractableObject.IsTouchable = value;
        }

        #endregion

        #region Events

        public delegate void HandInteractionHandler([Parameter(English:"interaction hand",Russian:"рука взаимодействия",Chinese:"手部互動",Kazakh:"өзара әрекеттесу қолы",Korean:"상호작용 하는 손")] ControllerInteraction.ControllerHand hand);
        public delegate void TouchInteractionHandler();

        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"The event is triggered when the player takes the specified object in his hand. The parameters include the object and the hand it was taken with.",Russian:"Событие срабатывает, когда игрок берет в руку указанный объект. В параметры передается объект и рука, которой он был взят.",Chinese:"此事件會在玩家以手拿取指定的物件時觸發，參數包含物件以及拿取物件的那隻手",Kazakh:"Ойыншы көрсетілген объектіні қолына алған кезде оқиға іске қосылады. Параметрлерге объект пен оны алған қол беріледі.",Korean:"이벤트는 사용자가 지정한 객체를 손에 들었을 때 작동(trigger)됩니다. 매개변수(parameter)는 객체와 객체를 집어든 손을 포함합니다 ")]
        [EventGroup("Grab")]
        [LogicEvent(English:"was grabbed",Russian:"взят в руку",Chinese:"被抓取",Kazakh:"қолға алынды",Korean:"잡힘")]
        public event HandInteractionHandler OnObjectGrabbed;
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"The event is triggered when the player takes the specified object in his hand. The parameters include the object and the hand it was taken with.",Russian:"Событие срабатывает, когда игрок берет в руку указанный объект. В параметры передается объект и рука, которой он был взят.",Chinese:"此事件會在玩家以手拿取指定的物件時觸發，參數包含物件以及拿取物件的那隻手",Kazakh:"Ойыншы көрсетілген объектіні қолына алған кезде оқиға іске қосылады. Параметрлерге объект пен оны алған қол беріледі.",Korean:"이벤트는 사용자가 지정한 객체를 손에 들었을 때 작동(trigger)됩니다. 매개변수(parameter)는 객체와 객체를 집어든 손을 포함합니다 ")]
        [EventGroup("Grab")]
        [LogicEvent(English:"was ungrabbed",Russian:"выпущен из руки",Chinese:"被放下",Kazakh:"қолдан босатылды",Korean:"놓음")]
        public event HandInteractionHandler OnObjectUnGrabbed;
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"The event is triggered when the player uses the specified object. The object and the hand it is used with are passed in the parameters.",Russian:"Событие срабатывает, когда игрок использует указанный объект. В параметры передается объект и рука, которой он используется.",Chinese:"當玩家使用指定的物體時會觸發該事件。物件和使用它的手在參數中傳遞。",Kazakh:"Ойыншы көрсетілген объектіні пайдаланған кезде оқиға іске қосылады. Параметрлерге объект пен оны пайдаланатын қол беріледі.",Korean:"플레이어가 지정된 오브젝트를 사용할 때 이벤트가 트리거됩니다. 오브젝트와 함께 사용된 손이 파라미터로 전달됩니다.")]
        [EventGroup("Use")]
        [LogicEvent(English:"started being used",Russian:"начал использоваться",Chinese:"使用中",Kazakh:"пайдаланыла бастады",Korean:"사용을 시작했을 때")]
        public event HandInteractionHandler OnObjectUsed;
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"The event is triggered when the player uses the specified object. The object and the hand it is used with are passed in the parameters.",Russian:"Событие срабатывает, когда игрок использует указанный объект. В параметры передается объект и рука, которой он используется.",Chinese:"當玩家使用指定的物體時會觸發該事件。物件和使用它的手在參數中傳遞。",Kazakh:"Ойыншы көрсетілген объектіні пайдаланған кезде оқиға іске қосылады. Параметрлерге объект пен оны пайдаланатын қол беріледі.",Korean:"플레이어가 지정된 오브젝트를 사용할 때 이벤트가 트리거됩니다. 오브젝트와 함께 사용된 손이 파라미터로 전달됩니다.")]
        [EventGroup("Use")]
        [LogicEvent(English:"ended being used",Russian:"перестал использоваться",Chinese:"使用完畢",Kazakh:"пайдалану тоқтатылды",Korean:"사용을 종료했을 때")]
        public event HandInteractionHandler OnObjectNotUsed; 
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"The event is triggered when the player touches the specified object.",Russian:"Событие срабатывает, когда игрок касается указанного объекта.",Chinese:"此事件於玩家觸碰指定物件時觸發",Kazakh:"Ойыншы көрсетілген объектімен жанасқан кезде оқиға іске қосылады.",Korean:"이벤트는 사용자가 지정한 객체를 터치 했을 때 작동(trigger)됩니다")]
        [EventGroup("Touch")]
        [LogicEvent(English:"started being touched",Russian:"к объекту прикоснулись",Chinese:"開始被觸碰時",Kazakh:"объектіге қол тигізілді",Korean:"터치를 시작했을 때")]
        public event TouchInteractionHandler OnObjectTouched;
        
        [LogicGroup(English:"Interactivity",Russian:"Интерактивность",Chinese:"互動性",Kazakh:"Интерактивтілік",Korean:"상호작용")]
        [LogicTooltip(English:"The event is triggered when the player touches the specified object.",Russian:"Событие срабатывает, когда игрок касается указанного объекта.",Chinese:"此事件於玩家觸碰指定物件時觸發",Kazakh:"Ойыншы көрсетілген объектімен жанасқан кезде оқиға іске қосылады.",Korean:"이벤트는 사용자가 지정한 객체를 터치 했을 때 작동(trigger)됩니다")]
        [EventGroup("Touch")]
        [LogicEvent(English:"ended being touched",Russian:"объекта перестали касаться",Chinese:"結束被觸碰時",Kazakh:"объектімен жанасу тоқтатылды",Korean:"터치를 종료했을 때")]
        public event TouchInteractionHandler OnObjectNotTouched; 
        
        #endregion
        
        public void OnGrabStart(GrabInteractionContext context)
        {
            if (context.Hand is ControllerInteraction.ControllerHand.Left or ControllerInteraction.ControllerHand.Right)
            {
                _isGrabbed = true;
                _grabbedHand = context.Hand;
                OnObjectGrabbed?.Invoke(_grabbedHand);
            }
        }

        public void OnGrabEnd(GrabInteractionContext context)
        {
            _isGrabbed = false;
            OnObjectUnGrabbed?.Invoke(context.Hand);
            _grabbedHand = default;
        }

        public void OnUseStart(UseInteractionContext context)
        {
            if (context.Hand is ControllerInteraction.ControllerHand.Left or ControllerInteraction.ControllerHand.Right)
            {
                _isUsed = true;
                _usedHand = context.Hand;
                OnObjectUsed?.Invoke(context.Hand);
            }
        }

        public void OnUseEnd(UseInteractionContext context)
        {
            _isUsed = false;
            OnObjectNotUsed?.Invoke(context.Hand);
            _usedHand = default;
        }

        public void OnTouchStart(TouchInteractionContext context)
        {
            OnObjectTouched?.Invoke();
            _isTouched = true;
        }

        public void OnTouchEnd(TouchInteractionContext context)
        {
            OnObjectNotTouched?.Invoke();
            _isTouched = false;
        }
    }
}

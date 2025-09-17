using Varwin.PlatformAdapter;

namespace Varwin
{
    /// <summary>
    /// Класс, обрабатывающий получение событий от поинтера.
    /// </summary>
    public class ObjectPointerBehaviour : PointableObject
    {
        public PointerAction PointerAction => _pointerAction;

        /// <summary>
        /// Pointer action для обработки событий поинтера.
        /// </summary>
        private PointerAction _pointerAction;

        /// <summary>
        /// Рука, которая взаимодействует с данным поведением.
        /// Для Desktop, NettleDesk и AR платформ это дефолтная (правая) рука.
        /// Для XR и NettleDesk Stylus значение выставляется в рантайме.
        /// </summary>
        private ControllerInteraction.ControllerHand _interactingHand = ControllerInteraction.DefaultDesktopIterationHand;

        /// <summary>
        /// Инициализация поведения.
        /// </summary>
        /// <param name="pointerAction">Соответствующий поведению PointerAction.</param>
        public void Init(PointerAction pointerAction)
        {
            _pointerAction = pointerAction;
        }

        /// <summary>
        /// Установить текущую интерактивную руку для поведения.
        /// </summary>
        /// <param name="hand">Рука, которая взаимодействует с поведением в данным момент.</param>
        public void SetHoveringHand(ControllerInteraction.ControllerHand hand)
        {
            _interactingHand = hand;
        }

        /// <summary>
        /// Передача события PointerIn в PointerAction.
        /// </summary>
        public override void OnPointerIn() => _pointerAction.OnPointerIn?.Invoke(_interactingHand);

        /// <summary>
        /// Передача события PointerOut в PointerAction.
        /// </summary>
        public override void OnPointerOut() => _pointerAction.OnPointerOut?.Invoke(_interactingHand);

        /// <summary>
        /// Передача события PointerDown в PointerAction.
        /// </summary>
        public override void OnPointerDown() => _pointerAction.OnPointerDown?.Invoke(_interactingHand);

        /// <summary>
        /// Передача события PointerUp в PointerAction.
        /// </summary>
        public override void OnPointerUp() => _pointerAction.OnPointerUp?.Invoke(_interactingHand);

        /// <summary>
        /// Передача события PointerClick в PointerAction.
        /// </summary>
        public override void OnPointerUpAsButton() => _pointerAction.OnPointerClick?.Invoke(_interactingHand);
    }
}

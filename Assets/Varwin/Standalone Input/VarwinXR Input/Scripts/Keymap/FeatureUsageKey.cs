namespace Varwin.XR
{
    /// <summary>
    /// Ключ элемента управления.
    /// </summary>
    public enum FeatureUsageKey
    {
        /// <summary>
        /// Первичная ось (триггер).
        /// </summary>
        PrimaryAxis2D,

        /// <summary>
        /// Нажатие на первичную ось (джойстик).
        /// </summary>
        PrimaryAxis2DClick,

        /// <summary>
        /// Вторичная ось (триггер).
        /// </summary>
        SecondaryAxis2D,

        /// <summary>
        /// Нажатие на вторичную ось (джойстик).
        /// </summary>
        SecondaryAxis2DClick,

        /// <summary>
        /// Первая кнопка.
        /// </summary>
        ButtonOne,

        /// <summary>
        /// Вторая кнопка.
        /// </summary>
        ButtonTwo,
        
        /// <summary>
        /// Кнопка меню.
        /// </summary>
        MenuButton,
        
        /// <summary>
        /// Кнопка захвата.
        /// </summary>
        Grip,
        
        /// <summary>
        /// Триггер.
        /// </summary>
        Trigger
    }
}
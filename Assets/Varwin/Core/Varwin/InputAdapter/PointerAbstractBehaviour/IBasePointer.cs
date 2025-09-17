namespace Varwin.PlatformAdapter
{
    public interface IBasePointer
    {
        /// <summary>
        /// Check if pointer can be shown
        /// </summary>
        bool CanToggle();

        /// <summary>
        /// Check if pointer can be pressed
        /// </summary>
        bool CanPress();

        /// <summary>
        /// Check if pointer can be released
        /// </summary>
        bool CanRelease();

        /// <summary>
        /// Turn rendering on/off
        /// </summary>
        void Toggle(bool value);

        /// <summary>
        /// Turn rendering on/off
        /// </summary>
        void Toggle();

        /// <summary>
        /// Pointer button pressed
        /// </summary>
        void Press();

        /// <summary>
        /// Pointer button released
        /// </summary>
        void Release();

        /// <summary>
        /// Pointer initialization
        /// </summary>
        void Init();

        /// <summary>
        /// Update pointer's state for the current frame
        /// </summary>
        void UpdateState();

        /// <summary>
        /// Returns whether pointer is active or not
        /// </summary>
        /// <returns></returns>
        bool IsActive();
    }
}

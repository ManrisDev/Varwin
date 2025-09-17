namespace Varwin.Editor
{
    /// <summary>
    /// Объект, описывающий состояние фичи SDK.
    /// </summary>
    public class SdkFeature
    {
        public bool Enabled;

        /// <summary>
        /// Является ли фича включенной.
        /// </summary>
        /// <param name="feature">Объект фичи.</param>
        /// <returns>true - фича включена, false - фича отключена.</returns>
        public static implicit operator bool(SdkFeature feature) => feature is { Enabled: true };
    }
}

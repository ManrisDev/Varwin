using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Локализуемый Renderer.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class LocalizedRenderer : LocalizedComponent<Texture>
    {
        /// <summary>
        /// Целевой компонент.
        /// </summary>
        private Renderer _renderer;

        /// <summary>
        /// При получении перевода.
        /// </summary>
        /// <param name="value">Значении с системной локалью.</param>
        public override void OnValueReceived(Texture value)
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer)
            {
                _renderer.sharedMaterial.mainTexture = value;
            }
        }
    }
}
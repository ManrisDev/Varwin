using System;
using UnityEngine;
using UnityEngine.UI;

namespace Varwin.Public
{
    /// <summary>
    /// Локализованная картинка.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class LocalizedImage : LocalizedComponent<Sprite>
    {
        /// <summary>
        /// Целевой компонент.
        /// </summary>
        private Image _image;

        /// <summary>
        /// При получении перевода.
        /// </summary>
        /// <param name="value">Значении с системной локалью.</param>
        public override void OnValueReceived(Sprite value)
        {
            _image = GetComponent<Image>();
            if (_image)
            {
                _image.sprite = value;
            }
        }
    }
}
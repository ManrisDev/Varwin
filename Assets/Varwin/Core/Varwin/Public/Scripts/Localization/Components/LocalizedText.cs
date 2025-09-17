using UnityEngine;
using UnityEngine.UI;

namespace Varwin.Public
{
    /// <summary>
    /// Локализуемый текст.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : LocalizedComponent<string>
    {
        /// <summary>
        /// Целевой компонент.
        /// </summary>
        private Text _text;

        /// <summary>
        /// При получении перевода.
        /// </summary>
        /// <param name="value">Значении с системной локалью.</param>
        public override void OnValueReceived(string value)
        {
            _text = GetComponent<Text>();
            if (_text)
            {
                _text.text = value;
            }
        }
    }
}
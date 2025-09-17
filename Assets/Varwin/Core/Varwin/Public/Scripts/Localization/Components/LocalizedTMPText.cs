using TMPro;
using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Локализуемый TMP_Text.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedTMPText : LocalizedComponent<string>
    {
        /// <summary>
        /// Целевой компонент.
        /// </summary>
        private TMP_Text _text;

        /// <summary>
        /// При получении перевода.
        /// </summary>
        /// <param name="value">Значении с системной локалью.</param>
        public override void OnValueReceived(string value)
        {
            _text = GetComponent<TMP_Text>();
            if (_text)
            {
                _text.text = value;
            }
        }
    }
}
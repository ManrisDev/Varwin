using UnityEngine;

namespace Varwin.Public
{
    /// <summary>
    /// Локализуемый источник звука.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class LocalizedAudioSource : LocalizedComponent<AudioClip>
    {
        /// <summary>
        /// Целевой компонент.
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        /// При получении перевода.
        /// </summary>
        /// <param name="value">Значении с системной локалью.</param>
        public override void OnValueReceived(AudioClip value)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource)
            {
                _audioSource.clip = value;
            }
        }
    }
}
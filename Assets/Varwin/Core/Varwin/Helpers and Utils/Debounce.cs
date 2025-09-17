using UnityEngine;

namespace Varwin
{
    public class Debounce
    {
        private readonly float _debounceTime;
        private float _currentTime;

        private bool _canBeReset;

        public Debounce(float time)
        {
            _debounceTime = time;
            _canBeReset = true;
        }

        public void Update()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime < _debounceTime)
            {
                return;
            }

            _canBeReset = true;
        }

        public void Reset()
        {
            _currentTime = 0f;
            _canBeReset = false;
        }

        public bool CanReset() => _canBeReset;
    }
}

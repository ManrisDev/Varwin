using System.Collections;

namespace Varwin
{
    public class CoroutineWrapper : IEnumerator
    {
        private bool _isStopped = false;
        private bool _isPaused = false;

        public object Current => !_isPaused && !_isStopped ? _coroutine.Current : null;
        private readonly IEnumerator _coroutine;

        public CoroutineWrapper(IEnumerator enumerator)
        {
            _coroutine = enumerator;
        }
        
        public void Stop()
        {
            _isStopped = true;
        }

        public void Pause()
        {
            _isPaused = true;
        }
        
        public void Play()
        {
            _isPaused = false;
        }

        public bool MoveNext()
        {
            if (_isPaused)
            {
                return true;
            }
            
            return !_isStopped && _coroutine.MoveNext();
        }

        public void Reset()
        {
            _isPaused = false;
            _isStopped = false;
            _coroutine.Reset();
        }
    }
}
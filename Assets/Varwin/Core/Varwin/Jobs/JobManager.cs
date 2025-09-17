using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin.Jobs
{
    /// <summary>
    /// Класс, управляющий выполнением работ.
    /// </summary>
    public class JobManager : MonoBehaviour
    {
        /// <summary>
        /// Инстанс класса.
        /// </summary>
        private static JobManager _instance;

        /// <summary>
        /// Условия ожидания.
        /// Т.к. задачи используются в подписках, то для корректной работы они должы запускаться после загрузки сцены.
        /// </summary>
        private readonly WaitWhile _waitCondition = new(() =>
        {
#if VARWINCLIENT
            return LoaderAdapter.IsLoadingProcess || ProjectData.CurrentScene == null;
#endif
            return ProjectData.CurrentScene == null;
        });

        /// <summary>
        /// Очередь задач.
        /// </summary>
        private readonly Queue<IJob> _jobs = new();

        /// <summary>
        /// Корутина, в которой выполняются работы.
        /// </summary>
        private Coroutine _workCoroutine;

        /// <summary>
        /// Получить инстанс класса.
        /// </summary>
        /// <returns>Инстанс класса.</returns>
        private static JobManager GetInstance()
        {
            if (_instance)
            {
                return _instance;
            }

            var container = new GameObject("Job Manager");
            DontDestroyOnLoad(container);

            return _instance = container.AddComponent<JobManager>();
        }

        /// <summary>
        /// Unity Singletone.
        /// </summary>
        private void Awake()
        {
            if (_instance)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Добавить работу к выполнению.
        /// </summary>
        /// <param name="job">Экземпляр задачи.</param>
        public static void AddJob(IJob job)
        {
            if (job == null)
            {
                return;
            }

            var instance = GetInstance();

            instance._jobs.Enqueue(job);
            instance._workCoroutine ??= instance.StartCoroutine(instance.Work());
        }

        /// <summary>
        /// Ожидание и выполнение работ из очереди.
        /// </summary>
        private IEnumerator Work()
        {
            while (_jobs.TryDequeue(out var job))
            {
                yield return _waitCondition;

                job.Execute();
            }

            _workCoroutine = null;
        }
    }
}
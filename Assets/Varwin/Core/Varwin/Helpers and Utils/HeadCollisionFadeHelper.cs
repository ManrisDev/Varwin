using System;
using System.Collections.Generic;
using UnityEngine;

namespace Varwin
{
    /// <summary>
    /// Класс-хэлпер для добавления и удаления коллайдеров, коллизия с которыми будет игнорироваться при затемнении головы.
    /// </summary>
    public static class HeadCollisionFadeHelper
    {
        /// <summary>
        /// Список коллайдеров, с которыми пересекается коллайдер головы.
        /// </summary>
        public static IEnumerable<Collider> CollidingColliders;

        /// <summary>
        /// Затемняется ли голова.
        /// </summary>
        public static bool IsHeadFading;

        /// <summary>
        /// Событие, вызываемое в момент начала затемнения головы.
        /// </summary>
        public static event Action<Collider[]> HeadCollisionStarted;

        /// <summary>
        /// Событие, вызываемое в момент завершения затемнения головы.
        /// </summary>
        public static event Action HeadCollisionEnded;

        /// <summary>
        /// Список игнорируемых коллайдеров.
        /// </summary>
        private static readonly HashSet<Collider> HeadCollisionFadeIgnoredColliders = new();

        /// <summary>
        /// Подписка на загрузку сцены.
        /// </summary>
        static HeadCollisionFadeHelper()
        {
            ProjectData.SceneCleared += Clear;
        }

        /// <summary>
        /// Очистка.
        /// </summary>
        private static void Clear()
        {
            HeadCollisionFadeIgnoredColliders.Clear();
            IsHeadFading = false;
            CollidingColliders = null;
            HeadCollisionStarted = null;
            HeadCollisionEnded = null;
        }

        /// <summary>
        /// Проверка, является ли коллайдер игнорируемым.
        /// </summary>
        /// <param name="collider">Коллайдер.</param>
        /// <returns>true - коллайдер нужно игнорировать. false - коллайдер не нужно игнорировать.</returns>
        public static bool IsIgnoredCollider(Collider collider)
        {
            return HeadCollisionFadeIgnoredColliders.Contains(collider);
        }

        /// <summary>
        /// Добавить коллайдер к списку игнорируемых коллайдеров.
        /// </summary>
        /// <param name="collider">Коллайдер.</param>
        public static void AddColliderToIgnore(Collider collider)
        {
            HeadCollisionFadeIgnoredColliders.Add(collider);
        }

        /// <summary>
        /// Удалить коллайдер из списка игнорируемых коллайдеров.
        /// </summary>
        /// <param name="collider">Коллайдер.</param>
        public static void RemoveIgnoredCollider(Collider collider)
        {
            if (HeadCollisionFadeIgnoredColliders.Contains(collider))
            {
                HeadCollisionFadeIgnoredColliders.Remove(collider);
            }
        }

        /// <summary>
        /// Вызвать событие начала затемнения головы.
        /// </summary>
        /// <param name="colliders">Список коллайдеров, с которыми пересекается коллайдер головы в момент события.</param>
        public static void InvokeHeadCollisionStartedEvent(Collider[] colliders) => HeadCollisionStarted?.Invoke(colliders);

        /// <summary>
        /// Вызвать событие завершения затемнения головы.
        /// </summary>
        public static void InvokeHeadCollisionEndedEvent() => HeadCollisionEnded?.Invoke();
    }
}
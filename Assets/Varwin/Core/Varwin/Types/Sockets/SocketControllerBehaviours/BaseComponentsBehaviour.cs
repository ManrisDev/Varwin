using UnityEngine;
using Varwin.PlatformAdapter;
using Varwin.Public;

namespace Varwin.SocketLibrary
{
    /// <summary>
    /// Базовые компоненты.
    /// </summary>
    public abstract class BaseComponentsBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Контейнер идентификатора.
        /// </summary>
        private ObjectId _objectId;

        /// <summary>
        /// Идентификатор.
        /// </summary>
        public int Id => gameObject.GetWrapper().GetInstanceId();

        /// <summary>
        /// Твердое тело.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// Твердое тело.
        /// </summary>
        public Rigidbody Rigidbody
        {
            get
            {
                if (!_rigidbody)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }

        /// <summary>
        /// Компонент взаимодействий.
        /// </summary>
        private InteractableObjectBehaviour _interactableObjectBehaviour;

        /// <summary>
        /// Компонент взаимодействий.
        /// </summary>
        public InteractableObjectBehaviour InteractableObjectBehaviour
        {
            get
            {
                if (!_interactableObjectBehaviour)
                {
                    _interactableObjectBehaviour = GetComponent<InteractableObjectBehaviour>();
                }

                return _interactableObjectBehaviour;
            }
        }

        /// <summary>
        /// Подсветка объекта.
        /// </summary>
        private Highlighter _highlighter;

        /// <summary>
        /// Подсветка объекта.
        /// </summary>
        public Highlighter Highlighter
        {
            get
            {
                if (!_highlighter)
                {
                    _highlighter = GetComponentInChildren<Highlighter>(true);
                }

                return _highlighter;
            }
        }

        /// <summary>
        /// Включить подсветку при подсоединении.
        /// </summary>
        public void SetJoinHighlight()
        {
            Highlighter.IsEnabled = true;
            Highlighter.SetConfig(HighlightAdapter.Instance.Configs.JointHighlight, null, false);
        }

        /// <summary>
        /// Выключить подсветку при подсоединении.
        /// </summary>
        public void ResetHighlight()
        {
            var isDeleting = gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true;
            if (isDeleting)
            {
                Highlighter.IsEnabled = false;
                return;
            }

            var rootController = gameObject.GetRootInputController();
            if (rootController != null && rootController.InteractObject.IsTouching() && !rootController.InteractObject.IsGrabbed())
            {
                rootController.SetupHighlightWithConfig(true, rootController.DefaultHighlightConfig);
            }
            else
            {
                Highlighter.IsEnabled = false;
            }
        }
    }
}
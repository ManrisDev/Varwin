using System;
using System.Collections.Generic;
using System.Linq;
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
        [Obsolete("Use Highlighters instead")]
        private Highlighter _highlighter;

        /// <summary>
        /// Подсветка объекта.
        /// </summary>
        [Obsolete("Use Highlighters instead")]
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
        /// Подсветка списка объектов.
        /// </summary>
        private List<Highlighter> _highlighters;
        
        /// <summary>
        /// Подсветка списка объектов.
        /// </summary>
        public List<Highlighter> Highlighters
        {
            get
            {
                if (_highlighters != null)
                {
                    return _highlighters;
                }
                
                _highlighters = new List<Highlighter>();
#if VARWINCLIENT                
                var descendants = ObjectController.LockParent.Descendants;
                descendants.Add(ObjectController.LockParent);
                foreach (var descendant in descendants)
                {
                    var highlighter = descendant.gameObject.GetComponentInChildren<Highlighter>(true);
                    if (!highlighter)
                    {
                        continue;
                    }
                        
                    _highlighters.Add(highlighter);
                }
#else
                _highlighters.Add(gameObject.GetComponentInChildren<Highlighter>());          
#endif
                return _highlighters;
            }
        }

        /// <summary>
        /// Контроллер объекта.
        /// </summary>
        private ObjectController _objectController;
        
        /// <summary>
        /// Контроллер объекта.
        /// </summary>
        public ObjectController ObjectController => _objectController ??= gameObject.GetWrapper().GetObjectController();

        /// <summary>
        /// Включить подсветку при подсоединении.
        /// </summary>
        public void SetJoinHighlight()
        {
            foreach (var highlighter in Highlighters)
            {
                if (!highlighter)
                {
                    continue;
                }
                
                highlighter.IsEnabled = true;
                highlighter.SetConfig(HighlightAdapter.Instance.Configs.JointHighlight, null, false);
            }
        }

        /// <summary>
        /// Выключить подсветку при подсоединении.
        /// </summary>
        public void ResetHighlight()
        {
            var isDeleting = gameObject.GetWrapper()?.GetObjectController()?.IsDeleted ?? true;
            if (isDeleting)
            {
                foreach (var highlighter in Highlighters.Where(highlighter => highlighter))
                {
                    highlighter.IsEnabled = false;
                }

                return;
            }

            foreach (var highlighter in Highlighters.Where(highlighter => highlighter))
            {
                var rootController = highlighter.gameObject.GetRootInputController();
                if (rootController != null && rootController.InteractObject.IsTouching() && !rootController.InteractObject.IsGrabbed())
                {
                    rootController.SetupHighlightWithConfig(true, rootController.DefaultHighlightConfig);
                }
                else
                {
                    highlighter.IsEnabled = false;
                }
            }
        }
    }
}
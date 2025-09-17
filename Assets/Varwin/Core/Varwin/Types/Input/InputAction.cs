using UnityEngine;
using UnityEngine.SceneManagement;
using Varwin.PlatformAdapter;

namespace Varwin
{
    public class InputAction
    {
        public ObjectInteraction.InteractObject InteractObject;
        protected ObjectController ObjectController;
        protected GameObject GameObject;
        protected InputController InputController;
        protected bool IsRootGameObject => GameObject == ObjectController.RootGameObject;

        public virtual bool IsEnabled { get; }
        
        protected InputAction(ObjectController objectController, GameObject gameObject, ObjectInteraction.InteractObject interactObject, InputController inputController)
        {
            ObjectController = objectController;
            GameObject = gameObject;
            InteractObject = interactObject;
            InputController = inputController;

            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
        {
            Destroy();
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public void Destroy()
        {
            OnDestroy();
            
            InteractObject = null;
            ObjectController = null;
            GameObject = null;
            InputController = null;
        }

        public virtual void DisableViewInput()
        {
        }

        public virtual void EnableViewInput()
        {
        }

        protected virtual void DisableEditorInput()
        {
        }

        protected virtual void EnableEditorInput()
        {
        }

        public void GameModeChanged(GameMode newGameMode)
        {
            if (newGameMode == GameMode.Edit)
            {
                DisableViewInput();
                EnableEditorInput();
            }
            else
            {
                DisableEditorInput();
                EnableViewInput();
            }
        }

        public void PlatformModeChanged(PlatformMode newPlatformMode)
        {
            GameModeChanged(ProjectData.GameMode);
        }
        
        protected virtual void OnDestroy()
        {
        }

        protected GameObject GetHandGameObject(ControllerInteraction.ControllerHand hand)
        {
            return InputAdapter.Instance?.PlayerController.Nodes.GetControllerReference(hand).GameObject;
        }
    }
}
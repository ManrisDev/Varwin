using UnityEngine;

namespace Varwin.Public
{
    public static class ActivityEx
    {
        public static void SetActivity(this IWrapperAware iWrapperAware, bool isActive)
        {
            Wrapper wrapper = iWrapperAware.Wrapper();
            SetActivity(wrapper.GetGameObject(), wrapper, isActive);
        }
        
        public static void SetActivity(this Wrapper wrapper, bool isActive)
        {
            SetActivity(wrapper.GetGameObject(), wrapper, isActive);
        }
        
        public static void SetActivity(this GameObject gameObject, bool isActive)
        {
            SetActivity(gameObject, gameObject.GetWrapper(), isActive);
        }

        private static void SetActivity(GameObject gameObject, Wrapper wrapper, bool isActive)
        {
            InputController inputController = wrapper.GetInputController(gameObject);
            if (!isActive && inputController != null && inputController.IsGrabbed())
            {
                inputController.DropGrabbedObjectAndDeactivate();
                return;
            }
            
            ObjectController objectController = wrapper.GetObjectController();
            if (objectController)
            {
                objectController.SetActive(isActive);
            }
            else
            {
                if (gameObject)
                {
                    gameObject.SetActive(isActive);
                }
            }
        }

    }
}
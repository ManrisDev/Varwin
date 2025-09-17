using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if VARWINCLIENT
using Varwin.UI.VRErrorManager;
#endif
namespace Varwin
{
    public static class ObjectControllerUtils
    {
        public static Action<List<ObjectController>> ObjectsCantBeDeleted;
        public static event Action<List<ObjectController>> ObjectsDeleted;

        public static event Func<List<ObjectController>, bool> ObjectsDeleteRequest;
        
        public static event Action DeselectEventHandler;

        [Obsolete]
        public static IEnumerator DeleteObjectsWithChildren(IEnumerable<ObjectController> objectControllers)
        {
            var removableControllers = new List<ObjectController>();

            foreach (ObjectController controller in objectControllers)
            {
                if (!controller.IsEmbedded && !controller.IsVirtualObject && !controller.IsSceneTemplateObject && !removableControllers.Contains(controller))
                {
                    removableControllers.Add(controller);
                }

                IEnumerable<ObjectController> removableChildren = controller.Descendants.Where(child => !removableControllers.Contains(child)).ToList();
                foreach (var removableChild in removableChildren)
                {
                    if (!removableControllers.Contains(removableChild))
                    {
                        removableControllers.Add(removableChild);
                    }
                }

                yield return null;
            }

            if (removableControllers.Count == 0)
            {
                yield break;
            }

            removableControllers.Sort((a, b) => GetObjectControllerLayer(a) < GetObjectControllerLayer(b) ? -1 : 1);
            
            if (removableControllers.Any(GameStateData.ObjectIsLocked))
            {
                ObjectsCantBeDeleted?.Invoke(removableControllers);
            }
            else
            {
                ForceDeleteObjectsWithChildren(removableControllers);
            }
        }
        
        public static void DeleteObjects(IEnumerable<ObjectController> objectControllers)
        {
            var removableControllers = new List<ObjectController>();

            foreach (ObjectController controller in objectControllers)
            {
                if (!controller.IsEmbedded && !controller.IsSceneTemplateObject && !removableControllers.Contains(controller))
                {
                    removableControllers.Add(controller);
                }

                IEnumerable<ObjectController> removableChildren = controller.Descendants.Where(child => !removableControllers.Contains(child)).ToList();
                foreach (var removableChild in removableChildren)
                {
                    if (!removableControllers.Contains(removableChild))
                    {
                        removableControllers.Add(removableChild);
                    }
                }
            }

            if (removableControllers.Count > 0)
            {
                removableControllers.Sort((a, b) => GetObjectControllerLayer(a) < GetObjectControllerLayer(b) ? -1 : 1);

                if (removableControllers.Any(GameStateData.ObjectIsLocked))
                {
                    ObjectsCantBeDeleted?.Invoke(removableControllers);
                }
                else
                {
                    ForceDeleteObjectsWithChildren(removableControllers);
                }
            }
        }

        private static int GetObjectControllerLayer(ObjectController controller)
        {
            var layer = 0;
            var parent = controller.Parent;
            while (parent != null)
            {
                parent = parent.Parent;
                layer++;
            }

            return layer;
        }

        public static void ForceDeleteObjectsWithChildren(List<ObjectController> objectControllersForRemove)
        {
            if (ObjectsDeleteRequest != null && ObjectsDeleteRequest(objectControllersForRemove))
            {
                ObjectsDeleted?.Invoke(objectControllersForRemove);
            }
        }

        private static void ShowError(string errorMessage)
        {
#if VARWINCLIENT
            CoreErrorManager.Error(new Exception(errorMessage));
            if (VRErrorManager.Instance)
            {
                VRErrorManager.Instance.Show(errorMessage);
            }
#endif
        }

        public static void DeselectObjects()
        {
            DeselectEventHandler?.Invoke();
        }
    }
}
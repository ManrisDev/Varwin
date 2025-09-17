using UnityEngine;
using Varwin.DesktopPlayer;
using Varwin.PlatformAdapter;
 
namespace Varwin.DesktopInput
{
    public class DesktopControllerInteractionComponent : MonoBehaviour
    {
        private DesktopPlayerInteractionController _playerInteractionController;

        private void Awake()
        {
            _playerInteractionController = GetComponentInParent<DesktopPlayerInteractionController>();
            
            InputAdapter.Instance.PlayerController.Nodes.RightHand.SetNode(gameObject);
            InputAdapter.Instance.PlayerController.Nodes.LeftHand.SetNode(gameObject);
        }

        public GameObject GetGrabbedObject()
        {
            return _playerInteractionController.GetGrabbedObject();
        }
    }
}

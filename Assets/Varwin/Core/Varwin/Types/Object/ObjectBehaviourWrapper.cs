using UnityEngine;

namespace Varwin
{
    public class ObjectBehaviourWrapper : MonoBehaviour
    {
        [SerializeField]
        public ObjectController OwdObjectController;

        public void OnClick()
        {
            Debug.Log($"Pointer click to {OwdObjectController.Name}");
        }
    }
}

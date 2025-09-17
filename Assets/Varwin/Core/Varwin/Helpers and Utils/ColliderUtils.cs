using UnityEngine;

namespace Varwin
{
    public class ColliderUtils
    {
        public static void SetupBoxColliderByBounds(BoxCollider boxCollider)
        {
            var gameObject = boxCollider.gameObject;
            
            var startPos = gameObject.transform.position;
            var startScale = gameObject.transform.localScale;
            var startRotation = gameObject.transform.rotation;

            gameObject.transform.position = Vector3.zero;
            gameObject.transform.localScale = Vector3.one;
            gameObject.transform.rotation = Quaternion.identity;
            
            Bounds bounds = BoundsUtils.GetBounds(gameObject.transform, true, false);
            
            boxCollider.center = bounds.center;
            boxCollider.size = bounds.size;

            gameObject.transform.position = startPos;
            gameObject.transform.localScale = startScale;
            gameObject.transform.rotation = startRotation;
        }
    }
}
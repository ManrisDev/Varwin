using UnityEngine;
using Varwin.Models.Data;

namespace Varwin
{
    public class ObjectTransform : MonoBehaviour
    {
        public TransformDto GetTransform()
        {
            var objectId = gameObject.GetComponent<ObjectId>();
            if (!objectId)
            {
                return null;
            }

            var transformDt = gameObject.transform.ToTransformDT();
            return new TransformDto {Id = objectId.Id, Transform = transformDt};
        }
    }
}
#if !NET_STANDARD_2_0  
using UnityEngine;

namespace Varwin
{
    public static class Dynamics
    {
        public static GameObject GetGameObject(dynamic target)
        {
            switch (target)
            {
                case Wrapper targetWrapper:
                    return targetWrapper.GetGameObject();

                    break;
                case GameObject targetObject:
                    return targetObject;

                    break;
                case Component targetComponent:
                    return targetComponent.gameObject;

                    break;
            }

            return null;
        }

        public static Transform GetTransform(dynamic target)
        {
            return GetGameObject(target)?.transform;
        }
    }
}
#endif
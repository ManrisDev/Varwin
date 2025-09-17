using UnityEngine;

namespace Varwin
{
    public interface IPlayerController
    {
        Quaternion Rotation { get; set; }
        
        Vector3 Position { get; set; }
        
        void SetPosition(Vector3 position);
        
        void SetRotation(Quaternion rotation);
        
        void CopyTransform(Transform targetTransform);
    }
}
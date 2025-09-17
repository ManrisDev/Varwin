using UnityEngine;
using UnityEngine.Serialization;

namespace Varwin.PlatformAdapter
{
    public class DistancePointerSettings : ScriptableObject
    {
        [SerializeField] private float _distance = 0.25f;
        [SerializeField] private LayerMask _ignoreLayerMask = 8708;

        [Space]
        public Material Material;
        
        public float StartWidth = 0.003f;
        public float EndWidth = 0.002f;
        public Color StartColor = new Color(0.665f, 1f, 1f);
        public Color EndColor = Color.cyan;
        
        public bool AlwaysDrawRay = false;

        public float Distance
        {
            get => _distance;
            set => _distance = value;
        }

        public LayerMask LayerMask
        {
            get => ~_ignoreLayerMask;
            set => _ignoreLayerMask = ~value;
        }

        public LineRenderer CreateRenderer(GameObject gameObject)
        {
            var result = gameObject.GetComponent<LineRenderer>();
            
            if(!result)
            {
                result = gameObject.AddComponent<LineRenderer>();
            }

            Material.SetFloat("_Cull", 0);
            result.material = Material;

            result.startWidth = StartWidth;
            result.endWidth = EndWidth;

            result.startColor = StartColor;
            result.endColor = EndColor;

            return result;
        }
    }
}
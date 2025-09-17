using System.Linq;
using UnityEngine;

namespace Varwin
{
    public static class TransformEx
    {
        public static Bounds CalculateBounds(this Transform target)
        {
            var renderers = target.gameObject.GetComponentsInChildren<Renderer>();
            var rectTransforms = target.gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.GetComponent<CanvasRenderer>());

            var result = new Bounds(target.position, Vector3.zero);

            foreach (var renderer in renderers)
            {
                result.Encapsulate(renderer.bounds);
            }

            foreach (var rectTransform in rectTransforms)
            {
                var corners = new Vector3[4];
                rectTransform.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    result.Encapsulate(corner);
                }
            }

            return result;
        }
    }
}
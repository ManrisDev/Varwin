using UnityEngine;

namespace Core.Varwin
{
    public static class RendererEx
    {
        public static MaterialPropertyBlock GetMaterialPropertyBlock(this Renderer renderer, MaterialPropertyBlock existingBlock = null, int materialIndex = 0)
        {
            var propertyBlock = existingBlock ?? new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock, materialIndex);

            return propertyBlock;
        }
    }
}
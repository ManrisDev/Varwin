using UnityEngine;
using Varwin.XR;

namespace Varwin
{
    /// <summary>
    /// Контроллер виньетки.
    /// </summary>
    public class VarwinXRVignette : MonoBehaviour
    {
        /// <summary>
        /// Целевой объект отрисовки.
        /// </summary>
        public MeshRenderer Renderer;

        /// <summary>
        /// Поля для материала.
        /// </summary>
        private MaterialPropertyBlock _propertyBlock;
        
        /// <summary>
        /// Создание материала.
        /// </summary>
        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
        }

        /// <summary>
        /// Включение виньетки при активации.
        /// </summary>
        private void OnEnable()
        {
            Renderer.enabled = true;
        }

        /// <summary>
        /// Выключение виньетки при деактивации.
        /// </summary>
        private void OnDisable()
        {
            Renderer.enabled = false;
        }

        /// <summary>
        /// Задать силу виньетки.
        /// </summary>
        /// <param name="force"></param>
        public void SetForce(float force)
        {
            _propertyBlock.SetFloat("_Force", force);
            Renderer.SetPropertyBlock(_propertyBlock);
        }
        
        /// <summary>
        /// Задать тип виньетки.
        /// </summary>
        /// <param name="type">Тип.</param>
        public void SetType(VignetteType type)
        {
            _propertyBlock.SetFloat("_Type", (float)type);
            Renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}

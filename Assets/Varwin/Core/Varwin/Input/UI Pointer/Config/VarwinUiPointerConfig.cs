using UnityEngine;

namespace Varwin
{
    [CreateAssetMenu(fileName = "UI Pointer Config", menuName = "Varwin/Configs/UI Pointer Config")]
    public class VarwinUiPointerConfig : ScriptableObject
    {
        /// <summary>
        /// Цвет указки в обычном состоянии.
        /// </summary>
        public Color IdleColor = new(0.23f, 1f, 0.23f);

        /// <summary>
        /// Цвет указки в нажатом состоянии.
        /// </summary>
        public Color PressedColor = new(0.74f, 1f, 0.74f);

        /// <summary>
        /// Материал указки.
        /// </summary>
        public Material LineMaterial;

        /// <summary>
        /// Стартовая толщина линии.
        /// </summary>
        public float StartWidth = 0.003f;

        /// <summary>
        /// Конечная толщина линии.
        /// </summary>
        public float EndWidth = 0.002f;
    }
}
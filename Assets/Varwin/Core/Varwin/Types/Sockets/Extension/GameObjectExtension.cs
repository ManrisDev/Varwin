using UnityEngine;

namespace Varwin.SocketLibrary.Extension
{
    /// <summary>
    /// Расширение базового GameObject.
    /// </summary>
    public static class GameObjectExtension
    {
        /// <summary>
        /// Материал превью.
        /// </summary>
        private static Material _previewMaterial = GetPreviewMaterial();

        /// <summary>
        /// Получение превью объекта.
        /// </summary>
        /// <param name="gameObject">Исходный объект.</param>
        /// <returns>Новый объект-превью.</returns>
        public static GameObject GetPreviewObject(this GameObject gameObject)
        {
            return GetPreviewObject(gameObject, null, null);
        }

        /// <summary>
        /// Получение превью объекта.
        /// </summary>
        /// <param name="source">Исходный объект.</param>
        /// <param name="parent">Родитель.</param>
        /// <returns>Новый объект-превью.</returns>
        private static GameObject GetPreviewObject(GameObject source, Transform parent, PreviewBehaviour previewBehaviour)
        {
            var resultObject = new GameObject($"Preview_{source.name}");

            if (!previewBehaviour)
            {
                previewBehaviour = resultObject.AddComponent<PreviewBehaviour>();
            }
            
            resultObject.SetActive(source.activeSelf);

            AddMeshFilter(source, resultObject);
            AddRenderer(source, resultObject, previewBehaviour);

            TranslateLocalTransformations(source.transform, resultObject.transform);

            resultObject.transform.parent = parent;

            for (int i = 0; i < source.transform.childCount; i++)
            {
                var child = GetPreviewObject(source.transform.GetChild(i).gameObject, resultObject.transform, previewBehaviour);
                TranslateLocalTransformations(source.transform.GetChild(i), child.transform);
            }

            return resultObject;
        }

        /// <summary>
        /// Перенос трансформа с одного объекта на другой.
        /// </summary>
        /// <param name="from">Откуда брать трансформации.</param>
        /// <param name="to">Куда перенести трансформации.</param>
        /// <param name="withScale">Используя масштабирование.</param>
        public static void TranslateLocalTransformations(Transform from, Transform to, bool withScale = true)
        {
            to.localPosition = from.localPosition;
            to.localRotation = from.localRotation;

            if (withScale)
            {
                to.localScale = from.localScale;
            }
        }
        
        /// <summary>
        /// Добавить MeshFilter из одного объекта в другой.
        /// </summary>
        /// <param name="sourceObject">Исходный объект.</param>
        /// <param name="destinationObject">Объект-назначение.</param>
        private static void AddMeshFilter(GameObject sourceObject, GameObject destinationObject)
        {
            var meshFilter = sourceObject.GetComponent<MeshFilter>();

            if (meshFilter)
            {
                destinationObject.AddComponent<MeshFilter>().sharedMesh = meshFilter.sharedMesh;
            }
        }

        /// <summary>
        /// Добавление рендерера из одного объекта в другой.
        /// </summary>
        /// <param name="sourceObject">Исходный объект.</param>
        /// <param name="destinationObject">Объект-назначение.</param>
        /// <param name="previewBehaviour">Контейнер превью.</param>
        private static void AddRenderer(GameObject sourceObject, GameObject destinationObject, PreviewBehaviour previewBehaviour)
        {
            var renderer = sourceObject.GetComponent<Renderer>();

            switch (renderer)
            {
                case MeshRenderer meshRenderer:
                {
                    var resultRenderer = destinationObject.AddComponent<MeshRenderer>();
                    resultRenderer.sharedMaterials = GetPreviewMaterials(renderer.sharedMaterials.Length);
                    resultRenderer.receiveShadows = renderer.receiveShadows;
                    resultRenderer.reflectionProbeUsage = renderer.reflectionProbeUsage;
                    break;
                }
                case SkinnedMeshRenderer skinnedMeshRenderer:
                {
                    var meshFilter = destinationObject.AddComponent<MeshFilter>();
                    var mesh = new Mesh();
                    
                    skinnedMeshRenderer.BakeMesh(mesh, true);
                    meshFilter.sharedMesh = mesh;
                    
                    previewBehaviour.AddAllocated(mesh);
                    
                    var resultRenderer = destinationObject.AddComponent<MeshRenderer>();
                    resultRenderer.sharedMaterials = GetPreviewMaterials(renderer.sharedMaterials.Length);
                    resultRenderer.receiveShadows = renderer.receiveShadows;
                    resultRenderer.reflectionProbeUsage = renderer.reflectionProbeUsage;
                    break;
                }
            }
        }

        /// <summary>
        /// Перенос объекта с вилкой к розетке.
        /// </summary>
        /// <param name="obj">Объект с вилкой.</param>
        /// <param name="socketPoint">Розетка.</param>
        /// <param name="plugPoint">Вилка.</param>
        /// <param name="t">Коэффициент переноса [0..1].</param>
        public static void TransformToSocket(this GameObject obj, SocketPoint socketPoint, PlugPoint plugPoint,
            float t = 1f)
        {
            var socketBehaviourRotation = socketPoint.SocketController.transform.rotation;
            var plugBehaviourRotation = plugPoint.SocketController.transform.rotation;

            var localSocketRotation = Quaternion.Inverse(socketBehaviourRotation) * socketPoint.transform.rotation;
            var localPlugRotation = Quaternion.Inverse(plugBehaviourRotation) * plugPoint.transform.rotation;

            obj.transform.rotation = Quaternion.Lerp(obj.transform.rotation,
                socketBehaviourRotation * localSocketRotation * Quaternion.Inverse(localPlugRotation), t);

            var localPoint = plugPoint.SocketController.transform.InverseTransformPoint(plugPoint.transform.position);

            localPoint.x *= obj.transform.localScale.x;
            localPoint.y *= obj.transform.localScale.y;
            localPoint.z *= obj.transform.localScale.z;

            obj.transform.position = Vector3.Lerp(obj.transform.position,
                socketPoint.transform.position - obj.transform.rotation * localPoint, t);
        }

        /// <summary>
        /// Получение списка превью материалов.
        /// </summary>
        /// <param name="count">Количество нужных материалов.</param>
        /// <returns>Массив материалов.</returns>
        private static Material[] GetPreviewMaterials(int count)
        {
            var materials = new Material[count];

            for (int i = 0; i < count; i++)
            {
                materials[i] = _previewMaterial;
            }

            return materials;
        }

        /// <summary>
        /// Получение материала-превью.
        /// </summary>
        /// <returns>Материал.</returns>
        private static Material GetPreviewMaterial()
        {
            var material = new Material(Resources.Load<Shader>("Shaders/Unlit/VarwinUnlitTransparent"));
            material.enableInstancing = true;
            material.renderQueue = 2999;
            material.color = new Color(1f, 1f, 1f, 0.3f);
            return material;
        }
    }
}
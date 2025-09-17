using System;
using System.Text;
using UnityEditor;
using Varwin.Public;

namespace Varwin.Editor
{
    /// <summary>
    /// Шаг для валидации объекта.
    /// </summary>
    public class ObjectsBuildValidationState : BaseObjectBuildingState
    {
        public ObjectsBuildValidationState(VarwinBuilder builder) : base(builder) => Label = "Validating";

        /// <summary>
        /// Выполнить валидацию объекта.
        /// </summary>
        /// <param name="currentObjectBuildDescription">Объект с информацией о сборке объекта.</param>
        /// <exception cref="Exception">Ошибки валидации.</exception>
        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Validate {currentObjectBuildDescription.ObjectName}";

            var objectDescriptor = currentObjectBuildDescription.ContainedObjectDescriptor;
            var bundlePartsHasErrors = !ValidateAssetBundleParts(objectDescriptor, out var bundlePartErrors);
            var objectNameHasErrors = !ValidateObjectNames(objectDescriptor, out var objectNameErrors);

            if (bundlePartsHasErrors || objectNameHasErrors)
            {
                throw new Exception(GetErrorsMessage(bundlePartErrors, objectNameErrors));
            }

            if (AsmdefUtils.HasMissingReferences(currentObjectBuildDescription.ContainedObjectDescriptor))
            {
                var asmdef = AsmdefUtils.GetAssemblyDefinitionData(currentObjectBuildDescription.ContainedObjectDescriptor);
                throw new Exception($"Can't build {currentObjectBuildDescription.ObjectName}. Assembly definition {asmdef.name} has missing references. Fix it and try to build again.");
            }
        }
        
        /// <summary>
        /// Провести валидацию имени объекта и его asmdef'а.
        /// </summary>
        /// <param name="objectDescriptor">Инормация об объекте.</param>
        /// <param name="objectNameErrors">Строка с текстом ошибки.</param>
        /// <returns>true - успешная валидация. false - неуспешная валидация.</returns>
        private static bool ValidateObjectNames(VarwinObjectDescriptor objectDescriptor, out string objectNameErrors)
        {
            objectNameErrors = string.Empty;
            var descriptorName = objectDescriptor.Name;
            var assetName = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(objectDescriptor.gameObject));
            var isValid = descriptorName == assetName;
            
            if (!isValid)
            {
                objectNameErrors =
                    $"Name \"{descriptorName}\" in {nameof(VarwinObjectDescriptor)} component is not match with object's prefab name \"{assetName}\"";
            }

            return isValid;
        }

        /// <summary>
        /// Провести валидация asset bundle part'ов.
        /// </summary>
        /// <param name="descriptor">Инормация об объекте.</param>
        /// <param name="errorsString">Строка с текстом ошибки.</param>
        /// <returns>true - успешная валидация. false - неуспешная валидация.</returns>
        private static bool ValidateAssetBundleParts(VarwinObjectDescriptor descriptor, out string errorsString)
        {
            if (descriptor.AssetBundleParts == null || descriptor.AssetBundleParts.Length == 0)
            {
                errorsString = string.Empty;
                return true;
            }

            bool isValid = true;

            var sb = new StringBuilder();
            for (var i = 0; i < descriptor.AssetBundleParts.Length; i++)
            {
                var part = descriptor.AssetBundleParts[i];
                if (!part)
                {
                    sb.AppendLine($"Missing asset bundle part at index {i} for object {descriptor.Name}");
                    isValid = false;
                    continue;
                }

                for (var j = 0; j < part.Assets.Count; j++)
                {
                    var asset = part.Assets[j];
                    if (!asset)
                    {
                        sb.AppendLine($"Missing asset at index {j} in asset bundle part at index {i} for object {descriptor.Name}");
                        isValid = false;
                    }
                }
            }

            errorsString = sb.ToString();
            return isValid;
        }

        /// <summary>
        /// Получить строку с ошибками валиации.
        /// </summary>
        /// <param name="bundlePartsErrors">Строка с текстом ошибки валидации asset bundle part.</param>
        /// <param name="objectNameErrors">Строка с текстом ошибки валидации имени объекта.</param>
        /// <returns>Строка с текстом ошибок.</returns>
        private static string GetErrorsMessage(string bundlePartsErrors, string objectNameErrors)
        {
            var sb = new StringBuilder();
            sb.Append("Object validation errors:");

            if (!string.IsNullOrEmpty(bundlePartsErrors))
            {
                sb.AppendLine("Asset bundle errors:\n").Append(bundlePartsErrors);
            }

            if (!string.IsNullOrEmpty(objectNameErrors))
            {
                sb.AppendLine("Object name errors:\n").Append(objectNameErrors);
            }

            return sb.ToString();
        }
    }
}
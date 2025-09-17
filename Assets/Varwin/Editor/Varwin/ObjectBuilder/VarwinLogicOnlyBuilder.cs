using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Varwin.Editor.LogicOnlyBuilding;

namespace Varwin.Editor
{
    /// <summary>
    /// Объект, определяющий логику билда логики выделенных объектов.
    /// </summary>
    public class VarwinLogicOnlyBuilder : VarwinBuilder
    {
        /// <summary>
        /// Определение типа билдера. Нужно для сериализации и десериализации.
        /// </summary>
        public override BuilderType BuildType { get; set; } = BuilderType.LogicOnly;

        /// <summary>
        /// Переопределение шагов билда для логики.
        /// </summary>
        protected override void InitializeStates()
        {
            States = new();
            
            if (SdkSettings.Features.Changelog && !Application.isBatchMode)
            {
                States.Enqueue(new EditChangelogState(this));
            }

            if (ObjectsToBuild != null)
            {
                States.Enqueue(new CheckObjectCacheState(this));
                States.Enqueue(new PreparationState(this));
                
                if (SdkSettings.Features.DynamicVersioning)
                {
                    States.Enqueue(new AsmdefReferencesCollectingState(this));
                    States.Enqueue(new RenameAssembliesToNewNamesState(this));
                }
                
                States.Enqueue(new WrapperGenerationState(this));
                States.Enqueue(new AssembliesCollectingState(this));

                if (SdkSettings.Features.DynamicVersioning)
                {
                    States.Enqueue(new SetVersionSuffixToDescriptorState(this));
                }

                States.Enqueue(new InstallJsonGenerationState(this));
                States.Enqueue(new ZippingLogicFilesState(this));
            }

            //TODO
            if (PackageInfos != null)
            {
                States.Enqueue(new PackageCreationState(this));
            }

            if (ObjectsToBuild != null)
            {
                if (SdkSettings.Features.DynamicVersioning)
                {
                    States.Enqueue(new RenameAssembliesToOldNamesState(this));
                }
            }

            StateCount = States.Count;
        }

        /// <summary>
        /// Переопределение типа объекта при сериализации билдера
        /// </summary>
        public override void Serialize()
        {
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            string jsonModels = JsonConvert.SerializeObject(this, DeleteTempStateFile ? Formatting.None : Formatting.Indented, jsonSerializerSettings);

            File.WriteAllText(TempStateFilename, jsonModels);
        }
    }
}
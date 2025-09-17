using System.Linq;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class AsmdefReferencesCollectingStep : BaseSceneTemplateBuildStep
    {
        private readonly AsmdefDynamicVersioningNamesGenerator _asmdefDynamicVersioningNamesGenerator;

        public AsmdefReferencesCollectingStep(SceneTemplateBuilder builder) : base(builder)
        {
            _asmdefDynamicVersioningNamesGenerator = new();
        }

        public override void Update()
        {
            base.Update();
            
            DllHelper.ForceUpdate();
            AsmdefUtils.Refresh();

            foreach (var asmdefName in Builder.WorldDescriptor.AsmdefNames)
            {
                var asmdefData = AsmdefUtils.LoadAsmdefData(AsmdefUtils.FindAsmdefByName(asmdefName));
                _asmdefDynamicVersioningNamesGenerator.DeepCollectAsmdefNames(asmdefData);
            }

            if (SdkSettings.Settings.DynamicVersioningSupport)
            {
                Builder.OldAssemblyNames = _asmdefDynamicVersioningNamesGenerator.AsmdefsOldToNewName.Keys.ToArray();
                Builder.NewAssemblyNames = _asmdefDynamicVersioningNamesGenerator.AsmdefsOldToNewName.Values.ToArray();
            }
            else
            {
                Builder.OldAssemblyNames = _asmdefDynamicVersioningNamesGenerator.AsmdefsOldToNewName.Keys.ToArray();
                Builder.NewAssemblyNames = Builder.OldAssemblyNames;
            }
        }
    }
}
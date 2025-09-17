using System.Collections.Generic;
using System.Linq;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class RenameAssembliesToOldNamesStep : BaseSceneTemplateBuildStep
    {
        private Dictionary<string, string> _newNamesToOldNames = new Dictionary<string, string>();
        private readonly HashSet<string> _processedAsmdefs = new HashSet<string>();

        
        public RenameAssembliesToOldNamesStep(SceneTemplateBuilder builder) : base(builder)
        {
        }

        public override void Update()
        {
            base.Update();
            
            DllHelper.ForceUpdate();
            AsmdefUtils.Refresh();
            
            _processedAsmdefs.Clear();
            _newNamesToOldNames.Clear();

            for (var i = 0; i < Builder.OldAssemblyNames.Length; i++)
            {
                var newAsmdefName = Builder.NewAssemblyNames[i];
                var oldAsmdefName = Builder.OldAssemblyNames[i];
                _newNamesToOldNames.Add(newAsmdefName, oldAsmdefName);
            }

            foreach (var namesPair in _newNamesToOldNames)
            {
                var newAsmdefName = namesPair.Key;
                var oldAsmdefName = namesPair.Value;

                var asmdefFileInfo = AsmdefUtils.FindAsmdefByName(newAsmdefName);
                var asmdefData = AsmdefUtils.LoadAsmdefData(asmdefFileInfo);

                _processedAsmdefs.Add(asmdefData.name);
                _processedAsmdefs.Add(oldAsmdefName);
                    
                asmdefData.name = oldAsmdefName;
                ReplaceReferences(asmdefData);
                asmdefData.Save(asmdefFileInfo.FullName);
            }

            ReplaceAffectedAsmdefReferences();
        }
        
        private void ReplaceAffectedAsmdefReferences()
        {
            foreach (var asmdef in AsmdefUtils.AllAsmdefInProject)
            {
                if (IsIgnoredAsmdef(asmdef.Key))
                {
                    continue;
                }
                
                if (_processedAsmdefs.Contains(asmdef.Value.name))
                {
                    continue;
                }
                
                ReplaceReferences(asmdef.Value);
                asmdef.Value.Save(asmdef.Key);
            }
        }

        private bool IsIgnoredAsmdef(string asmdefPath)
        {
            asmdefPath = asmdefPath.Replace('\\', '/');
            return asmdefPath.StartsWith("Assets/Plugins/")
                   || asmdefPath.StartsWith("Assets/Varwin/")
                   || asmdefPath.StartsWith("Assets/SteamVR_Input/")
                   || asmdefPath.StartsWith("Packages/");
        }

        private void ReplaceReferences(AssemblyDefinitionData asmdefData)
        {
            if (asmdefData.references == null)
            {
                return;
            }

            string[] references = asmdefData.references.ToArray();
            for (var i = 0; i < references.Length; i++)
            {
                string asmdefDataReference = references[i];
                if (_newNamesToOldNames.ContainsKey(asmdefDataReference))
                {
                    asmdefData.references[i] = _newNamesToOldNames[asmdefDataReference];
                }
            }
        }
    }
}
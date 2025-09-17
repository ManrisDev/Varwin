using System.Collections.Generic;
using System.IO;
using System.Linq;
using Varwin.Editor;

namespace Varwin.SceneTemplateBuilding
{
    public class RenameAssembliesToNewNamesStep : BaseSceneTemplateBuildStep
    {
        private Dictionary<string, string> _oldNamesToNewNames = new Dictionary<string, string>();
        private readonly HashSet<string> _processedAsmdefs = new HashSet<string>();

        
        public RenameAssembliesToNewNamesStep(SceneTemplateBuilder builder) : base(builder)
        {
        }

        public override void Update()
        {
            base.Update();
            
            DllHelper.ForceUpdate();
            AsmdefUtils.Refresh();
            
            _processedAsmdefs.Clear();
            _oldNamesToNewNames.Clear();

            for (var i = 0; i < Builder.OldAssemblyNames.Length; i++)
            {
                string oldAsmdefName = Builder.OldAssemblyNames[i];
                string newAsmdefName = Builder.NewAssemblyNames[i];
                _oldNamesToNewNames.Add(oldAsmdefName, newAsmdefName);
            }

            foreach (var namesPair in _oldNamesToNewNames)
            {
                string oldAsmdefName = namesPair.Key;
                string newAsmdefName = namesPair.Value;

                FileInfo asmdefFileInfo = AsmdefUtils.FindAsmdefByName(oldAsmdefName);
                AssemblyDefinitionData asmdefData = AsmdefUtils.LoadAsmdefData(asmdefFileInfo);

                _processedAsmdefs.Add(asmdefData.name);
                _processedAsmdefs.Add(newAsmdefName);
                    
                asmdefData.name = newAsmdefName;
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
                if (_oldNamesToNewNames.ContainsKey(asmdefDataReference))
                {
                    asmdefData.references[i] = _oldNamesToNewNames[asmdefDataReference];
                }
            }
        }
    }
}
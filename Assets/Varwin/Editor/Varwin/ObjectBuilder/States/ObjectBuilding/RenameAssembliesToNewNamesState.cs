using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class RenameAssembliesToNewNamesState : BaseObjectBuildingState
    {
        private Dictionary<string, string> _oldNamesToNewNames = new Dictionary<string, string>();
        private readonly HashSet<string> _processedAsmdefs = new HashSet<string>();

        public RenameAssembliesToNewNamesState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Create unique assembly versions";
        }

        protected override void OnEnter()
        {
            _processedAsmdefs.Clear();
            _oldNamesToNewNames.Clear();

            for (var i = 0; i < VarwinBuildingData.OldAssemblyNames.Length; i++)
            {
                string oldAsmdefName = VarwinBuildingData.OldAssemblyNames[i];
                string newAsmdefName = VarwinBuildingData.NewAssemblyNames[i];
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
            
            Exit();
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
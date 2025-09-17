using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class RenameAssembliesToOldNamesState : BaseObjectBuildingState
    {
        private Dictionary<string, string> _newNamesToOldNames = new Dictionary<string, string>();
        private readonly HashSet<string> _processedAsmdefs = new HashSet<string>();

        public RenameAssembliesToOldNamesState(VarwinBuilder builder) : base(builder)
        {
            Label = $"Restore default assembly versions";
        }

        protected override void OnEnter()
        {
            _processedAsmdefs.Clear();
            _newNamesToOldNames.Clear();

            for (var i = 0; i < VarwinBuildingData.OldAssemblyNames.Length; i++)
            {
                string newAsmdefName = VarwinBuildingData.NewAssemblyNames[i];
                string oldAsmdefName = VarwinBuildingData.OldAssemblyNames[i];
                _newNamesToOldNames.Add(newAsmdefName, oldAsmdefName);
            }

            foreach (var namesPair in _newNamesToOldNames)
            {
                string newAsmdefName = namesPair.Key;
                string oldAsmdefName = namesPair.Value;

                FileInfo asmdefFileInfo = AsmdefUtils.FindAsmdefByName(newAsmdefName);
                AssemblyDefinitionData asmdefData = AsmdefUtils.LoadAsmdefData(asmdefFileInfo);

                _processedAsmdefs.Add(asmdefData.name);
                _processedAsmdefs.Add(oldAsmdefName);
                    
                asmdefData.name = oldAsmdefName;
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
                if (_newNamesToOldNames.ContainsKey(asmdefDataReference))
                {
                    asmdefData.references[i] = _newNamesToOldNames[asmdefDataReference];
                }
            }
        }
    }
}
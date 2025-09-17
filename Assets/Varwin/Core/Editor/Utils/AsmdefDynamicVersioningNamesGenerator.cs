using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public class AsmdefDynamicVersioningNamesGenerator
    {
        public const string DynamicVersioningAssetLabel = "IgnoreVersioning";
        public HashSet<string> ProcessedAsmdefs { get; protected set; }
        public Dictionary<string, string> AsmdefsOldToNewName { get; protected set; }

        private readonly string[] IgnoringAsmDefNames = 
        {
            "VarwinCore",
            "AVProVideo.Runtime"
        };

        public AsmdefDynamicVersioningNamesGenerator()
        {
            ProcessedAsmdefs = new();
            AsmdefsOldToNewName = new();
        }
        
        public void DeepCollectAsmdefNames(AssemblyDefinitionData asmdefData)
        {
            if (!ProcessedAsmdefs.Add(asmdefData.name))
            {
                return;
            }

            if (ShouldIgnoreVersioning(asmdefData.name))
            {
                AsmdefsOldToNewName.TryAdd(asmdefData.name, asmdefData.name);
            }
            else
            {
                GenerateAsmdefNewName(asmdefData.name);
            }

            if (asmdefData.references == null)
            {
                return;
            }

            var currentAsmdefAsset = AsmdefUtils.LoadAsmdefAsset(AsmdefUtils.FindAsmdefByName(asmdefData.name));
            foreach (string asmdefReference in asmdefData.references)
            {
                if (string.IsNullOrEmpty(asmdefReference))
                {
                    Debug.LogError($"Assembly Definition \"{asmdefData.name}\" has missing reference", currentAsmdefAsset);
                    continue;
                }
                
                if (IgnoringAsmDefNames.Contains(asmdefReference))
                {
                    continue;
                }

                var asmdefReferenceFileInfo = AsmdefUtils.FindAsmdefByName(asmdefReference);
                
                if (asmdefReferenceFileInfo == null)
                {
                    Debug.LogError($"Assembly Definition \"{asmdefData.name}\" has missing reference \"{asmdefReference}\"", currentAsmdefAsset);
                    continue;
                }

                if (asmdefReferenceFileInfo.FullName.Replace("\\", "/").StartsWith(UnityProject.Library))
                {
                    continue;
                }

                if (asmdefReferenceFileInfo.FullName.Replace("\\", "/").StartsWith($"{UnityProject.Assets}/Varwin"))
                {
                    continue;
                }

                if (!ShouldIgnoreVersioning(asmdefReferenceFileInfo) && !AsmdefsOldToNewName.ContainsKey(asmdefReference))
                {
                    GenerateAsmdefNewName(asmdefReference);
                    DeepCollectAsmdefNames(AsmdefUtils.LoadAsmdefData(asmdefReferenceFileInfo));

                    AsmdefsOldToNewName.TryAdd(asmdefReference, asmdefReference);
                }
            }
        }
        
        private void GenerateAsmdefNewName(string asmdefName)
        {
            const int maxFileNameLength = 256;
            const int maxGuidLength = 8;

            if (AsmdefsOldToNewName.ContainsKey(asmdefName))
            {
                return;
            }

            string newGuid = Guid.NewGuid().Clear().Substring(0, maxGuidLength);
            var asmdefNewName = $"{asmdefName}_{newGuid}";
            if (asmdefNewName.Length > maxFileNameLength)
            {
                int maxSubstringIndex = Mathf.Min(asmdefName.LastIndexOf('_'), maxFileNameLength - maxGuidLength);
                string asmdefNameWithoutGuid = asmdefName.Substring(0, Mathf.Min(maxSubstringIndex, asmdefName.Length - 1));
                asmdefNewName = $"{asmdefNameWithoutGuid}_{newGuid}";
            }
            
            AsmdefsOldToNewName.Add(asmdefName, asmdefNewName);
        }

        private static bool ShouldIgnoreVersioning(string name)
        {
            return ShouldIgnoreVersioning(AsmdefUtils.FindAsmdefByName(name));
        }

        public static bool ShouldIgnoreVersioning(FileInfo fileInfo)
        {
            var assetGuid = AssetDatabase.GUIDFromAssetPath(fileInfo.GetAssetPath());
            var assetLabels = AssetDatabase.GetLabels(assetGuid);

            return assetLabels != null && assetLabels.Any(x => !string.IsNullOrWhiteSpace(x) && string.Equals(x, DynamicVersioningAssetLabel));
        }
    }
}
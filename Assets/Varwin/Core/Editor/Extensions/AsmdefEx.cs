using System.IO;
using Newtonsoft.Json;
using UnityEditorInternal;
using Varwin.Data;

namespace Varwin.Editor
{
    public static class AsmdefEx
    {
        public static AssemblyDefinitionData GetData(this AssemblyDefinitionAsset asmdefAsset)
        {
            return asmdefAsset.text.JsonDeserialize<AssemblyDefinitionData>();
        }

        public static void Save(this AssemblyDefinitionData asmdefData, string path)
        {
            File.WriteAllText(path, asmdefData.Serialize());
        }
    }
}
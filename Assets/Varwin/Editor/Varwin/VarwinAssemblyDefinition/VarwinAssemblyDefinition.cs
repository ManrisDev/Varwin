using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class VarwinAssemblyDefinition
    {
        [MenuItem("Assets/Create/Varwin/Varwin Assembly Definition", false, 91)]
        public static string Create()
        {
            string path = GetSelectedPath();

            string guid = System.Guid.NewGuid().ToString().Replace("-", "");
            
            string name = path;
            if (Directory.Exists(path))
            {
                name = new DirectoryInfo(path).Name;
            }

            name = ObjectHelper.ConvertToClassName(name);

            string asmdef = Create($"{name}_{guid}", $"{path}/{name}.asmdef");

            AssetDatabase.ImportAsset(asmdef, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            return asmdef;
        }

        public static string Create(string path)
        {
            string folderPath = path;
            if (File.Exists(path))
            {
                folderPath = new FileInfo(path).DirectoryName;
            }
            
            string name = new DirectoryInfo(folderPath).Name;
            
            string guid = System.Guid.NewGuid().ToString().Replace("-", "");
            
            string asmdef = Create($"{name}_{guid}", $"{folderPath}/{name}.asmdef");

            AssetDatabase.ImportAsset(asmdef, ImportAssetOptions.ForceUpdate);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            return asmdef;
        }
        
        public static string Create(string name, string path)
        {
            if (Directory.Exists(path))
            {
                path = $"{path.TrimEnd('/', '\\')}/{name}.asmdef";
            }

            var asmdefData = new AssemblyDefinitionData
            {
                name = name,
                references = new[] {"VarwinCore"}
            };

            asmdefData.Save(path);

            return path;
        }
        
        [MenuItem("Assets/Create/Varwin Assembly Definition", true, 91)]
        public static bool CanCreate()
        {
            string path = GetSelectedPath();
            
            if (path == "Assets" || !Directory.Exists(path) || Directory.GetFiles(path, "*.asmdef", SearchOption.TopDirectoryOnly).Length > 0)
            {
                return false;
            }

            return true;
        }

        private static string GetSelectedPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
            }

            return path;
        }
    }
}
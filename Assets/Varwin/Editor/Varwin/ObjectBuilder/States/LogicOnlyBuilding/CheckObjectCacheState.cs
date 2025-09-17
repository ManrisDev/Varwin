using System.IO;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor.LogicOnlyBuilding
{
    /// <summary>
    /// Шаг проверки наличия *.vwo объекта, для которого билдится логика.
    /// </summary>
    public class CheckObjectCacheState : BaseObjectBuildingState
    {
        public CheckObjectCacheState(VarwinBuilder builder) : base(builder)
        {
            Label = "Check object cache";
        }

        protected override void OnEnter()
        {
            if (!Directory.Exists(VarwinBuildingPath.BakedObjects))
            {
                var errorMessage = $"Can't build. Folder with cache ({VarwinBuildingPath.BakedObjects}) doesn't exist.";
                Debug.LogError(errorMessage);
                EditorUtility.DisplayDialog("Build logic error", errorMessage, "OK");
                Builder.Stop();
            }
        }

        protected override void Update(ObjectBuildDescription currentObjectBuildDescription)
        {
            Label = $"Check cache for {currentObjectBuildDescription.ObjectName}";
            var pathToZippedCache = GetZippedObjectPath(currentObjectBuildDescription);

            if (!File.Exists(pathToZippedCache))
            {
                var errorMessage = $"Can't build. File with cache ({pathToZippedCache}) doesn't exist.";
                EditorUtility.DisplayDialog("Build logic error", errorMessage, "OK");
                Builder.Stop();
            }
        }

        private static string GetZippedObjectPath(ObjectBuildDescription buildDescription)
        {
            return $"{Path.Combine(UnityProject.Path, VarwinBuildingPath.BakedObjects,buildDescription.ObjectName )}.vwo";
        }
    }
}
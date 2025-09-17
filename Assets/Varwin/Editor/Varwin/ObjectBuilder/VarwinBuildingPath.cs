namespace Varwin.Editor
{
    public static class VarwinBuildingPath
    {
        public static string AssetBundles => $"{UnityProject.Path}/AssetBundles";
        public static string SourcePackages => $"{UnityProject.Path}/SourcePackages";
        public static string BakedObjects => SdkSettings.ObjectBuildingFolderPath;
        public static string BakedSceneTemplates => SdkSettings.SceneTemplateBuildingFolderPath;
        public static string ScriptAssemblies => $"{UnityProject.Path}/Library/ScriptAssemblies";
        public static string ObjectPreviews => $"{UnityProject.Path}/Temp/ObjectPreviews";
    }
}
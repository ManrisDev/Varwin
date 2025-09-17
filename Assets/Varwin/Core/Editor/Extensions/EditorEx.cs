using System;
using System.Reflection;

namespace Varwin.Editor
{
    public class EditorEx
    {
        /// <summary>
        /// Force recompile all editor's scripts
        /// </summary>
        public static void ForceRecompile()
        {
            //TODO: DirtyAllScripts is null, надо переписать метод
            
            Assembly editorAssembly = typeof(UnityEditor.Editor).Assembly;
            Type compilationInterface = editorAssembly.GetType("UnityEditor.Scripting.ScriptCompilation.EditorCompilationInterface");

            if (compilationInterface != null)
            {
                BindingFlags staticBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                MethodInfo dirtyAllScriptsMethod = compilationInterface.GetMethod("DirtyAllScripts", staticBindingFlags);
                dirtyAllScriptsMethod?.Invoke(null, null);
            }

            UnityEditor.AssetDatabase.Refresh();
        }
    }
}
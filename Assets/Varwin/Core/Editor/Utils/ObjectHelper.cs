using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace Varwin.Editor
{
    public static class ObjectHelper
    {
        public static bool IsValidTypeName(string typeName, bool showMessage = false)
        {
            if (!System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(typeName))
            {
                if (showMessage)
                {
                    EditorUtility.DisplayDialog("Error!",
                        $"Invalid type name \"{typeName}\"", "OK");
                }

                return false;
            }

            if (!Regex.IsMatch(typeName, @"^[a-zA-Z0-9_]+$"))
            {
                if (showMessage)
                {
                    EditorUtility.DisplayDialog("Error!",
                        $"Wrong type name {typeName}; Varwin type name can contain only English letters and numbers.", "OK");
                }

                return false;
            }

            return true;
        }
        
        public static string ConvertToNiceName(string name)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;  
            TextInfo textInfo = cultureInfo.TextInfo;

            name = Regex.Replace(name, "([A-ZА-Я])", " $1")
                .Replace("_", " ")
                .Replace("-", " ");

            name = Regex.Replace(name, "(\\s)+", "$1");
                
            return ObjectNames.NicifyVariableName(textInfo.ToTitleCase(name)).Trim();
        }

        public static string ConvertToClassName(string name)
        {
            return  Regex.Replace(name.Trim(), "[^A-Za-z0-9_]", "");
        }
        
        public static string EscapeString(string input)
        {
            using (var writer = new StringWriter())
            {
                using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                {
                    provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);

                    var ret = writer.ToString();

                    return ret.Substring(1, ret.Length - 2);
                }
            }
        }

        public static List<string> GetExistingObjectsNames()
        {
            var list = new List<string>();
            var allVarwinObjectPaths = TypeUtils.FindAllPrefabsOfType<Public.VarwinObjectDescriptor>(SearchResultType.Path);
            foreach (var varwinObjectPath in allVarwinObjectPaths)
            {
                 var arr = varwinObjectPath.Split('/');
                 list.Add(arr[arr.Length - 2]);
            }
            return list;
        }
    }
}
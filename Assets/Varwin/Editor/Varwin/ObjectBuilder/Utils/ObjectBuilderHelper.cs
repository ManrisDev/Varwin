using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Varwin.Public;
using Debug = UnityEngine.Debug;

namespace Varwin.Editor
{
    public static class ObjectBuilderHelper
    {
        public static void Log(IEnumerable<ObjectBuildDescription> objectsList)
        {
            var builtObjects = new Dictionary<string, string>();
            foreach (var objectBuildDescription in objectsList)
            {
                if (objectBuildDescription == null || objectBuildDescription.HasError)
                {
                    continue;
                }

                var varwinObjectDescriptor = objectBuildDescription.ContainedObjectDescriptor;
                if (varwinObjectDescriptor)
                {
                    string locale = "";
                    if (varwinObjectDescriptor.DisplayNames != null && varwinObjectDescriptor.DisplayNames.Count > 0)
                    {
                        string en = varwinObjectDescriptor.DisplayNames.FirstOrDefault(x => x.key == Language.English)?.value;
                        string ru = varwinObjectDescriptor.DisplayNames.FirstOrDefault(x => x.key == Language.Russian)?.value;
                        
                        if (!string.IsNullOrEmpty(en))
                        {
                            locale += $"{en}; ";
                        }

                        if (!string.IsNullOrEmpty(ru))
                        {
                            locale += $"{ru}; ";
                        }
                    }
                    else
                    {
                        var varwinObject = varwinObjectDescriptor.GetComponent<IWrapperAware>();
                        if (varwinObject != null)
                        {
                            var type = varwinObject.GetType();
                                
                            var objectName = type.GetCustomAttribute<ObjectNameAttribute>(false);
                            if (objectName != null)
                            {
                                string en = objectName.LocalizedNames?.en;
                                string ru = objectName.LocalizedNames?.ru;
                                
                                if (!string.IsNullOrEmpty(en))
                                {
                                    locale += $"{en}; ";
                                }

                                if (!string.IsNullOrEmpty(ru))
                                {
                                    locale += $"{ru}; ";
                                }
                            }
                            else
                            {
                                var locales = type.GetCustomAttributes<LocaleAttribute>(false);
                                foreach (var l in locales)
                                {
                                    locale += $"{l.Strings[0]}; ";
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(locale.Trim()))
                    {
                        builtObjects.Add(objectBuildDescription.ObjectName, locale.Trim());
                    }
                }
            }
            
            string builtObjectsListMessage = "";
            foreach (var o in builtObjects)
            {
                builtObjectsListMessage += $"* `{o.Key}` — {o.Value}\n";
            }
            Debug.Log(builtObjectsListMessage);
        }
        
        public static void DeleteTempFolder()
        {
            if (!VarwinBuilder.DeleteTempFolder)
            {
                return; 
            }
            
            string folder = $"Assets/VarwinTemp/";
            
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Varwin.Core;
using Varwin.Public;

namespace Varwin.Editor
{
    public class LocalizationDictionaryDrawer : ReorderableList
    {
        private static Defaults _defaults;

        public string Title;
        public LocalizationDictionary LocalizationDictionary;
        public string ErrorMessage;
        
        private bool _isInited;

        public bool IsValidDictionary => LocalizationDictionary.IsValidDictionary;
        
        public LocalizationDictionaryDrawer(LocalizationDictionary localizationDictionary) : base(localizationDictionary, typeof(LocalizationString), true, false, true, true)
        {
            LocalizationDictionary = localizationDictionary;
        }
        
        public LocalizationDictionaryDrawer(IList elements, Type elementType) : base(elements, elementType)
        {
        }

        public LocalizationDictionaryDrawer(IList elements, Type elementType, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) : base(elements, elementType, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
        }

        public LocalizationDictionaryDrawer(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements)
        {
        }

        public LocalizationDictionaryDrawer(SerializedObject serializedObject, SerializedProperty elements, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) : base(serializedObject, elements, draggable, displayHeader, displayAddButton, displayRemoveButton)
        {
        }

        public void Initialize(LocalizationDictionary localizationDictionary)
        {
            if (LocalizationDictionary != localizationDictionary && localizationDictionary != null)
            {
                LocalizationDictionary = localizationDictionary;
                _isInited = false;
            }
            
            Initialize();
        }

        public void Initialize()
        {
            if (_isInited)
            {
                return;
            }
        
            drawElementCallback = (Rect rect, int elementIndex, bool isActive, bool isFocused) =>
            {
                rect.y += 2;

                var keyRect = new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight);
                var valueRect = new Rect(rect.x + 128, rect.y, rect.width - 128, EditorGUIUtility.singleLineHeight);
                
                if (LocalizationDictionary != null)
                {
                    var languages = (Language[]) Enum.GetValues(typeof(Language));
                    
                    var availableLanguages = languages
                        .Where(x => !LocalizationDictionary.Contains(x) || x == LocalizationDictionary[elementIndex].key)
                        .Select(x => x.ToString())
                        .ToList();

                    int selectedLanguageIndex = availableLanguages.IndexOf(LocalizationDictionary[elementIndex].key.ToString());

                    selectedLanguageIndex = EditorGUI.Popup(keyRect, selectedLanguageIndex, availableLanguages.ToArray());

                    string selectedLanguageName = availableLanguages[selectedLanguageIndex];

                    LocalizationDictionary[elementIndex].key = languages.FirstOrDefault(x => x.ToString() == selectedLanguageName);
                    LocalizationDictionary[elementIndex].value = EditorGUI.TextField(valueRect, LocalizationDictionary[elementIndex].value);
                }
                else if (serializedProperty != null)
                {
                    SerializedProperty element = serializedProperty.GetArrayElementAtIndex(elementIndex);
                    EditorGUI.PropertyField(keyRect, element.FindPropertyRelative("key"), GUIContent.none);
                    EditorGUI.PropertyField(valueRect, element.FindPropertyRelative("value"), GUIContent.none);
                }
            };

            onAddCallback = (ReorderableList reorderableList) =>
            {
                if (LocalizationDictionary != null)
                {
                    if (LocalizationDictionary.Count == 0 || !LocalizationDictionary.Contains(Language.English))
                    {
                        LocalizationDictionary.Add(Language.English, string.Empty);
                    }
                    else
                    {
                        if ((Language) Application.systemLanguage != Language.English && !LocalizationDictionary.Contains(Application.systemLanguage))
                        {
                            LocalizationDictionary.Add((Language) Application.systemLanguage, string.Empty);
                        }
                        else
                        {
                            Language lastLanguage = LocalizationDictionary.LastOrDefault().key;
                            Language nextLanguage = lastLanguage.Next();

                            while (LocalizationDictionary.Contains(nextLanguage))
                            {
                                nextLanguage = nextLanguage.Next();
                            }
                            
                            LocalizationDictionary.Add(nextLanguage, string.Empty);
                        }
                    }

                    reorderableList.index = LocalizationDictionary.Count - 1;
                }
                else if (reorderableList.serializedProperty != null)
                {
                    int index = reorderableList.serializedProperty.arraySize;
                    reorderableList.serializedProperty.arraySize++;
                    reorderableList.index = index;

                    SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                    var prop = element.FindPropertyRelative("key");
                    prop.enumValueIndex = (prop.enumValueIndex + 1) % prop.enumNames.Length;
                }
                
                Update();
            };
            
            onRemoveCallback += (ReorderableList reorderableList) =>
            {
                _defaults.DoRemoveButton(this);

                LocalizationDictionary.Validate();
            };

            drawHeaderCallback += (Rect rect) =>
            {
                if (!string.IsNullOrEmpty(Title))
                {
                    GUI.Label(rect, Title);
                }
            };
            
            drawFooterCallback += (Rect rect) =>
            {
                _defaults.DrawFooter(rect, this);

                if (!IsValidDictionary)
                {
                    DrawErrorMessage();
                }
            };
            
            _isInited = true;
        }

        public void Draw()
        {
            if (_defaults == null)
            {
                _defaults = new ReorderableList.Defaults();
            }

            Update();

            if (IsValidDictionary)
            {
                displayAdd = true;
                footerHeight = 13;
            }
            else
            {
                displayAdd = false;
                footerHeight = -3;
            }

            if (!UniqueLanguageExists())
            {
                displayAdd = false;
            }
            
            DoLayoutList();
        }
        
        private void DrawErrorMessage()
        {
            var nonUniqueLanguage = LocalizationDictionary.GroupBy(x => x.key).FirstOrDefault(x => x.Count() > 1);
            if (nonUniqueLanguage != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                EditorGUILayout.HelpBox(string.IsNullOrEmpty(ErrorMessage) ? "Error!" : string.Format(ErrorMessage, nonUniqueLanguage.Key), MessageType.Error, true);
            }
        }

        public void Update()
        {
            LocalizationDictionary.Validate();
        }

        public bool UniqueLanguageExists()
        {
            var languages = (Language[]) Enum.GetValues(typeof(Language));
            foreach (var language in languages)
            {
                if (!LocalizationDictionary.Contains(language))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    [CustomPropertyDrawer(typeof(LocalizationDictionary))]
    public class LocalizationDictionaryPropertyDrawer : PropertyDrawer
    {
        private const string AnItemWithLanguageHasAlreadyBeenAddedFormat = "An item with the language \"{0}\" has already been added";
        
        private const int HugarianIndex = 18;
        private const int EditorIndentSize = 15;
        
        private static ReorderableList.Defaults _defaults;
        private static Language[] _languages;
        private static Dictionary<SerializedProperty, ReorderableList> _reorderableLists = new Dictionary<SerializedProperty, ReorderableList>();
        private static Dictionary<ReorderableList, int> _deleteCommands = new Dictionary<ReorderableList, int>();
        
        private ReorderableList _list;
        private float _defaultFooterHeight;
        private float _errorMessageMarginTop = 16f;
        private float _errorMessageHeight = 40f;

        private ReorderableList GetList(SerializedProperty serializedProperty)
        {
            if (_defaults == null)
            {
                _defaults = new ReorderableList.Defaults();
            }

            if (_languages == null)
            {
                _languages = (Language[]) Enum.GetValues(typeof(Language));
            }

            _list = _reorderableLists.ContainsKey(serializedProperty) ? _reorderableLists[serializedProperty] : null;
            
            if (_list == null)
            {
                SerializedProperty localizationStrings = serializedProperty.FindPropertyRelative("LocalizationStrings");
                SerializedProperty isValidDictionary = serializedProperty.FindPropertyRelative("IsValidDictionary");
                
                _list = new ReorderableList(serializedProperty.serializedObject, localizationStrings, false, true, true, false);
                _reorderableLists[serializedProperty] = _list;
                
                _defaultFooterHeight = _list.footerHeight;
                
                _list.drawElementCallback += (Rect rect, int elementIndex, bool isActive, bool isFocused) =>
                {
                    var selectedLanguages = new List<Language>();
                    for (int i = 0; i < localizationStrings.arraySize; ++i)
                    {
                        selectedLanguages.Add(_languages[localizationStrings.GetArrayElementAtIndex(i).FindPropertyRelative("key").enumValueIndex]);
                    }
                    
                    rect.y += 2;

                    SerializedProperty element = localizationStrings.GetArrayElementAtIndex(elementIndex);
                    SerializedProperty key = element.FindPropertyRelative("key");

                    int indent = EditorIndentSize * EditorGUI.indentLevel;
                    var keyRect = new Rect(rect.x - indent, rect.y, 120 + indent, EditorGUIUtility.singleLineHeight);
                    var valueRect = new Rect(keyRect.x + 128, rect.y, rect.width - 128 + indent - 25, EditorGUIUtility.singleLineHeight);
                    var minusRect = new Rect(valueRect.x + valueRect.width + 4, rect.y - 1, 20, EditorGUIUtility.singleLineHeight);
                    
                    var availableLanguages = _languages
                        .Where(x => !selectedLanguages.Contains(x) || x == _languages[key.enumValueIndex])
                        .Select(x => x.ToString())
                        .ToList();
                    
                    int selectedLanguageIndex = availableLanguages.IndexOf(_languages[key.enumValueIndex].ToString());
                    
                    selectedLanguageIndex = EditorGUI.Popup(keyRect, selectedLanguageIndex, availableLanguages.ToArray());
                    
                    string selectedLanguageName = availableLanguages[selectedLanguageIndex];

                    int currentLanguageIndex = 0;
                    for (int i = 0; i < _languages.Length; ++i)
                    {
                        if (_languages[i].ToString() == selectedLanguageName)
                        {
                            currentLanguageIndex = i;
                            break;
                        }
                    }
                    
                    key.enumValueIndex = currentLanguageIndex;
                    
                    EditorGUI.PropertyField(valueRect, element.FindPropertyRelative("value"), GUIContent.none);
                    
                    if (VarwinStyles.Minus(minusRect))
                    {
                        _deleteCommands.Add(_list, elementIndex);
                    }
                    
                    serializedProperty.serializedObject.ApplyModifiedProperties();
                };

                _list.onAddDropdownCallback += (rect, reorderableList) =>
                {
                    void OnSelectAddMenuItem(object userData)
                    {
                        int selectedLanguage = (int) userData;
                
                        int index = localizationStrings.arraySize;
                        localizationStrings.InsertArrayElementAtIndex( localizationStrings.arraySize);
                        
                        SerializedProperty element = localizationStrings.GetArrayElementAtIndex(index);
                        SerializedProperty prop = element.FindPropertyRelative("key");
                        prop.enumValueIndex = selectedLanguage;
                        _list.index = index;

                        serializedProperty.serializedObject.ApplyModifiedProperties();
                        
                        Validate(serializedProperty);
                    }
                    
                    var selectedLanguages = new List<Language>();
                    for (int i = 0; i < localizationStrings.arraySize; ++i)
                    {
                        selectedLanguages.Add(_languages[localizationStrings.GetArrayElementAtIndex(i).FindPropertyRelative("key").enumValueIndex]);
                    }

                    var availableLanguages = new Dictionary<string, int>();
                    for (int i = 0; i < _languages.Length; ++i)
                    {
                        if (!selectedLanguages.Contains(_languages[i]) && !availableLanguages.ContainsKey(_languages[i].ToString()))
                        {
                            availableLanguages.Add(_languages[i].ToString(), i);
                        }
                    }
                    
                    var menu = new GenericMenu();
                    foreach (var availableLanguage in availableLanguages)
                    {
                        menu.AddItem(new GUIContent(availableLanguage.Key), false, OnSelectAddMenuItem, availableLanguage.Value);
                    }
                    menu.DropDown(rect);
                };
                
                _list.onRemoveCallback += (ReorderableList reorderableList) =>
                {
                    _defaults.DoRemoveButton(_list);
                };

                _list.drawHeaderCallback += (Rect rect) =>
                {
                    GUI.Label(rect, serializedProperty.displayName);
                };
            
                _list.drawFooterCallback += (Rect rect) =>
                {
                    _defaults.DrawFooter(rect, _list);

                    if (!isValidDictionary.boolValue)
                    {
                        _list.footerHeight = _defaultFooterHeight + _errorMessageHeight + _errorMessageMarginTop;
                        DrawErrorMessage(rect, serializedProperty);
                    }
                    else
                    {
                        _list.footerHeight = _defaultFooterHeight;
                    }
                };
            }

            return _list;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetList(property).GetHeight();
        }
        
        public override void OnGUI(Rect position, SerializedProperty serializedProperty, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, serializedProperty);

            ReorderableList list = GetList(serializedProperty);

            int indent = EditorGUI.indentLevel * EditorIndentSize;
            var rect = new Rect(position) 
            {
                x = position.x + indent,
                width = position.width - indent
            };

            list.DoList(rect);

            EditorGUI.EndProperty();
            
            Validate(serializedProperty);

            _list.displayAdd = UniqueLanguageExists(serializedProperty);
            
            if (_deleteCommands.ContainsKey(_list))
            {
                _list.serializedProperty.DeleteArrayElementAtIndex(_deleteCommands[_list]);
                _deleteCommands.Remove(_list);
                serializedProperty.serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void Validate(SerializedProperty serializedProperty)
        {
            SerializedProperty localizationStrings = serializedProperty.FindPropertyRelative("LocalizationStrings");
            SerializedProperty isValidDictionary = serializedProperty.FindPropertyRelative("IsValidDictionary");
            
            isValidDictionary.boolValue = true;

            var selectedLanguagesIndexes = new HashSet<int>();
            for (int i = 0; i < localizationStrings.arraySize; ++i)
            {
                var index = localizationStrings.GetArrayElementAtIndex(i).FindPropertyRelative("key").enumValueIndex;
                if (!selectedLanguagesIndexes.Contains(index))
                {
                    selectedLanguagesIndexes.Add(index);
                }
                else
                {
                    isValidDictionary.boolValue = false;
                    break;
                }
            }

            serializedProperty.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
        
        private bool UniqueLanguageExists(SerializedProperty serializedProperty)
        {
            SerializedProperty localizationStrings = serializedProperty.FindPropertyRelative("LocalizationStrings");
            
            var systemLanguages = new List<Language>();
            for (int i = 0; i < localizationStrings.arraySize; ++i)
            {
                systemLanguages.Add(_languages[localizationStrings.GetArrayElementAtIndex(i).FindPropertyRelative("key").enumValueIndex]);
            }

            return _languages.Any(language => !systemLanguages.Contains(language));
        }
        
        private void DrawErrorMessage(Rect rect, SerializedProperty serializedProperty)
        {
            SerializedProperty localizationStrings = serializedProperty.FindPropertyRelative("LocalizationStrings");
            
            rect.y += _errorMessageMarginTop;
            rect.height = _errorMessageHeight;

            int nonUniqueLanguage = -1;
            var selectedLanguages = new List<int>();
            
            for (int i = 0; i < localizationStrings.arraySize; ++i)
            {
                var index = localizationStrings.GetArrayElementAtIndex(i).FindPropertyRelative("key").enumValueIndex;

                if (selectedLanguages.Contains(index))
                {
                    nonUniqueLanguage = index;
                    break;
                }
                
                selectedLanguages.Add(index);
            }
            
            if (nonUniqueLanguage >= 0)
            {
                string languageName = _languages[nonUniqueLanguage].ToString();
                string errorMessage = string.Format(AnItemWithLanguageHasAlreadyBeenAddedFormat, languageName);
                EditorGUI.HelpBox(rect, errorMessage, MessageType.Error);
            }
        }
    }
}
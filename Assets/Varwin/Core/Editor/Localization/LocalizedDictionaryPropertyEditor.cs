using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    /// <summary>
    /// Редактор словаря.
    /// </summary>
    [CustomPropertyDrawer(typeof(LocalizedDictionaryBase), true)]
    public class LocalizedDictionaryPropertyEditor : PropertyDrawer
    {
        /// <summary>
        /// UI списка.
        /// </summary>
        private Dictionary<SerializedProperty, ReorderableList> _reorderableLists;

        /// <summary>
        /// Индексы элементов для удаления.
        /// </summary>
        private Dictionary<ReorderableList, Queue<int>> _indicesForRemove = new();

        /// <summary>
        /// Отображнеие в редакторе.
        /// </summary>
        /// <param name="rect">Габариты.</param>
        /// <param name="property">Целевое поле.</param>
        /// <param name="label">Имя.</param>
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);
            var list = GetReorderableList(property);
            list.DoList(rect);

            if (_indicesForRemove.ContainsKey(list))
            {
                var indices = _indicesForRemove[list];
                if (_indicesForRemove.Count > 0 && indices?.Count > 0)
                {
                    list.serializedProperty.DeleteArrayElementAtIndex(indices.Dequeue());
                    property.serializedObject.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Получение списка по свойству.
        /// </summary>
        /// <param name="property">Свойство.</param>
        /// <returns>Список.</returns>
        private ReorderableList GetReorderableList(SerializedProperty property)
        {
            _reorderableLists ??= new Dictionary<SerializedProperty, ReorderableList>();
            var list = _reorderableLists.ContainsKey(property) ? _reorderableLists[property] : null;
            if (list != null)
            {
                return list;
            }
            
            var items = property.FindPropertyRelative("_dictionary");
            list = new ReorderableList(property.serializedObject, items, false, true, true, false);
            list.drawElementCallback = (elementRect, index, isActive, isFocused) => DrawElement(list, elementRect, index, isActive, isFocused);
            list.drawHeaderCallback = headerRect => DrawHeader(property, headerRect);
            list.onAddDropdownCallback = OnDropdownClicked;
            _reorderableLists[property] = list;

            return list;
        }

        /// <summary>
        /// Событие, вызываемое при нажатии на кнопку.
        /// </summary>
        /// <param name="buttonRect">Габариты кнопки.</param>
        /// <param name="list">Список переводов.</param>
        private void OnDropdownClicked(Rect buttonRect, ReorderableList list)
        {
            var availableLanguages = GetAvailableLanguages(list);
            var menu = new GenericMenu();
            foreach (var language in availableLanguages)
            {
                menu.AddItem(new GUIContent(language.ToString()), false, (arg) => OnLanguageSelected(arg, list), language);
            }

            menu.DropDown(buttonRect);
        }

        /// <summary>
        /// Список доступных локалей.
        /// </summary>
        /// <param name="list">Список переводов.</param>
        /// <returns>Список локалей.</returns>
        private List<Language> GetAvailableLanguages(ReorderableList list)
        {
            var usedLanguages = new List<Language>();
            for (int i = 0; i < list.serializedProperty.arraySize; i++)
            {
                usedLanguages.Add((Language) list.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Locale").enumValueFlag);
            }

            var allLocales = Enum.GetValues(typeof(Language));
            var availableLanguages = allLocales.Cast<Language>().Where(locale => usedLanguages.All(a => a != locale)).ToList();
            return availableLanguages;
        }

        /// <summary>
        /// При выборе языка в выпадающем меню.
        /// </summary>
        /// <param name="userdata">Выбранная локаль.</param>
        /// <param name="list">Список.</param>
        private void OnLanguageSelected(object userdata, ReorderableList list)
        {
            var newElementIndex = list.serializedProperty.arraySize;
            list.serializedProperty.InsertArrayElementAtIndex(newElementIndex);
            var element = list.serializedProperty.GetArrayElementAtIndex(newElementIndex);
            var localeProperty = element.FindPropertyRelative("Locale");
            localeProperty.enumValueFlag = (int) userdata;
            list.serializedProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Отрисовка заголовка.
        /// </summary>
        /// <param name="property">Целевое свойство.</param>
        /// <param name="rect">Габариты заголовка.</param>
        private void DrawHeader(SerializedProperty property, Rect rect)
        {
            var propertyName = property.name;
            propertyName = propertyName.Replace("_", "");
            propertyName = char.ToUpperInvariant(propertyName[0]) + propertyName.Substring(1);
            EditorGUI.LabelField(rect, propertyName);
        }

        /// <summary>
        /// Отрисовка одного элемента списка.
        /// </summary>
        /// <param name="rect">Габариты.</param>
        /// <param name="index">Индекс элемента.</param>
        /// <param name="isActive">Активный ли.</param>
        /// <param name="isFocused">В фокусе ли.</param>
        private void DrawElement(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;

            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var keyRect = new Rect(rect.x, rect.y, 100, lineHeight);
            var valueRect = new Rect(keyRect.x + keyRect.width + 8, rect.y, rect.width - keyRect.width - 8 - lineHeight, lineHeight);
            var minusButtonRect = new Rect(valueRect.x + valueRect.width + 2, rect.y, lineHeight, lineHeight);
            var value = element.FindPropertyRelative("Locale");

            var availableLanguages = GetAvailableLanguages(list);
            availableLanguages.Add((Language) value.enumValueFlag);

            var availableLanguagesNames = availableLanguages.Select(a => a.ToString()).ToArray();
            var selectedIndexValue = availableLanguages.IndexOf((Language) value.enumValueFlag);

            selectedIndexValue = EditorGUI.Popup(keyRect, selectedIndexValue, availableLanguagesNames);
            value.enumValueFlag = (int) availableLanguages[selectedIndexValue];

            EditorGUI.PropertyField(valueRect, element.FindPropertyRelative("Value"), GUIContent.none);

            if (VarwinStyles.Minus(minusButtonRect))
            {
                if (_indicesForRemove.ContainsKey(list))
                {
                    _indicesForRemove[list] ??= new Queue<int>();
                    _indicesForRemove[list].Enqueue(index);
                }
                else
                {
                    var queue = new Queue<int>();
                    queue.Enqueue(index);
                    _indicesForRemove.Add(list, queue);    
                }
            }
        }

        /// <summary>
        /// Получение размера свойства.
        /// </summary>
        /// <param name="property">Свойство.</param>
        /// <param name="label">Заголовок.</param>
        /// <returns>Размер свойства.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetReorderableList(property).GetHeight();
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Editor
{
    [CustomEditor(typeof(VarwinAnimationPlayer))]
    public class VarwinAnimationPlayerEditor : UnityEditor.Editor
    {
        private SerializedProperty _animatorPropertyField;
        private SerializedProperty _customAnimations;

        private FieldInfo _customAnimationField;
        private List<VarwinCustomAnimation> _customAnimationList;
        
        private List<bool> _foldouts;

        private VarwinAnimationPlayer _varwinAnimationPlayer;
        private Animator _animatorComponentOnObject;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawAnimatorPropertyField();


            GUILayout.Label("Custom animations:");
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int animationToDeleteIndex = -1;

            if (_customAnimations.arraySize > 0)
            {
                for (int i = 0; i < _customAnimations.arraySize; ++i)
                {
                    SerializedProperty property = _customAnimations.GetArrayElementAtIndex(i);
                    SerializedProperty clipProperty = property.FindPropertyRelative("Clip");
                    SerializedProperty nameProperty = property.FindPropertyRelative("Name");
                    
                    AnimationClip clipValue = GetCustomAnimationAtIndex(i);
                    string foldoutName = clipValue ? ObjectHelper.ConvertToNiceName(clipValue.name) : "None";
                    
                    Rect rect = EditorGUILayout.GetControlRect();
                    
                    var foldoutRect = new Rect(rect)
                    {
                        x = 32, 
                        width = rect.width - 32
                    };
                    
                    _foldouts[i] = EditorGUI.Foldout(foldoutRect, _foldouts[i], foldoutName);

                    var buttonRect = new Rect(rect)
                    {
                        x = foldoutRect.width + 32, 
                        width = 20
                    };
                    if (VarwinStyles.Minus(buttonRect))
                    {
                        animationToDeleteIndex = i;
                    }

                    if (_foldouts[i])
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(clipProperty);

                        if (clipValue && nameProperty.FindPropertyRelative("LocalizationStrings").arraySize == 0)
                        {
                            AddLocalizationString(i, SystemLanguage.English,ObjectHelper.ConvertToNiceName(clipValue.name));
                        }
                        
                        EditorGUILayout.PropertyField(nameProperty);
                        EditorGUI.indentLevel--;
                    }
                    
                    nameProperty.Dispose();
                }
                
                if (animationToDeleteIndex >= 0 )
                {
                    _customAnimations.DeleteArrayElementAtIndex(animationToDeleteIndex);
                    _foldouts.RemoveAt(animationToDeleteIndex);
                }
            }
            else
            {
                GUILayout.Label("No animations");
            }
            
            if (GUILayout.Button("Add", EditorStyles.miniButton))
            {
                AddEmptyCustomAnimation();
            }
            
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnimatorPropertyField()
        {
            EditorGUILayout.PropertyField(_animatorPropertyField); 
            var animatorPropertyValue = _animatorPropertyField.GetValue<Animator>();

            if (!animatorPropertyValue)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                var guiContent = new GUIContent
                {
                    image = EditorGUIUtility.IconContent("console.erroricon").image,
                    text = "Animator component not found"
                };

                var helpBoxLabelStyle = new GUIStyle(EditorStyles.helpBox);
                helpBoxLabelStyle.border = new RectOffset();
                helpBoxLabelStyle.padding = new RectOffset();
                helpBoxLabelStyle.normal.background = null;

                EditorGUILayout.LabelField(guiContent, helpBoxLabelStyle);

                EditorGUILayout.BeginVertical();

                var helpBoxButtonStyle = new GUIStyle(GUI.skin.button);
                helpBoxButtonStyle.margin = new RectOffset(10, 10, 10, 10);

                if (!_animatorComponentOnObject)
                {
                    if (GUILayout.Button("Add Animator", helpBoxButtonStyle))
                    {
                        _animatorComponentOnObject = _varwinAnimationPlayer.gameObject.AddComponent<Animator>();
                        _animatorPropertyField.SetValue(_animatorComponentOnObject);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_varwinAnimationPlayer);
                    }
                }
                else
                {
                    if (GUILayout.Button("Find Animator", helpBoxButtonStyle))
                    {
                        _animatorPropertyField.SetValue(_animatorComponentOnObject);
                        PrefabUtility.RecordPrefabInstancePropertyModifications(_varwinAnimationPlayer);
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
        }

        private void OnEnable()
        {
            _varwinAnimationPlayer = (VarwinAnimationPlayer) serializedObject.targetObject;

            _animatorPropertyField = serializedObject.FindProperty("Animator");
            _customAnimations = serializedObject.FindProperty("CustomAnimations");
                
            _foldouts = Enumerable.Repeat(false, _customAnimations.arraySize).ToList();
        }

        private void Reset()
        {
            if (!_varwinAnimationPlayer)
            {
                _varwinAnimationPlayer = (VarwinAnimationPlayer) serializedObject.targetObject;
            }
            
            _animatorComponentOnObject = _varwinAnimationPlayer.GetComponentInChildren<Animator>(true);
        }

        private void BlendNameProperty(SerializedProperty prop, string name, string[] blendNames)
        {
            if (blendNames == null)
            {
                return;
            }

            var values = new int[blendNames.Length + 1];
            var options = new GUIContent[blendNames.Length + 1];
            values[0] = -1;
            options[0] = new GUIContent("None");

            for (int i = 0; i < blendNames.Length; ++i)
            {
                values[i + 1] = i;
                options[i + 1] = new GUIContent(blendNames[i]);
            }

            EditorGUILayout.IntPopup(prop,
                options,
                values,
                new GUIContent(name));
        }

        private AnimationClip GetCustomAnimationAtIndex(int index)
        {
            var targetObject = _customAnimations.GetArrayElementAtIndex(index).serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();

            if (_customAnimationField == null)
            {
                _customAnimationField = targetObjectClassType.GetField(_customAnimations.propertyPath);
            }
            
            if (_customAnimationField != null)
            {
                _customAnimationList = (List<VarwinCustomAnimation>) _customAnimationField.GetValue(targetObject);
                return _customAnimationList[index].Clip;
            }

            return null;
        }

        private void SetCustomAnimationAtIndex(int index, AnimationClip value)
        {
            var targetObject = _customAnimations.GetArrayElementAtIndex(index).serializedObject.targetObject;
            var targetObjectClassType = targetObject.GetType();

            if (_customAnimationField == null)
            {
                _customAnimationField = targetObjectClassType.GetField(_customAnimations.propertyPath);
            }
            
            if (_customAnimationField != null)
            {
                _customAnimationList = (List<VarwinCustomAnimation>) _customAnimationField.GetValue(targetObject);
                _customAnimationList[index].Clip = value;
            }
        }

        private SerializedProperty AddEmptyCustomAnimation()
        {
            _foldouts.Add(true);
            _customAnimations.InsertArrayElementAtIndex(_customAnimations.arraySize);
                
            SerializedProperty newCustomAnimation = _customAnimations.GetArrayElementAtIndex(_customAnimations.arraySize - 1);
            
            SerializedProperty localizationStrings = newCustomAnimation.FindPropertyRelative("Name").FindPropertyRelative("LocalizationStrings");
            for (int i = localizationStrings.arraySize - 1; i >= 0; --i)
            {
                localizationStrings.DeleteArrayElementAtIndex(i);
            }
            _customAnimations.serializedObject.ApplyModifiedProperties();
            
            SetCustomAnimationAtIndex(_customAnimations.arraySize - 1, null);
            _customAnimations.serializedObject.ApplyModifiedPropertiesWithoutUndo();

            return newCustomAnimation;
        }

        private void AddLocalizationString(int index, SystemLanguage language, string str)
        {
            SerializedProperty customAnimation = _customAnimations.GetArrayElementAtIndex(index);
            SerializedProperty localizationStrings = customAnimation.FindPropertyRelative("Name").FindPropertyRelative("LocalizationStrings");
            
            localizationStrings.InsertArrayElementAtIndex(localizationStrings.arraySize);
            SerializedProperty element = localizationStrings.GetArrayElementAtIndex(localizationStrings.arraySize - 1);

            element.FindPropertyRelative("key").enumValueIndex = (int) language;
            element.FindPropertyRelative("value").stringValue = str;

            customAnimation.serializedObject.ApplyModifiedProperties();
        }
    }
}
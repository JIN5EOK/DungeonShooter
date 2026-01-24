using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DungeonShooter
{
    [CustomEditor(typeof(SkillData))]
    public class SkillDataEditor : Editor
    {
        private readonly string _skillNameName = "_skillName";
        private readonly string _skillDescriptionName = "_skillDescription";
        private readonly string _skillIconName = "_skillIcon";
        private readonly string _activeEffectsName = "_activeEffects";
        private readonly string _passiveEffectsName = "_passiveEffects";
        
        private SerializedProperty _skillNameProperty;
        private SerializedProperty _skillDescriptionProperty;
        private SerializedProperty _skillIconProperty;
        private SerializedProperty _activeEffectsProperty;
        private SerializedProperty _passiveEffectsProperty;
        
        private List<Type> _effectTypes;
        private string[] _effectTypeNames;
        
        private void OnEnable()
        {
            _skillNameProperty = serializedObject.FindProperty(_skillNameName);
            _skillDescriptionProperty = serializedObject.FindProperty(_skillDescriptionName);
            _skillIconProperty = serializedObject.FindProperty(_skillIconName);
            _activeEffectsProperty = serializedObject.FindProperty(_activeEffectsName);
            _passiveEffectsProperty = serializedObject.FindProperty(_passiveEffectsName);
            
            // EffectBase를 상속하는 모든 타입 찾기
            _effectTypes = Jin5eok.ReflectionHelper.GetSubclasses<EffectBase>();
            _effectTypeNames = _effectTypes.Select(type => type.Name).ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(_skillNameProperty);
            EditorGUILayout.PropertyField(_skillDescriptionProperty);
            EditorGUILayout.PropertyField(_skillIconProperty);
            EditorGUILayout.Space();
            
            // 액티브 스킬 효과
            EditorGUILayout.LabelField("액티브 스킬 효과", EditorStyles.boldLabel);
            DrawEffectList(_activeEffectsProperty, 0);
            
            EditorGUILayout.Space(10);
            
            // 패시브 스킬 효과
            EditorGUILayout.LabelField("패시브 스킬 효과", EditorStyles.boldLabel);
            DrawEffectList(_passiveEffectsProperty, 0);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEffectList(SerializedProperty effectsProperty, int depth)
        {
            if (!effectsProperty.isArray)
                return;
            
            // Depth에 따라 들여쓰기 적용
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(depth * 20); // Depth마다 20픽셀 들여쓰기
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            
            for (int i = 0; i < effectsProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // 헤더 영역 (타입 + 삭제 버튼)
                EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
                SerializedProperty elementProperty = effectsProperty.GetArrayElementAtIndex(i);
                
                // 타입 표시
                string typeName = elementProperty.managedReferenceValue == null ? "null" : elementProperty.managedReferenceValue.GetType().Name;
                var originColor = GUI.color;
                GUI.color = Color.green;
                EditorGUILayout.LabelField($"{typeName}", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
                GUI.color = originColor;
                
                // 삭제 버튼
                var originalColor = GUI.color;
                GUI.color = Color.red;
                if (GUILayout.Button("×", GUILayout.Width(25), GUILayout.Height(18), GUILayout.ExpandWidth(false)))
                {
                    effectsProperty.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    GUI.color = originalColor;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space();
                    break;
                }
                GUI.color = originalColor;
                EditorGUILayout.EndHorizontal();
                
                // 속성 표시
                EditorGUI.indentLevel++;
                SerializedProperty iterator = elementProperty.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();

                // 첫 번째 자식으로 이동 후 모든 속성 처리
                if (iterator.Next(true))
                {
                    do
                    {
                        // elementProperty의 직접 자식 범위를 벗어나면 종료
                        if (SerializedProperty.EqualContents(iterator, endProperty))
                            break;
                        
                        // List<EffectBase>라면 재귀로 자식의 이펙트 리스트 그리기, 아니라면 필드만 그리기
                        if (IsEffectBaseList(iterator))
                        {
                            EditorGUILayout.LabelField(iterator.displayName, EditorStyles.boldLabel);
                            DrawEffectList(iterator, depth + 1);
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(iterator, true);
                        }
                    }
                    while (iterator.Next(false)); // 같은 깊이의 다음 속성으로 이동
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }

            // 구분선 추가
            
            EditorGUILayout.Space(5);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUILayout.Space(5);
            
            // 이펙트 추가 버튼
            var addButtonColor = GUI.color;
            GUI.color = new Color(0.5f, 1f, 0.5f);
            
            string buttonLabel = depth == 0 ? "+ Add Effect" : $"+ Add Effect (Depth {depth})";
            if (GUILayout.Button(buttonLabel, GUILayout.Height(25), GUILayout.ExpandWidth(true)))
            {
                ShowAddEffectMenu(effectsProperty);
            }
            GUI.color = addButtonColor;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 이펙트 추가 드롭다운 띄우기
        /// </summary>
        private void ShowAddEffectMenu(SerializedProperty effectsProperty)
        {
            GenericMenu menu = new GenericMenu();
            
            string propertyPath = effectsProperty.propertyPath;
            
            for (int i = 0; i < _effectTypes.Count; i++)
            {
                Type effectType = _effectTypes[i];
                string typeName = _effectTypeNames[i];
                
                menu.AddItem(new GUIContent(typeName), false, () =>
                {
                    serializedObject.Update();
                    
                    SerializedProperty targetProperty = serializedObject.FindProperty(propertyPath);
                    if (targetProperty != null && targetProperty.isArray)
                    {
                        object newEffect = Activator.CreateInstance(effectType);
                        
                        targetProperty.arraySize++;
                        SerializedProperty newElement = targetProperty.GetArrayElementAtIndex(targetProperty.arraySize - 1);
                        newElement.managedReferenceValue = newEffect;
                        
                        serializedObject.ApplyModifiedProperties();
                    }
                });
            }
            
            menu.ShowAsContext();
        }

        private bool IsEffectBaseList(SerializedProperty property)
        {
            // 배열이고 형식 문자열에 EffectBase이 포함되어 있는지 확인
            var isArray = property.isArray;
            var isContainName = property.type.Contains(typeof(EffectBase).Name);
            return isArray && isContainName;
        }
    }
}

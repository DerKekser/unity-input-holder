using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.Helper.Input.Editor
{
    [CustomPropertyDrawer(typeof(InputHolder))]
    public class InputHolderDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent label)
        {
            InputHolder inputHolder = GetTargetObjectOfProperty(property) as InputHolder;
            
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            
            position.height = EditorGUIUtility.singleLineHeight;
            inputHolder._inputAsset = EditorGUI.ObjectField(position, label, inputHolder._inputAsset, typeof(InputActionAsset), false) as InputActionAsset;
            position.y += EditorGUIUtility.singleLineHeight;
            
            if (inputHolder._inputAsset == null)
            {
                EditorGUI.EndChangeCheck();
                EditorGUI.EndProperty();
                
                if (GUI.changed)
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "InputHolder");
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                return;
            }
            
            List<string> actions = new List<string>();
            Dictionary<string, string> actionIds = new Dictionary<string, string>();
            foreach (InputActionMap actionMap in inputHolder._inputAsset.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    string actionName = $"{actionMap.name}/{action.name}";
                    actions.Add(actionName);
                    actionIds.Add(actionName, action.id.ToString());
                }
            }

            int currentIndex = actions.Contains(inputHolder._actionName) ? actions.IndexOf(inputHolder._actionName) : 0;
            int selectedActionName = EditorGUI.Popup(position, " ", currentIndex, actions.ToArray());
            inputHolder._actionName = actions[selectedActionName];
            inputHolder._actionId = actionIds[inputHolder._actionName];

            EditorGUI.EndChangeCheck();
            EditorGUI.EndProperty();
            
            if (GUI.changed)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "InputHolder");
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InputHolder inputHolder = GetTargetObjectOfProperty(property) as InputHolder;
            if (inputHolder != null && inputHolder._inputAsset != null)
                return EditorGUIUtility.singleLineHeight * 2;
            return EditorGUIUtility.singleLineHeight;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop, object targetObj)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    targetObj = GetValue_Imp(targetObj, elementName, index);
                }
                else
                {
                    targetObj = GetValue_Imp(targetObj, element);
                }
            }
            return targetObj;
        }
        
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}
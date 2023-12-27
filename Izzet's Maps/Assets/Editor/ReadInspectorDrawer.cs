using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SCR_utils.customAttributes.ReadOnlyAttribute))]
public class ReadInspectorDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label);
        GUI.enabled = true;
    }
}

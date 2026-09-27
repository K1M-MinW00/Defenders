#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RewardData))]
public sealed class RewardDataDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        SerializedProperty type = property.FindPropertyRelative("type");
        bool needsId = NeedsId((RewardType)type.enumValueIndex);
        int lines = needsId ? 4 : 3;
        return lines * EditorGUIUtility.singleLineHeight +
               (lines - 1) * EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect line = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(line, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            SerializedProperty type = property.FindPropertyRelative("type");
            SerializedProperty id = property.FindPropertyRelative("id");
            SerializedProperty amount = property.FindPropertyRelative("amount");

            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(line, type, new GUIContent("Reward Type"));

            if (NeedsId((RewardType)type.enumValueIndex))
            {
                line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.PropertyField(line, id, new GUIContent("Catalog ID"));
            }
            else
            {
                id.stringValue = string.Empty;
            }

            line.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(line, amount, new GUIContent("Amount"));
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private static bool NeedsId(RewardType type)
    {
        return type is RewardType.Item or RewardType.Equipment or RewardType.Unit;
    }
}
#endif

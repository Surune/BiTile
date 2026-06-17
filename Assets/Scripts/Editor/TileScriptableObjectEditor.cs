using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileScriptableObject))]
public class TileScriptableObjectEditor : Editor
{
    private const int PatternSize = 9;
    private const int PatternWidth = 3;
    private const float CellSize = 18f;

    private SerializedProperty typeNameProperty;
    private SerializedProperty modelProperty;
    private SerializedProperty flipPatternProperty;

    private void OnEnable()
    {
        typeNameProperty = serializedObject.FindProperty("typeName");
        modelProperty = serializedObject.FindProperty("model");
        flipPatternProperty = serializedObject.FindProperty("flipPattern");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.PropertyField(typeNameProperty);
        EditorGUILayout.PropertyField(modelProperty);

        ResizePattern();
        DrawFlipPattern();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawFlipPattern()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Flip Pattern", EditorStyles.boldLabel);

        for (var row = 0; row < PatternWidth; row++)
        {
            EditorGUILayout.BeginHorizontal();

            for (var col = 0; col < PatternWidth; col++)
            {
                var index = row * PatternWidth + col;
                var cell = flipPatternProperty.GetArrayElementAtIndex(index);
                cell.boolValue = EditorGUILayout.Toggle(cell.boolValue, GUILayout.Width(CellSize));
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private void ResizePattern()
    {
        while (flipPatternProperty.arraySize < PatternSize)
        {
            flipPatternProperty.InsertArrayElementAtIndex(flipPatternProperty.arraySize);
            flipPatternProperty.GetArrayElementAtIndex(flipPatternProperty.arraySize - 1).boolValue = false;
        }

        while (flipPatternProperty.arraySize > PatternSize)
        {
            flipPatternProperty.DeleteArrayElementAtIndex(flipPatternProperty.arraySize - 1);
        }
    }
}

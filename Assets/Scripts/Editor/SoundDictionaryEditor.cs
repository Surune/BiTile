using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundDictionary))]
public class SoundDictionaryEditor : TableViewEditorBase
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        if (GUILayout.Button("Sort by Sound Type"))
        {
            SortSounds();
        }
    }

    private void SortSounds()
    {
        serializedObject.Update();

        var sounds = serializedObject.FindProperty("sounds");
        for (var i = 0; i < sounds.arraySize - 1; i++)
        {
            var minIndex = i;
            var minValue = GetSoundTypeValue(sounds, i);

            for (var j = i + 1; j < sounds.arraySize; j++)
            {
                var value = GetSoundTypeValue(sounds, j);
                if (value < minValue)
                {
                    minIndex = j;
                    minValue = value;
                }
            }

            if (minIndex != i)
            {
                sounds.MoveArrayElement(minIndex, i);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static int GetSoundTypeValue(SerializedProperty sounds, int index)
    {
        return sounds.GetArrayElementAtIndex(index).FindPropertyRelative("soundType").intValue;
    }
}

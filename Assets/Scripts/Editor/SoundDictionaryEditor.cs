using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SoundDictionary))]
public class SoundDictionaryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
        GUI.enabled = true;

        var sounds = serializedObject.FindProperty("sounds");
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Sound Type", GUILayout.Width(130f));
        GUILayout.Label("Audio Clips", GUILayout.ExpandWidth(true));
        GUILayout.Space(24f);
        EditorGUILayout.EndHorizontal();

        for (var i = 0; i < sounds.arraySize; i++)
        {
            var sound = sounds.GetArrayElementAtIndex(i);
            var clips = sound.FindPropertyRelative("audioClips");

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(sound.FindPropertyRelative("soundType"), GUIContent.none,
                GUILayout.Width(130f));

            EditorGUILayout.BeginVertical();
            for (var j = 0; j < clips.arraySize; j++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(clips.GetArrayElementAtIndex(j), GUIContent.none);
                var removeClip = GUILayout.Button("-", GUILayout.Width(24f));
                EditorGUILayout.EndHorizontal();

                if (removeClip)
                {
                    clips.GetArrayElementAtIndex(j).objectReferenceValue = null;
                    clips.DeleteArrayElementAtIndex(j);
                    break;
                }
            }

            if (GUILayout.Button("Add Clip"))
            {
                clips.InsertArrayElementAtIndex(clips.arraySize);
                clips.GetArrayElementAtIndex(clips.arraySize - 1).objectReferenceValue = null;
            }
            EditorGUILayout.EndVertical();

            var removeSound = GUILayout.Button("-", GUILayout.Width(24f));
            EditorGUILayout.EndHorizontal();

            if (removeSound)
            {
                sounds.DeleteArrayElementAtIndex(i);
                break;
            }
        }

        if (GUILayout.Button("Add Sound Type"))
        {
            sounds.InsertArrayElementAtIndex(sounds.arraySize);
            var sound = sounds.GetArrayElementAtIndex(sounds.arraySize - 1);
            sound.FindPropertyRelative("soundType").intValue = (int)Definitions.SoundType.None;
            sound.FindPropertyRelative("audioClips").ClearArray();
        }

        serializedObject.ApplyModifiedProperties();

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

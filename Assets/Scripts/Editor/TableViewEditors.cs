using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class TableViewMonoBehaviourEditor : TableViewEditorBase
{
}

[CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
public class TableViewScriptableObjectEditor : TableViewEditorBase
{
}

public abstract class TableViewEditorBase : Editor
{
    private const float IndexColumnWidth = 32f;
    private const float RemoveButtonWidth = 28f;
    private const float ColumnSpacing = 4f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var iterator = serializedObject.GetIterator();
        var enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            using (new EditorGUI.DisabledScope(iterator.propertyPath == "m_Script"))
            {
                var property = iterator.Copy();

                if (TryGetTableViewField(property, out var fieldInfo))
                {
                    DrawTable(property, fieldInfo);
                }
                else
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTable(SerializedProperty property, FieldInfo fieldInfo)
    {
        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, property.displayName, true);
        if (!property.isExpanded)
        {
            return;
        }

        var columns = GetColumns(fieldInfo);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField($"Size: {property.arraySize}", EditorStyles.miniLabel);
            if (GUILayout.Button("Add", GUILayout.Width(60f)))
            {
                property.InsertArrayElementAtIndex(property.arraySize);
                ResetRow(property.GetArrayElementAtIndex(property.arraySize - 1));
            }
        }

        if (columns.Count > 0)
        {
            DrawHeader(columns);
        }

        for (var i = 0; i < property.arraySize; i++)
        {
            DrawRow(property, property.GetArrayElementAtIndex(i), i, columns);
        }
    }

    private static void DrawHeader(List<ColumnInfo> columns)
    {
        var controlRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
        var columnWidth = GetColumnWidth(controlRect.width, columns.Count);

        var indexRect = new Rect(controlRect.x, controlRect.y, IndexColumnWidth, controlRect.height);
        EditorGUI.LabelField(indexRect, "#", EditorStyles.miniBoldLabel);

        for (var i = 0; i < columns.Count; i++)
        {
            var x = controlRect.x + IndexColumnWidth + ColumnSpacing + i * (columnWidth + ColumnSpacing);
            var rect = new Rect(x, controlRect.y, columnWidth, controlRect.height);
            EditorGUI.LabelField(rect, columns[i].displayName, EditorStyles.miniBoldLabel);
        }
    }

    private void DrawRow(SerializedProperty arrayProperty, SerializedProperty rowProperty, int index, List<ColumnInfo> columns)
    {
        if (columns.Count == 0)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(index.ToString(), GUILayout.Width(IndexColumnWidth));
                EditorGUILayout.PropertyField(rowProperty, GUIContent.none, true);

                if (GUILayout.Button("-", GUILayout.Width(RemoveButtonWidth)))
                {
                    DeleteElement(arrayProperty, index);
                }
            }

            return;
        }

        var rowHeight = GetRowHeight(rowProperty, columns);
        var controlRect = EditorGUILayout.GetControlRect(false, rowHeight);
        var columnWidth = GetColumnWidth(controlRect.width, columns.Count);

        var indexRect = new Rect(controlRect.x, controlRect.y, IndexColumnWidth, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(indexRect, index.ToString(), EditorStyles.miniLabel);

        for (var i = 0; i < columns.Count; i++)
        {
            var columnProperty = rowProperty.FindPropertyRelative(columns[i].name);
            var x = controlRect.x + IndexColumnWidth + ColumnSpacing + i * (columnWidth + ColumnSpacing);
            var rect = new Rect(x, controlRect.y, columnWidth, controlRect.height);
            EditorGUI.PropertyField(rect, columnProperty, GUIContent.none, true);
        }

        var buttonRect = new Rect(controlRect.xMax - RemoveButtonWidth, controlRect.y, RemoveButtonWidth, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(buttonRect, "-"))
        {
            DeleteElement(arrayProperty, index);
        }
    }

    private static float GetRowHeight(SerializedProperty rowProperty, List<ColumnInfo> columns)
    {
        var rowHeight = EditorGUIUtility.singleLineHeight;

        for (var i = 0; i < columns.Count; i++)
        {
            var columnProperty = rowProperty.FindPropertyRelative(columns[i].name);
            var columnHeight = EditorGUI.GetPropertyHeight(columnProperty, true);
            if (columnHeight > rowHeight)
            {
                rowHeight = columnHeight;
            }
        }

        return rowHeight;
    }

    private static float GetColumnWidth(float totalWidth, int columnCount)
    {
        var widthWithoutButton = totalWidth - IndexColumnWidth - RemoveButtonWidth - (ColumnSpacing * 2f);
        var totalSpacing = ColumnSpacing * (columnCount - 1);
        return (widthWithoutButton - totalSpacing) / columnCount;
    }

    private static bool TryGetTableViewField(SerializedProperty property, out FieldInfo fieldInfo)
    {
        fieldInfo = GetField(property.serializedObject.targetObject.GetType(), property.name);
        return fieldInfo != null && Attribute.IsDefined(fieldInfo, typeof(TableViewAttribute));
    }

    private static FieldInfo GetField(Type type, string fieldName)
    {
        while (type != null)
        {
            var fieldInfo = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo != null)
            {
                return fieldInfo;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static List<ColumnInfo> GetColumns(FieldInfo fieldInfo)
    {
        var elementType = GetElementType(fieldInfo.FieldType);
        if (elementType == null)
        {
            return new List<ColumnInfo>();
        }

        var columns = new List<ColumnInfo>();
        var fields = elementType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            if (field.IsStatic)
            {
                continue;
            }

            if (!field.IsPublic && !Attribute.IsDefined(field, typeof(SerializeField)))
            {
                continue;
            }

            if (Attribute.IsDefined(field, typeof(NonSerializedAttribute)))
            {
                continue;
            }

            columns.Add(new ColumnInfo(field.Name, ObjectNames.NicifyVariableName(field.Name)));
        }

        return columns;
    }

    private static Type GetElementType(Type fieldType)
    {
        if (fieldType.IsArray)
        {
            return fieldType.GetElementType();
        }

        if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return fieldType.GetGenericArguments()[0];
        }

        return null;
    }

    private static void DeleteElement(SerializedProperty arrayProperty, int index)
    {
        var element = arrayProperty.GetArrayElementAtIndex(index);
        var objectReference = element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue != null;
        arrayProperty.DeleteArrayElementAtIndex(index);
        if (objectReference)
        {
            arrayProperty.DeleteArrayElementAtIndex(index);
        }
    }

    private static void ResetRow(SerializedProperty rowProperty)
    {
        if (rowProperty.propertyType == SerializedPropertyType.ObjectReference)
        {
            rowProperty.objectReferenceValue = null;
            return;
        }

        if (rowProperty.propertyType != SerializedPropertyType.Generic)
        {
            return;
        }

        var iterator = rowProperty.Copy();
        var endProperty = iterator.GetEndProperty();
        var enterChildren = true;

        while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
        {
            if (iterator.depth == rowProperty.depth + 1)
            {
                switch (iterator.propertyType)
                {
                    case SerializedPropertyType.Integer:
                        iterator.intValue = 0;
                        break;
                    case SerializedPropertyType.Boolean:
                        iterator.boolValue = false;
                        break;
                    case SerializedPropertyType.Float:
                        iterator.floatValue = 0f;
                        break;
                    case SerializedPropertyType.String:
                        iterator.stringValue = string.Empty;
                        break;
                    case SerializedPropertyType.ObjectReference:
                        iterator.objectReferenceValue = null;
                        break;
                    case SerializedPropertyType.Enum:
                        iterator.enumValueIndex = 0;
                        break;
                }
            }

            enterChildren = false;
        }
    }

    private readonly struct ColumnInfo
    {
        public readonly string name;
        public readonly string displayName;

        public ColumnInfo(string name, string displayName)
        {
            this.name = name;
            this.displayName = displayName;
        }
    }
}

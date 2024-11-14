using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GravityField))]
public class GravityFieldDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Zeichne die Standardfelder
        //DrawDefaultInspector();

        // Zeichne das gravityStrength-Feld
        SerializedProperty gravityStrengthProp = serializedObject.FindProperty("gravityStrength");
        EditorGUILayout.PropertyField(gravityStrengthProp, new GUIContent("Gravity Strength"));

        // Zeichne das priority-Feld
        SerializedProperty priorityProp = serializedObject.FindProperty("priority");
        EditorGUILayout.PropertyField(priorityProp, new GUIContent("Priority"));

        // Zeichne das priority-Feld
        SerializedProperty gravityFieldMassProp = serializedObject.FindProperty("gravityFieldMass");
        EditorGUILayout.PropertyField(gravityFieldMassProp, new GUIContent("Gravity Field Mass"));

        // Zeichne das Enum-Feld für GravityFieldType
        SerializedProperty gravityFieldTypeProp = serializedObject.FindProperty("gravityFieldType");
        EditorGUILayout.PropertyField(gravityFieldTypeProp, new GUIContent("Gravity Field Type"));

        // Zeichne das meshCollider-Feld nur, wenn GravityFieldType auf MeshBased gesetzt ist
        if ((GravityFieldType)gravityFieldTypeProp.enumValueIndex == GravityFieldType.MeshBased)
        {
            SerializedProperty meshColliderProp = serializedObject.FindProperty("meshCollider");
            EditorGUILayout.PropertyField(meshColliderProp, new GUIContent("Mesh Collider"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}

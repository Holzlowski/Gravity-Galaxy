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

        // Zeichne das gravityFieldMass-Feld
        SerializedProperty gravityFieldMassProp = serializedObject.FindProperty("gravityFieldMass");
        EditorGUILayout.PropertyField(gravityFieldMassProp, new GUIContent("Gravity Field Mass"));

        // Zeichne das Enum-Feld für GravityFieldType
        SerializedProperty gravityFieldTypeProp = serializedObject.FindProperty("gravityFieldType");
        EditorGUILayout.PropertyField(gravityFieldTypeProp, new GUIContent("Gravity Field Type"));

        // Zeichne das Enum-Feld für GravityDelay
        SerializedProperty gravityDelay = serializedObject.FindProperty("gravityDelay");
        EditorGUILayout.PropertyField(gravityDelay, new GUIContent("Gravity Delay"));

        if ((GravityFieldType)gravityFieldTypeProp.enumValueIndex == GravityFieldType.SimpleMesh)
        {
            SerializedProperty meshColliderProp = serializedObject.FindProperty("simpleCollider");
            EditorGUILayout.PropertyField(meshColliderProp, new GUIContent("Collider"));
        }

        if ((GravityFieldType)gravityFieldTypeProp.enumValueIndex == GravityFieldType.OneDirection)
        {
            SerializedProperty gravityDirectionType = serializedObject.FindProperty("gravityDirectionType");
            EditorGUILayout.PropertyField(gravityDirectionType, new GUIContent("Gravity Direction Type"));
        }


        if ((GravityFieldType)gravityFieldTypeProp.enumValueIndex == GravityFieldType.LowPolyMeshKDTree)
        {
            SerializedProperty meshColliderProp = serializedObject.FindProperty("meshCollider");
            EditorGUILayout.PropertyField(meshColliderProp, new GUIContent("Mesh Collider"));

            SerializedProperty thresholdDistance = serializedObject.FindProperty("thresholdDistance");
            EditorGUILayout.PropertyField(thresholdDistance, new GUIContent("Threshold Distance"));

            SerializedProperty neighborThresholdDistance = serializedObject.FindProperty("neighborThresholdDistance");
            EditorGUILayout.PropertyField(neighborThresholdDistance, new GUIContent("Neighbour Threshold Distance"));

            SerializedProperty smoothingFactor = serializedObject.FindProperty("smoothingFactor");
            EditorGUILayout.PropertyField(smoothingFactor, new GUIContent("Smoothing Factor"));
        }

        if ((GravityFieldType)gravityFieldTypeProp.enumValueIndex == GravityFieldType.HighPolyMeshKDTree)
        {
            SerializedProperty meshColliderProp = serializedObject.FindProperty("meshCollider");
            EditorGUILayout.PropertyField(meshColliderProp, new GUIContent("Mesh Collider"));
        }

        if ((GravityFieldType)gravityFieldTypeProp.enumValueIndex == GravityFieldType.CalculateInterpolatedGravityWithNeighbors)
        {
            SerializedProperty meshColliderProp = serializedObject.FindProperty("meshCollider");
            EditorGUILayout.PropertyField(meshColliderProp, new GUIContent("Mesh Collider"));

        }


        serializedObject.ApplyModifiedProperties();
    }
}

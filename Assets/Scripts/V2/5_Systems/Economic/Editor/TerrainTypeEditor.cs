#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using V1.Data;

/// <summary>
/// Custom editor for TerrainTypeDataSO to provide a better editing experience
/// </summary>
[CustomEditor(typeof(TerrainTypeDataSO))]
public class TerrainTypeEditor : Editor
{
    SerializedProperty terrainName;
    SerializedProperty description;
    SerializedProperty baseColor;
    SerializedProperty terrainIcon;
    SerializedProperty terrainTexture;
    SerializedProperty fertilityMultiplier;
    SerializedProperty mineralsMultiplier;
    SerializedProperty productionMultiplier;
    SerializedProperty commerceMultiplier;
    SerializedProperty isPassable;
    SerializedProperty isNaturalBorder;
    SerializedProperty specialResourceChance;
    SerializedProperty specialEffects;

    void OnEnable()
    {
        // Initialize the serialized properties
        terrainName = serializedObject.FindProperty("terrainName");
        description = serializedObject.FindProperty("description");
        baseColor = serializedObject.FindProperty("baseColor");
        terrainIcon = serializedObject.FindProperty("terrainIcon");
        terrainTexture = serializedObject.FindProperty("terrainTexture");
        fertilityMultiplier = serializedObject.FindProperty("fertilityMultiplier");
        mineralsMultiplier = serializedObject.FindProperty("mineralsMultiplier");
        productionMultiplier = serializedObject.FindProperty("productionMultiplier");
        commerceMultiplier = serializedObject.FindProperty("commerceMultiplier");
        isPassable = serializedObject.FindProperty("isPassable");
        isNaturalBorder = serializedObject.FindProperty("isNaturalBorder");
        specialResourceChance = serializedObject.FindProperty("specialResourceChance");
        specialEffects = serializedObject.FindProperty("specialEffects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Basic information section
        EditorGUILayout.LabelField("Basic Information", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(terrainName);
        EditorGUILayout.PropertyField(description);
        EditorGUILayout.Space();

        // Visual properties section
        EditorGUILayout.LabelField("Visual Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(baseColor);
        EditorGUILayout.PropertyField(terrainIcon);
        EditorGUILayout.PropertyField(terrainTexture);
        
        // Show color preview
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Color Preview");
        GUILayout.Box("", GUILayout.Height(20), GUILayout.Width(100));
        if (Event.current.type == EventType.Repaint)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, baseColor.colorValue);
            tex.Apply();
            GUIStyle colorStyle = new GUIStyle();
            colorStyle.normal.background = tex;
            GUI.Box(GUILayoutUtility.GetLastRect(), GUIContent.none, colorStyle);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Economic properties section
        EditorGUILayout.LabelField("Economic Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(fertilityMultiplier);
        EditorGUILayout.PropertyField(mineralsMultiplier);
        EditorGUILayout.PropertyField(productionMultiplier);
        EditorGUILayout.PropertyField(commerceMultiplier);
        EditorGUILayout.Space();

        // Special properties section
        EditorGUILayout.LabelField("Special Properties", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(isPassable);
        EditorGUILayout.PropertyField(isNaturalBorder);
        EditorGUILayout.PropertyField(specialResourceChance);
        EditorGUILayout.PropertyField(specialEffects);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif

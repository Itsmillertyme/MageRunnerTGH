using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        // Draw the default inspector first
        DrawDefaultInspector();

        // Reference to the actual target
        LevelGenerator generator = (LevelGenerator) target;

        GUILayout.Space(10);

        if (GUILayout.Button("Generate Level")) {
            // Register undo
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Generate Level");

            generator.GenerateLevel();

            // Mark scene dirty
            EditorUtility.SetDirty(generator);
        }

        if (GUILayout.Button("Clear Level")) {
            // Register undo
            Undo.RegisterFullObjectHierarchyUndo(generator.gameObject, "Clear Level");

            generator.ClearLevel();

            // Mark scene dirty
            EditorUtility.SetDirty(generator);
        }
    }
}

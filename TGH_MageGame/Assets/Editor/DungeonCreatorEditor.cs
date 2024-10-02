using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonCreator))]
public class DungeonCreatorEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        DungeonCreator dungeonCreator = (DungeonCreator) target;
        if (GUILayout.Button("Clear Dungeon")) {
            dungeonCreator.ClearDungeon();
        }

        if (GUILayout.Button("Create New Dungeon")) {
            dungeonCreator.RetryGeneration();
        }
    }
}

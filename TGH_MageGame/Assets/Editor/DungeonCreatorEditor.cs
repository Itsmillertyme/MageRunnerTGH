using UnityEditor;

[CustomEditor(typeof(GameManager))]
public class DungeonCreatorEditor : Editor {
    //public override void OnInspectorGUI() {
    //    base.OnInspectorGUI();
    //    DungeonCreator dungeonCreator = target.GetComponent<DungeonCreator>();
    //    //DungeonCreator dungeonCreator = (DungeonCreator) target;
    //    if (GUILayout.Button("Clear Dungeon")) {
    //        SceneView.RepaintAll();
    //        dungeonCreator.ClearDungeon();
    //    }

    //    if (GUILayout.Button("Create New Dungeon")) {
    //        dungeonCreator.RetryGeneration();
    //    }

    //    if (GUILayout.Button("Save Seed")) {
    //        dungeonCreator.SaveSeed();
    //    }
    //}
}

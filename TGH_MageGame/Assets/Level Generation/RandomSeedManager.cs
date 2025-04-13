using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RandomSeedManager {
    //**PROPERTIES**
    static string SEED_DATA_FILE_PATH = Path.Combine(Application.streamingAssetsPath, "LevelSeedData.json");

    //**STATIC METHODS**
    //Sets the Unity Random object's starting seed
    public static void SetSeed(int seed) {
        UnityEngine.Random.InitState(seed);
    }
    //
    //Generates a random seed and then sets the Unity Random object's starting seed
    public static int SetRandomSeed() {

        int seed = UnityEngine.Random.Range(0, int.MaxValue);

        SetSeed(seed);

        return seed;
    }
    //
    //Prints out seed data to Unity console
    public static void DebugLevelSeeds() {
        if (File.Exists(SEED_DATA_FILE_PATH)) {
            try {
                //Read in file
                string json = File.ReadAllText(SEED_DATA_FILE_PATH);
                //Parse JSON
                LevelDataList levelDataList = JsonUtility.FromJson<LevelDataList>(json);

                //Print out seeds
                foreach (LevelData level in levelDataList.levels) {
                    Debug.Log($"Level {level.levelIndex}:\n\t {string.Join("\n\t", level.seeds)}");
                }
            }
            catch (Exception e) {
                Debug.LogError("Error reading JSON file: " + e.Message);
            }
        }
        else {
            Debug.LogWarning("No seed file found");
        }
    }
    //
    //Save a given seed to a given level index;
    public static bool SaveSeedToJSON(int index, int seed) {

        if (!File.Exists(SEED_DATA_FILE_PATH)) {
            Debug.LogError("JSON file not found: " + SEED_DATA_FILE_PATH);
            return false;
        }

        try {
            //Read in file
            string json = File.ReadAllText(SEED_DATA_FILE_PATH);
            //Parse JSON
            LevelDataList levelDataList = JsonUtility.FromJson<LevelDataList>(json);

            //Find level from index
            LevelData level = levelDataList.levels.Find(l => l.levelIndex == index);

            if (level != null) {

                //append if not already present
                if (!level.seeds.Contains(seed)) {
                    level.seeds.Add(seed);
                    Debug.Log($"Seed {seed} added to level {index}.");
                }
                else {
                    Debug.LogWarning($"Seed {seed} already exists in level {index}.");
                    return false;
                }
            }
            else {
                Debug.LogError($"Level {index} not found in JSON.");
                return false;
            }

            //Convert back to JSON and write
            string updatedLevelDataJSON = JsonUtility.ToJson(levelDataList, true);
            File.WriteAllText(SEED_DATA_FILE_PATH, updatedLevelDataJSON);
            Debug.Log("JSON file updated successfully.");
        }
        catch (Exception e) {
            Debug.LogError("Error reading JSON file: " + e.Message);
            return false;
        }

        return true;
    }

    //**JSON HELPER CLASSES**
    [Serializable]
    public class LevelData {
        public int levelIndex;
        public List<int> seeds = new List<int>(); // List of seeds per level
    }

    [Serializable]
    public class LevelDataList {
        public List<LevelData> levels = new List<LevelData>();
    }
}

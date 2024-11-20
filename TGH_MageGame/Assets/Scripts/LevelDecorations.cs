using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelDecorations : ScriptableObject {
    [SerializeField] List<GameObject> decorationPrefabs;

    public List<GameObject> DecorationPrefabs { get => decorationPrefabs; }
}

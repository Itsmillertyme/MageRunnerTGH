using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshStitcher {

    List<MeshFilter> meshes;
    GameObject meshesParent;
    CombineInstance[] combineInstances;
    Mesh newMesh;
    public Mesh NewMesh { get => newMesh; }
    public MeshStitcher(GameObject meshesParentIn) {
        meshesParent = meshesParentIn;
        meshes = meshesParent.GetComponentsInChildren<MeshFilter>().ToList();
        combineInstances = new CombineInstance[meshes.Count];

        StitchMesh();
    }



    private void StitchMesh() {

        for (int i = 0; i < combineInstances.Length; i++) {
            combineInstances[i].mesh = meshes[i].sharedMesh;
            combineInstances[i].transform = meshes[i].transform.localToWorldMatrix;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.CombineMeshes(combineInstances);
        newMesh = mesh;
    }
}

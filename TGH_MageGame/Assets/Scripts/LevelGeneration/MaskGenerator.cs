using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour {

    [SerializeField] GameObject maskPrefab; //SII
    [SerializeField] Transform maskParent; //SII
    [SerializeField] Material maskMat; //SII
    [SerializeField] Material altMat; //SII
    [SerializeField] int maskOverflow; //SII

    public void GenerateMaskMesh(List<RoomNode> rooms, List<CorridorNode> corridors, int dungeonWidth, int dungeonHeight) {

        //Loop through every space in dungeon width
        for (int i = -maskOverflow; i < dungeonWidth + maskOverflow; i++) {
            //Loop through every space in dungeon height
            for (int j = -maskOverflow; j < dungeonHeight + maskOverflow; j++) {
                //Flag
                bool outsideOfRoom = true;

                //Loop through each room
                foreach (RoomNode room in rooms) {
                    //Check if this space is inside of room bounds
                    if (j > room.TopLeftAreaCorner.x - 1 &&
                        j < room.TopRightAreaCorner.x &&
                        i > room.BottomLeftAreaCorner.y &&
                        i < room.TopLeftAreaCorner.y + 1) {
                        //Set flag
                        outsideOfRoom = false;
                    }
                }
                //Loop through each corridor
                foreach (Node corridor in corridors) {
                    //Check if this space is inside of room bounds
                    if (j > corridor.TopLeftAreaCorner.x - 1 &&
                        j < corridor.TopRightAreaCorner.x &&
                        i > corridor.BottomLeftAreaCorner.y &&
                        i < corridor.TopLeftAreaCorner.y + 1) {
                        //Set flag
                        outsideOfRoom = false;
                    }
                }

                //Test flag
                if (outsideOfRoom) {
                    //Create mask prefab for this cell
                    GameObject mask = Instantiate(maskPrefab, new Vector3(j, 3, i), Quaternion.Euler(0, 0, 0), maskParent);
                }
            }
        }

        //Create composite mesh of all masks
        MeshStitcher stitcher = new MeshStitcher(maskParent.gameObject);

        //Destroy all mask GameObjects
        while (GameObject.Find("Mask(Clone)") != null) {
            //IN BUILD SET TO DESTROY GAMEOBJECTS
            //GameObject.Find("Mask(Clone)").SetActive(false);
            DestroyImmediate(GameObject.Find("Mask(Clone)"));
        }

        //Create new GameObject for composite mesh
        GameObject newMeshObj = new GameObject("LevelMask", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        //Setup GameObject
        newMeshObj.transform.position = Vector3.zero;
        newMeshObj.transform.parent = maskParent.transform;
        newMeshObj.transform.SetSiblingIndex(0);
        //Attach mesh
        newMeshObj.GetComponent<MeshFilter>().sharedMesh = stitcher.NewMesh;
        //Set material
        newMeshObj.GetComponent<MeshRenderer>().material = maskMat;
    }
}

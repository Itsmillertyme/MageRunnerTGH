using System.Collections.Generic;
using UnityEngine;

public class MaskGeneratorPG2 : MonoBehaviour {

    [SerializeField] GameObject maskPrefab; //SII
    [SerializeField] Transform maskParent; //SII
    [SerializeField] Material maskMat; //SII
    [SerializeField] Material altMat; //SII

    public void GenerateMaskMesh(List<RoomData> rooms, int dungeonWidth, int dungeonHeight) {

        //Loop through every space in level width
        for (int i = 0; i < dungeonWidth; i++) {
            //Loop through every space in level height
            for (int j = 0; j < dungeonHeight; j++) {
                //Flag
                bool outsideOfRoom = true;

                //Loop through each room
                foreach (RoomData room in rooms) {
                    //Check if this space is inside of room bounds
                    if (j < room.TopLeftObject.position.y + 1 &&
                        j > room.BottomRightObject.position.y &&
                        i > room.TopLeftObject.position.x - 1 &&
                        i < room.BottomRightObject.position.x) {
                        //Set flag
                        outsideOfRoom = false;
                    }
                }
                //Test flag
                if (outsideOfRoom) {
                    //Create mask prefab for this cell
                    GameObject mask = Instantiate(maskPrefab, new Vector3(i, j, -5.2f), Quaternion.Euler(-90, 0, 0), maskParent);
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

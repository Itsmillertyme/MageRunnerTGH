using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour {
    List<Transform> walls;
    [SerializeField] GameObject maskPrefab; //SII
    [SerializeField] GameObject maskParent; //SII
    [SerializeField] Material maskMat; //SII
    [SerializeField] int debugRayTime; //SII


    public void GenerateMaskMesh(List<Transform> wallsIn) {
        walls = wallsIn;
        int counter = 0;
        for (int i = 0; i < walls.Count; i++) {
            Vector3 dir = walls[i].GetChild(0).forward.normalized;
            Vector3 rayStartPos = new Vector3();

            Direction faceDir = Direction.UP;
            switch (walls[i].GetChild(0).transform.rotation.eulerAngles.y) {
                case 0:
                    faceDir = Direction.LEFT;
                    rayStartPos = new Vector3(walls[i].position.x + .5f, 2f, walls[i].position.z);
                    break;
                case 90:
                    faceDir = Direction.UP;
                    rayStartPos = new Vector3(walls[i].position.x, 2f, walls[i].position.z + .5f);
                    break;
                case 180:
                    faceDir = Direction.RIGHT;
                    rayStartPos = new Vector3(walls[i].position.x + .5f, 2f, walls[i].position.z);
                    break;
                case 270:
                    faceDir = Direction.DOWN;
                    rayStartPos = new Vector3(walls[i].position.x, 2f, walls[i].position.z + .5f);
                    break;
            }


            //DEBUG RAYS
            //Debug.DrawRay(rayStartPos + dir * 0.1f, dir, Color.red, debugRayTime);
            //Vector3 maskCheckDebugStartPo = new Vector3(rayStartPos.x, rayStartPos.y + 1.5f, rayStartPos.z);
            //Debug.DrawRay(maskCheckDebugStartPo + dir * 0.1f, new Vector3(dir.x * 1, dir.y - .51f, dir.z * 1), Color.green, debugRayTime);



            Ray wallRay = new Ray(rayStartPos + dir * 0.1f, dir);
            //Ray maskRay = new Ray(maskCheckDebugStartPo + dir * 0.1f, new Vector3(dir.x * 1, dir.y - .51f, dir.z * 1));

            RaycastHit wallHitInfo = new RaycastHit();
            //RaycastHit maskHitInfo = new RaycastHit();

            Physics.Raycast(wallRay, out wallHitInfo);
            //Physics.Raycast(maskRay, out maskHitInfo);




            int dist = -1;
            if (wallHitInfo.collider != null) {
                dist = Mathf.RoundToInt(wallHitInfo.distance);
            }

            //if (maskHitInfo.collider == null && dist < 20) {
            //Vector3 maskSpawnPos = new Vector3(walls[i].position.x, 3f, walls[i].position.z);
            //Debug.Log(faceDir);

            for (int j = 0; j < (dist < 0 || dist > 100 ? 100 : (dist / 2) + 1); j++) {
                counter++;
                int index = j;
                if (faceDir == Direction.LEFT) {
                    index *= -1;
                }
                if (faceDir == Direction.UP) {
                    index *= -1;
                }

                Vector3 prefabSpawnPos = new Vector3(walls[i].position.x, 3f, walls[i].position.z);
                Vector3 maskCheckRaySpawnPos = prefabSpawnPos;
                Vector3 maskCheckRayDir = prefabSpawnPos;

                switch (faceDir) {
                    case Direction.UP:
                        prefabSpawnPos = new Vector3(prefabSpawnPos.x - index, prefabSpawnPos.y, prefabSpawnPos.z + 1);
                        maskCheckRaySpawnPos = new Vector3(maskCheckRaySpawnPos.x, maskCheckRaySpawnPos.y + 5f, maskCheckRaySpawnPos.z + .5f);
                        maskCheckRayDir = new Vector3(dir.x * -index, dir.y - 5.01f, dir.z * index);
                        //Debug.DrawRay(maskCheckRaySpawnPos, maskCheckRayDir, Color.green, debugRayTime);
                        break;
                    case Direction.DOWN:
                        prefabSpawnPos = new Vector3(prefabSpawnPos.x - index - 1, prefabSpawnPos.y, prefabSpawnPos.z + 1);
                        maskCheckRaySpawnPos = new Vector3(maskCheckRaySpawnPos.x, maskCheckRaySpawnPos.y + 5f, maskCheckRaySpawnPos.z + .5f);
                        maskCheckRayDir = new Vector3(dir.x * index, dir.y - 5.01f, dir.z * index);
                        //Debug.DrawRay(maskCheckRaySpawnPos, maskCheckRayDir, Color.green, debugRayTime);
                        break;
                    case Direction.RIGHT:
                        prefabSpawnPos = new Vector3(prefabSpawnPos.x, prefabSpawnPos.y, prefabSpawnPos.z - index);
                        maskCheckRaySpawnPos = new Vector3(maskCheckRaySpawnPos.x + .5f, maskCheckRaySpawnPos.y + 5f, maskCheckRaySpawnPos.z);
                        maskCheckRayDir = new Vector3(dir.x * index, dir.y - 5.01f, dir.z * index);
                        //Debug.DrawRay(maskCheckRaySpawnPos, maskCheckRayDir, Color.green, debugRayTime);
                        break;
                    case Direction.LEFT:
                        prefabSpawnPos = new Vector3(prefabSpawnPos.x, prefabSpawnPos.y, prefabSpawnPos.z - index + 1);
                        maskCheckRaySpawnPos = new Vector3(maskCheckRaySpawnPos.x + .5f, maskCheckRaySpawnPos.y + 5f
                            , maskCheckRaySpawnPos.z);
                        maskCheckRayDir = new Vector3(dir.x * index, dir.y - 5.01f, dir.z * -index);
                        //Debug.DrawRay(maskCheckRaySpawnPos, maskCheckRayDir, Color.green, debugRayTime);
                        break;
                }

                if (prefabSpawnPos.x < -5 ||
                    prefabSpawnPos.x > GetComponent<DungeonCreator>().dungeonHeight + 5 ||
                    prefabSpawnPos.z < -5 ||
                    prefabSpawnPos.z > GetComponent<DungeonCreator>().dungeonWidth + 5) {
                    break;
                }


                RaycastHit maskHit = new RaycastHit();
                Ray maskRay = new Ray(maskCheckRaySpawnPos, maskCheckRayDir);
                Physics.Raycast(maskRay, out maskHit);

                //if (maskHit.collider != null) {

                //    Debug.Log(maskHit.collider.name);
                //    Debug.DrawRay(maskCheckRaySpawnPos, maskCheckRayDir, Color.blue, debugRayTime);

                //}
                Instantiate(maskPrefab, prefabSpawnPos, Quaternion.Euler(0, 0, 0), maskParent.transform);
            }
        }


        Debug.Log(counter);
        //Stitch all masks together
        MeshStitcher stitcher = new MeshStitcher(maskParent);

        //Destroy all mask gameobjects
        while (GameObject.Find("Mask(Clone)") != null) {
            //IN BUILD SET TO DESTROY GAMEOBJECTS
            //GameObject.Find("Mask(Clone)").SetActive(false);
            DestroyImmediate(GameObject.Find("Mask(Clone)"));
        }


        GameObject newMeshObj = new GameObject("New Mask Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        newMeshObj.transform.position = Vector3.zero;
        newMeshObj.transform.parent = maskParent.transform;
        newMeshObj.transform.SetSiblingIndex(0);

        newMeshObj.GetComponent<MeshFilter>().sharedMesh = stitcher.NewMesh;
        newMeshObj.GetComponent<MeshRenderer>().material = maskMat;
    }

    public GameObject GetTopmostParent(GameObject child) {
        // Check if the object has a parent
        if (child.transform.parent == null) {
            return child;
        }

        Transform current = child.transform;
        while (current.parent != null) {
            current = current.parent;
        }

        return current.gameObject;
    }
}

public enum Direction {
    LEFT = 0, UP = 1, RIGHT = 2, DOWN = 3
}

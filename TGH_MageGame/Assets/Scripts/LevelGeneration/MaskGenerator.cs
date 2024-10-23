using System.Collections.Generic;
using UnityEngine;

public class MaskGenerator : MonoBehaviour {
    List<Transform> walls;
    [SerializeField] GameObject maskPrefab; //SII
    [SerializeField] GameObject maskParent; //SII
    [SerializeField] Material maskMat; //SII

    public void GenerateMaskMesh(List<Transform> wallsIn) {
        walls = wallsIn;
        for (int i = 0; i < walls.Count; i++) {
            Vector3 dir = walls[i].GetChild(0).forward.normalized;
            Vector3 rayStartPos = new Vector3(walls[i].position.x, 3f, walls[i].position.z - 0.1f);

            Direction faceDir = Direction.UP;
            switch (walls[i].GetChild(0).transform.rotation.eulerAngles.y) {
                case 0:
                    faceDir = Direction.LEFT;
                    break;
                case 90:
                    faceDir = Direction.UP;
                    break;
                case 180:
                    faceDir = Direction.RIGHT;
                    break;
                case 270:
                    faceDir = Direction.DOWN;
                    break;
            }


            Debug.DrawRay(rayStartPos + dir * 0.1f, dir * 4, Color.red, 6);

            Ray ray = new Ray(rayStartPos + dir * 0.1f, dir);
            RaycastHit hitInfo = new RaycastHit();
            Physics.Raycast(ray, out hitInfo);

            int dist = -1;
            if (hitInfo.collider != null) {
                dist = Mathf.RoundToInt(hitInfo.distance);
            }

            Vector3 maskSpawnPos = new Vector3(walls[i].position.x, 3f, walls[i].position.z);
            Debug.Log(faceDir);

            for (int j = 0; j < (dist < 0 ? 10 : (dist / 2) + 1); j++) {

                int index = j;
                if (faceDir == Direction.LEFT) {
                    index *= -1;
                }
                if (faceDir == Direction.UP) {
                    index *= -1;
                }

                if (faceDir == Direction.RIGHT || faceDir == Direction.LEFT) {
                    Instantiate(maskPrefab, new Vector3(maskSpawnPos.x, maskSpawnPos.y, maskSpawnPos.z - index), Quaternion.Euler(0, 0, 0), maskParent.transform);
                }
                else {
                    Instantiate(maskPrefab, new Vector3(maskSpawnPos.x - index, maskSpawnPos.y, maskSpawnPos.z + 1), Quaternion.Euler(0, 0, 0), maskParent.transform);
                }



            }
        }

        //Stitch all masks together
        MeshStitcher stitcher = new MeshStitcher(maskParent);

        GameObject newMeshObj = new GameObject("New Mask Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        newMeshObj.transform.position = Vector3.zero;
        newMeshObj.transform.parent = maskParent.transform;

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

using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

[RequireComponent(typeof(NavMeshSurface))]
public class DungeonCreator : MonoBehaviour
{
    private NavMeshSurface navMeshSurface;

    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    public float lootProb, shopProb;
    public Material material;
    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerMidifier;
    [Range(0, 2)]
    public int roomOffset;
    public GameObject wallPrefab, playerPrefab, chestPrefab, enemyPrefab, shopPrefab;
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;
    // Start is called before the first frame update
    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();

        CreateDungeon();

        navMeshSurface.BuildNavMesh();
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();
        DugeonGenerator generator = new DugeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = generator.CalculateDungeon(maxIterations,
            roomWidthMin,
            roomLengthMin,
            roomBottomCornerModifier,
            roomTopCornerMidifier,
            roomOffset,
            corridorWidth);
        GameObject wallParent = new GameObject("WallParent");
        wallParent.transform.parent = transform;
        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();
        CreatePlayer(listOfRooms);
        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
        }
        CreateWalls(wallParent);
        CreateLoot(listOfRooms);
        CreateEnemy(listOfRooms);
        CreateShop(listOfRooms);
    }

    private void CreatePlayer(List<Node> listOfRooms)
    {
        Node room = listOfRooms[UnityEngine.Random.Range(0, listOfRooms.Count)];
        /*
        float posX = (room.BottomRightAreaCorner.x- room.BottomLeftAreaCorner.x) / 2;
        float posY = (room.TopRightAreaCorner.y - room.BottomRightAreaCorner.y) / 2;
        playerPrefab.transform.SetPositionAndRotation(new Vector3(posX, 1, posY), Quaternion.identity);
        */

        int playerPosX = UnityEngine.Random.Range(room.BottomLeftAreaCorner.x + 2, room.BottomRightAreaCorner.x - 1);
        int playerPosY = UnityEngine.Random.Range(room.BottomLeftAreaCorner.y + 2, room.TopLeftAreaCorner.y - 1);
            Vector3 playerPos = new Vector3(
                playerPosX,
                2,
                playerPosY);
        playerPrefab.transform.SetPositionAndRotation(playerPos, Quaternion.identity);
    }

    private void CreateEnemy(List<Node> listOfRooms)
    {
        foreach( var room in listOfRooms)
        {
            if (room.Type == "room")
            {
                int enemyPosX = UnityEngine.Random.Range(room.BottomLeftAreaCorner.x + 2, room.BottomRightAreaCorner.x - 1);
                int enemyPosY = UnityEngine.Random.Range(room.BottomLeftAreaCorner.y + 2, room.TopLeftAreaCorner.y - 1);
                Vector3 enemyPos = new Vector3(
                    enemyPosX,
                    1,
                    enemyPosY);
                if(UnityEngine.Random.Range(0f,1f) > 0.3)
                {
                    Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
                }   
            }
        }
    }


    private void CreateShop(List<Node> listOfRooms)
    {
        foreach( var room in listOfRooms)
        {
            if (room.Type == "room")
            {
                int shopX = (room.BottomLeftAreaCorner.x + 1 + room.BottomRightAreaCorner.x) / 2;
                int shopY = (room.BottomLeftAreaCorner.y + 1 + room.TopLeftAreaCorner.y) / 2;
                Vector3 shopPos = new Vector3(
                    shopX,
                    0.35f,
                    shopY);
                if(UnityEngine.Random.Range(0f,1f) < shopProb)
                {
                    var shop = Instantiate(shopPrefab, shopPos, Quaternion.identity);

                    shop.GetComponent<ShopRenderer>().AddRandomItems();
                }   
            }
        }
    }

    private void CreateLoot(List<Node> listOfRooms)
    {
        foreach( var room in listOfRooms)
        {
            if (room.Type == "room")
            {
                int chestX = UnityEngine.Random.Range(room.BottomLeftAreaCorner.x + 2, room.BottomRightAreaCorner.x - 1);
                int chestY = UnityEngine.Random.Range(room.BottomLeftAreaCorner.y + 2, room.TopLeftAreaCorner.y - 1);
                Vector3 chestPos = new Vector3(
                    chestX,
                    0.35f,
                    chestY);
                if(UnityEngine.Random.Range(0f,1f) > lootProb)
                {
                    Instantiate(chestPrefab, chestPos, Quaternion.identity);
                }   
            }
        }
    }

    private void CreateWalls(GameObject wallParent)
    {
        int wallPosX = 0;
        int wallPosY = 3;
        int wallPosZ = 0;
        int startX = -1;
        int startZ = -1;
        int temp = 0;
        foreach (var wallPosition in possibleWallHorizontalPosition)
        {
            if (startX == -1)
            {
                startX = wallPosition.x;
                wallPosZ = wallPosition.z;
                temp = wallPosition.x;
            }
            else
            {
                if(wallPosition.x == temp+1)
                {
                    temp = wallPosition.x;
                }
                else
                {
                    int xScale = temp - startX;
                    wallPrefab.transform.localScale = new Vector3(xScale, 6, 1);
                    wallPosX = startX + xScale / 2;
                    Vector3Int wallPos = new Vector3Int(
                        wallPosX,
                        wallPosY,
                        wallPosZ
                    );
                    CreateWall(wallParent, wallPos, wallPrefab);
                    startX = wallPosition.x;
                    wallPosZ = wallPosition.z;
                    temp = wallPosition.x;
                }
            }
        }
        wallPrefab.transform.localScale = Vector3.one;
        foreach (var wallPosition in possibleWallVerticalPosition)
        {
            if (startZ == -1)
            {
                startZ = wallPosition.z;
                wallPosX = wallPosition.x;
                temp = wallPosition.z;
            }
            else
            {
                if(wallPosition.z == temp+1)
                {
                    temp = wallPosition.z;
                }
                else
                {
                    int zScale = temp - startZ;
                    wallPrefab.transform.localScale = new Vector3(1, 6, zScale);
                    wallPosZ = startZ + zScale / 2;
                    Vector3Int wallPos = new Vector3Int(
                        wallPosX,
                        wallPosY,
                        wallPosZ
                    );
                    CreateWall(wallParent, wallPos, wallPrefab);
                    startZ = wallPosition.z;
                    wallPosX = wallPosition.x;
                    temp = wallPosition.z;
                }
            }
        }
    }

    private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
    {
        Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
    }
    

    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }

        int[] triangles = new int[]
        {
            0,
            1,
            2,
            2,
            1,
            3
        };
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        GameObject dungeonFloor = new GameObject("Mesh" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));

        dungeonFloor.transform.position = Vector3.zero;
        dungeonFloor.transform.localScale = Vector3.one;
        dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
        dungeonFloor.GetComponent<MeshRenderer>().material = material;
        dungeonFloor.AddComponent<MeshCollider>();
        dungeonFloor.transform.parent = transform;

        for (int row = (int)Math.Ceiling(bottomLeftV.x); row < (int)Math.Ceiling(bottomRightV.x); row++)
        {
            var wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int row = (int)Math.Ceiling(topLeftV.x); row < (int)Math.Ceiling(topRightCorner.x); row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
        }
        for (int col = (int)Math.Ceiling(bottomLeftV.z); col < (int)Math.Ceiling(topLeftV.z); col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
        for (int col = (int)Math.Ceiling(bottomRightV.z); col < (int)Math.Ceiling(topRightV.z); col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
        }
    }

    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        if (wallList.Contains(point)){
            doorList.Add(point);
            wallList.Remove(point);
        }
        else
        {
            wallList.Add(point);
        }
    }

    private void DestroyAllChildren()
    {
        while(transform.childCount != 0)
        {
            foreach(Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}

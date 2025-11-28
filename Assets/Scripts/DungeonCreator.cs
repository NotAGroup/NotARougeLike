using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.Rendering;
using UnityEngine;
using Random = System.Random;

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
    List<Vector3Int> possibleVerticalDoorPosition;
    List<Vector3Int> possibleHorizontalDoorPosition;
    List<Vector3Int> possibleHorizontalWallPosition;
    List<Vector3Int> possibleVerticalWallPosition;

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
        possibleHorizontalDoorPosition = new List<Vector3Int>();
        possibleVerticalDoorPosition = new List<Vector3Int>();
        possibleHorizontalWallPosition = new List<Vector3Int>();
        possibleVerticalWallPosition = new List<Vector3Int>();
        CreatePlayer(listOfRooms);
        for (int i = 0; i < listOfRooms.Count; i++)
        {
            CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, listOfRooms[i].Type);
        }
        CreateWalls(wallParent);
        CreateLoot(listOfRooms);
        CreateEnemy(listOfRooms);
        CreateShop(listOfRooms);
    }

    private void CreatePlayer(List<Node> listOfRooms)
    {
        Node room = listOfRooms[UnityEngine.Random.Range(0, listOfRooms.Count)];
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
        // To Cap the possibleWalls Lists
        possibleHorizontalWallPosition.Add(possibleHorizontalWallPosition[0]);
        possibleVerticalWallPosition.Add(possibleVerticalWallPosition[0]);
        Random rng = new Random();
        int start = possibleHorizontalWallPosition[0].x;
        int wallLength;
        float wallDetail;
        float wallPosX = 0;
        int wallPosY = 0;
        float wallPosZ = 0;
        int startX = -1;
        int startZ = -1;
        int temp = 0;
        float wallScaler = 4f;
        foreach (var wallPosition in possibleHorizontalWallPosition)
        {
            // Define starting point
            if (startX == -1)
            {
                startX = wallPosition.x;
                wallPosZ = wallPosition.z;
                temp = wallPosition.x;
            }
            else
            {
                // check if coherent Wall, save end point as temp
                if(wallPosition.x == temp + 1)
                {
                    temp = wallPosition.x;
                }
                // (temp - start) = total length of wall
                else
                {
                    wallPosX = startX;
                    wallLength = temp - startX;
                    int segmentCount = (wallLength < 10) ? 2 : wallLength / 10 * 2 ;
                    int cutCount = segmentCount - 1;

                    List<float> cuts = new List<float>();
                    for (int c = 0; c < cutCount; c++)
                    {
                        cuts.Add((float)(rng.NextDouble() * wallLength));
                    }
                    cuts.Sort();

                    float[] segments = new float[segmentCount];

                    if (segmentCount > 0)
                        segments[0] = cuts[0];

                    for (int c = 1; c < cutCount; c++)
                        segments[c] = cuts[c] - cuts[c - 1];

                    segments[segmentCount - 1] = wallLength - cuts[cutCount - 1];

                    foreach (float xScale in segments)
                    {
                        wallPrefab.transform.localScale = new Vector3(xScale/wallScaler, 1, 1);
                        wallPosX += xScale / 2;
                        wallDetail = UnityEngine.Random.Range(-0.1f, 0.1f);
                        Vector3 wallPos  = new Vector3(
                            wallPosX,
                            wallPosY,
                            wallPosZ + wallDetail
                        );  
                        CreateWall(wallParent, wallPos, wallPrefab, Quaternion.identity);
                        wallPosX += xScale / 2;
                    }
                    startX = wallPosition.x;
                    wallPosZ = wallPosition.z;
                    temp = wallPosition.x;
                }
            }
        }
        foreach (var wallPosition in possibleVerticalWallPosition)
        {
            // Define starting point
            if (startZ == -1)
            {
                startZ = wallPosition.z;
                wallPosX = wallPosition.x;
                temp = wallPosition.z;
            }
            else
            {
                // check if coherent Wall, save end point as temp
                if(wallPosition.z == temp + 1)
                {
                    temp = wallPosition.z;
                }
                // (temp - start) = total length of wall
                else
                {
                    wallPosZ = startZ;
                    wallLength = temp - startZ;
                    int segmentCount = (wallLength < 10) ? 2 : wallLength / 10 * 2 ;
                    int cutCount = segmentCount - 1;

                    List<float> cuts = new List<float>();
                    for (int c = 0; c < cutCount; c++)
                    {
                        cuts.Add((float)(rng.NextDouble() * wallLength));
                    }
                    cuts.Sort();

                    float[] segments = new float[segmentCount];

                    if (segmentCount > 0)
                        segments[0] = cuts[0];

                    for (int c = 1; c < cutCount; c++)
                        segments[c] = cuts[c] - cuts[c - 1];

                    segments[segmentCount - 1] = wallLength - cuts[cutCount - 1];

                    foreach (float zScale in segments)
                    {
                        wallPrefab.transform.localScale = new Vector3(zScale/wallScaler, 1, 1);
                        wallPosZ += zScale / 2;
                        wallDetail = UnityEngine.Random.Range(-0.1f, 0.1f);
                        Vector3 wallPos  = new Vector3(
                            wallPosX + wallDetail,
                            wallPosY,
                            wallPosZ
                        );  
                        CreateWall(wallParent, wallPos, wallPrefab, Quaternion.Euler(0, 90, 0));
                        wallPosZ += zScale / 2;
                    }
                    startZ = wallPosition.z;
                    wallPosX = wallPosition.x;
                    temp = wallPosition.z;
                }
            }
        }
    }
   
    private void CreateWall(GameObject wallParent, Vector3 wallPosition, GameObject wallPrefab, Quaternion rotation)
    {
        Instantiate(wallPrefab, wallPosition, rotation, wallParent.transform);
    }
    

    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, String roomType)
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
            AddWallPositionToList(wallPosition, possibleHorizontalWallPosition, possibleHorizontalDoorPosition);                
            
        }
        for (int row = (int)Math.Ceiling(topLeftV.x); row < (int)Math.Ceiling(topRightCorner.x); row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(wallPosition, possibleHorizontalWallPosition, possibleHorizontalDoorPosition);                
            
        }
        for (int col = (int)Math.Ceiling(bottomLeftV.z); col < (int)Math.Ceiling(topLeftV.z); col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleVerticalWallPosition, possibleVerticalDoorPosition);                
        }
        for (int col = (int)Math.Ceiling(bottomRightV.z); col < (int)Math.Ceiling(topRightV.z); col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(wallPosition, possibleVerticalWallPosition, possibleVerticalDoorPosition);                
        }
    }

    private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        if (wallList.Contains(point)){
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

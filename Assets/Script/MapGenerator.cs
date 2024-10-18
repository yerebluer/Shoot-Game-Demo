using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navMeshFloor;
    public Transform mapFloor;
    public Transform navmeshMaskPrefab;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;
    public float tileSize;

    List<Coord> allTileCoords=new List<Coord>();
    Queue<Coord> shuffledTileCoords=new Queue<Coord>();
    Queue<Coord> shuffledOpenTileCoords = new Queue<Coord>();

    Transform[,] tileMap;

    Map currentMap;
    void Awake()
    {
        FindObjectOfType<Spwaner>().OnNewWave += OnNewWave;
    }
    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber;
        GenerateMap();
    }
    public void GenerateMap()
    {
        currentMap = maps[mapIndex];
        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];
        System.Random prng = new System.Random(currentMap.seed);
        
        //Coords of tiles
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArray<Coord>(allTileCoords.ToArray(), currentMap.seed));
        //mapHolder
        string holderName = "Generaterd Map";
        if (transform.Find(holderName))
        {
            DestroyImmediate(transform.Find(holderName).gameObject);
        }
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        Vector3 size= Vector3.one * (1 - outlinePercent) * tileSize;
        //Spawning Tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                //因为tilePostion是中心点位置,所以要多偏移.5f
                Vector3 tilePosition = Coord2Position(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right*90)) as Transform;
                newTile.localScale = size;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }
        //Spwaning Obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];
        int obstacleCount =(int)(currentMap.mapSize.x*currentMap.mapSize.y* currentMap.obstaclePercent);
        int currentObstacleCount =0;
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);
        for (int i = 0; i < obstacleCount; i++)
        {
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;
            if (randomCoord != currentMap.mapCentre && MapIsFulllyAcessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePostion = Coord2Position(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePostion + Vector3.up * obstacleHeight/2, Quaternion.identity) as Transform;
                newObstacle.parent = mapHolder;
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize,obstacleHeight,(1 - outlinePercent) * tileSize);
                Renderer obstacleRender = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRender.sharedMaterial);//利用引用创建一个新的材质
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRender.sharedMaterial = obstacleMaterial;//将障碍的材质引用替换为现有的新材质

                allOpenCoords.Remove(randomCoord);//这里是遍历删除
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray<Coord>(allOpenCoords.ToArray(), currentMap.seed));

        //地板大小
        navMeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
        mapFloor.localScale=new Vector3(currentMap.mapSize.x * tileSize, currentMap.mapSize.y * tileSize);
        //超出实际大小部分用遮罩
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left*(currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y)*tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;

        maskTop.parent = mapHolder;
        maskBottom.parent = mapHolder;
        maskRight.parent = mapHolder;
        maskLeft.parent = mapHolder;

        //
    }
    bool MapIsFulllyAcessible(bool [,] obstacleMap,int currentObstacleCount)
    {
        int m = obstacleMap.GetLength(0);
        int n = obstacleMap.GetLength(1);
        bool[,] mapFlags = new bool[m, n];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;
        int[] dir = { -1, 0, 1, 0, -1 };
        int cnt = 1;
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            for (int i = 0; i < 4; i++)
            {
                int x = tile.x + dir[i];
                int y = tile.y + dir[i + 1];
                if (x >= 0 && y >= 0 && x < m && y < n)
                {
                    if (!mapFlags[x, y] && !obstacleMap[x, y])
                    {
                        queue.Enqueue(new Coord(x, y));
                        cnt++;
                        mapFlags[x, y] = true;
                    }

                }
            }
        }
        //返回 中心点联通区域可达区块==总区块数-障碍区块数
        return cnt == (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
    }

    Vector3 Coord2Position(int x,int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y)*tileSize;
    }

    public Coord GetRandomCoord()
    {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }
    public Transform Position2Coord(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        //constrains
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0)-1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1)-1);
        return tileMap[x, y];
    }

    public Transform GetRandomOpenTile()
    {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coord
    {
        public int x;
        public int y;
        public Coord(int _x,int _y)
        {
            x = _x;
            y = _y;
        }
        public static bool operator ==(Coord a,Coord b)
        {
            return a.x==b.x&&a.y==b.y;
        }
        public static bool operator !=(Coord a, Coord b)
        {
            return a.x != b.x || a.y != b.y;
        }
    }
    
    [System.Serializable]
    public class Map
    {
        public Coord mapSize;
        [Range(0,1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;
        public Coord mapCentre
        {
            get
            {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
    
}

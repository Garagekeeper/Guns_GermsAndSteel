using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static Define;
using static RoomClass;
using static UnityEngine.RuleTile.TilingRuleOutput;
using Object = UnityEngine.Object;
using Transform = UnityEngine.Transform;
using static Utility;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class RoomClass
{
    public enum ERoomType
    {
        Start,
        Normal,
        Gold,
        Sacrifice,
        Curse,
        Shop,
        Boss,
        Angel,
        Devil,
        Secret,
        SSecret,
    }

    public ERoomType RoomType { get; set; }
    public int XPos { get; private set; }
    public int YPos { get; private set; }

    public Transform Transform { get; set; }
    public Vector2 WorldCenterPos { get; set; }

    public GameObject RoomObject { get; set; }
    public GameObject ItemHolder { get; set; }

    private bool _isClear = false;
    public bool IsClear
    {
        get { return _isClear; }
        set
        {
            if (_isClear != value)
            {
                _isClear = value;
            }
        }
    }

    private Transform _transform;
    private GameObject _tilemapPrefab;
    private GameObject _tilemap_CollisionPrefab;
    private GameObject _ColliderPrefab;
    private GameObject _obstacle;
    private GameObject _doors;
    private Tilemap _tilemap;
    private long _awardSeed;
    private long _objectSeed;


    public GameObject TilemapPrefab { get { return _tilemapPrefab; } set { _tilemapPrefab = value; } }
    public GameObject TilemapCollisionPrefab { get { return _tilemap_CollisionPrefab; } set { _tilemap_CollisionPrefab = value; } }
    public GameObject CollidePrefab { get { return _ColliderPrefab; } set { _ColliderPrefab = value; } }
    public GameObject Obstacle { get { return _obstacle; } set { _obstacle = value; } }

    public GameObject Doors { get { return _doors; } set { _doors = value; } }
    public Tilemap Tilemap { get { return _tilemap; } set { _tilemap = value; } }
    
    public long AwardSeed { get { return _awardSeed; } set { _awardSeed = value; } }
    public long ObjectSeed { get { return _objectSeed; } set { _objectSeed = value; } }

    public int DiffiCulty { get; set; }

    //R D L U 순서
    public RoomClass[] _adjacencentRooms { get; set; }

    public RoomClass(int xPos, int yPos)
    {
        RoomType = ERoomType.Normal;
        XPos = xPos;
        YPos = yPos;
        _adjacencentRooms = new RoomClass[4];
        AwardSeed = Managers.Game.RNG.Sn;
        ObjectSeed = Managers.Game.RNG.Sn;
    }
}
public class MapManager
{
    public enum ECellType
    {
        UnVisited,
        currentRoom,
        Visited,
    }

    string[,] doorFrame =
{
    { "door_01_normaldoor_0" , "door_01_normaldoor_4"},
    { "door_01_normaldoor_0" , "door_01_normaldoor_4"},
    { "door_02_treasureroomdoor_0" , "door_01_normaldoor_4"},
    { "door_03_sacrificeroomdoor_0" , "door_03_sacrificeroomdoor_4"},
    { "door_04_curseroomdoor_0" , "door_04_curseroomdoor_4"},
    { "door_05_shopdoor_0" , "door_05_shopdoor_4"},
    { "door_06_bossroomdoor_0" , "door_06_bossroomdoor_4"},
    { "door_07_devilroomdoor_0" , "door_07_devilroomdoor_4"},
    { "door_07_holyroomdoor_0" , "door_07_holyroomdoor_4"},
};

    string[] doorBackGround =
    {
    "door_01_normaldoor_1",
    "door_01_normaldoor_1",
    "door_01_normaldoor_1",
    "door_03_sacrificeroomdoor_1",
    "door_01_normaldoor_1",
    "door_05_shopdoor_1",
    "door_06_bossroomdoor_1",
    "door_07_devilroomdoor_1",
    "door_07_holyroomdoor_1",
};

    string[,] doorSide =
    {
    { "door_01_normaldoor_2", "door_01_normaldoor_3","door_01_normaldoor_5", "door_01_normaldoor_6"},
    { "door_01_normaldoor_2", "door_01_normaldoor_3","door_01_normaldoor_5", "door_01_normaldoor_6"},
    { "door_01_normaldoor_2", "door_01_normaldoor_3", "door_02_treasureroomdoor_5", "door_02_treasureroomdoor_6"},
    { "door_03_sacrificeroomdoor_2","door_03_sacrificeroomdoor_3","door_03_sacrificeroomdoor_5","door_03_sacrificeroomdoor_6"},
    { "door_04_curseroomdoor_2","door_04_curseroomdoor_3","door_04_curseroomdoor_5","door_04_curseroomdoor_6"},
    { "door_05_shopdoor_2","door_05_shopdoor_3","door_05_shopdoor_5","door_05_shopdoor_6"},
    { "door_06_bossroomdoor_2","door_06_bossroomdoor_3","door_06_bossroomdoor_5","door_06_bossroomdoor_6"},
    { "door_07_devilroomdoor_2","door_07_devilroomdoor_3","door_07_devilroomdoor_5","door_07_devilroomdoor_6"},
    { "door_07_holyroomdoor_2","door_07_holyroomdoor_3","door_07_holyroomdoor_5","door_07_holyroomdoor_6"},
};

    #region A* pathFinding
    public struct PQNode : IComparable<PQNode>
    {
        public int Huristic;
        public Vector3Int Pos;
        public int Depth;

        public int CompareTo(PQNode other)
        {
            if (Huristic == other.Huristic)
                return 0;
            return Huristic < other.Huristic ? 1 : -1;
        }
    }


    List<Vector3Int> _possibleVector = new()
    {
        new Vector3Int(0, 1, 0), //Top
        new Vector3Int(1, 0, 0), //Right
        new Vector3Int(0, -1, 0), //Bottom
        new Vector3Int(-1, 0, 0), //Left
        //new Vector3Int(-1, -1, 0), //Bottom Left
        //new Vector3Int(1, -1, 0), //Bottom Right
        //new Vector3Int(1, 1, 0), //Top Right
        //new Vector3Int(-1, 1, 0), //Top Left
    };

    public bool CanGo(Vector3Int next)
    {
        int x = next.x;
        int y = next.y;

        if (x < StageColXMin || x > StageColXMax) return false;
        if (y < StageColYMin || y > StageColYMax) return false;
        //Debug.Log("x: " + (x + XMax));
        //Debug.Log("y: " + (y + YMax));
        if (collisionData[y + math.abs(StageColYMin), x + math.abs(StageColXMin)]
            == (int)ECellCollisionType.Wall) return false;
        return true;
    }

    /*
     *  -3 -2 -1 0 1 2 3
     *   0  1  2 3 4 5 6
     */

    public List<Vector3Int> FindPath(Creature crreature, Vector3Int startPos, Vector3Int destPos, int maxDepth = 10)
    {
        //Save Best Candidate
        Dictionary<Vector3Int, int> best = new();

        //Path tracking
        Dictionary<Vector3Int, Vector3Int> parent = new();

        PriorityQueue<PQNode> pq = new();


        Vector3Int pos = startPos;
        Vector3Int dest = destPos;

        Vector3Int closestCellPos = startPos;
        int closestHuristic = (dest - pos).sqrMagnitude;

        //
        {
            int huristic = (dest - pos).sqrMagnitude;
            pq.Push(new PQNode() { Huristic = huristic, Pos = pos, Depth = 1 });
            parent[pos] = pos;
            best[pos] = huristic;
        }

        while (pq.Count > 0)
        {
            PQNode node = pq.Pop();
            pos = node.Pos;

            if (pos == dest)
                break;

            if (node.Depth >= maxDepth)
                break;

            foreach (Vector3Int dirVec in _possibleVector)
            {
                Vector3Int next = pos + dirVec;

                if (!CanGo(next)) break;

                int huristic = (dest - next).sqrMagnitude;

                if (best.ContainsKey(next) == false)
                    best[next] = int.MaxValue;

                if (best[next] <= huristic)
                    continue;

                best[next] = huristic;

                pq.Push(new PQNode() { Huristic = huristic, Pos = next, Depth = node.Depth + 1 });
                parent[next] = pos;

                if (closestHuristic > huristic)
                {
                    closestHuristic = huristic;
                    closestCellPos = next;
                }
            }
        }
        if (parent.ContainsKey(dest) == false)
            return CalcCellPathFromParent(parent, closestCellPos);

        return CalcCellPathFromParent(parent, dest);
    }

    List<Vector3Int> CalcCellPathFromParent(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int dest)
    {
        List<Vector3Int> cells = new();

        if (parent.ContainsKey(dest) == false)
            return cells;

        Vector3Int now = dest;

        int cnt = 0;
        while (parent[now] != now)
        {
            if (cnt++ > 500) break;
            cells.Add(now);
            now = parent[now];
        }

        cells.Add(now);
        cells.Reverse();

        return cells;
    }

    #endregion

    #region MAP_GENERATING
    public struct PQRNode : IComparable<PQRNode>
    {
        public RoomClass RoomClass;
        public int ManhattanDistance;

        public int CompareTo(PQRNode other)
        {
            if (ManhattanDistance == other.ManhattanDistance)
                return 0;
            return ManhattanDistance > other.ManhattanDistance ? 1 : -1;
        }
    }

    //방을 만들때 사용하는 배열의 최대X
    public static int s_mapMaxXofRoomArray = 9;
    //방을 만들때 사용하는 배열의 최대Y
    public static int s_mapMaxYofRoomArray = 9;
    public static int[,] s_roomGraph;
    public int[,] collisionData;

    public List<RoomClass> Rooms { get; private set; }


    public List<int> _difficulty = new List<int>
            {
                0, 0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0,
                1, 1, 1, 1 ,1 ,1 ,1, 1 ,1,
                2, 2, 2, 2 ,2 ,2, 2 ,2,
                3, 3, 3, 3 ,3 ,3 ,3,
                4, 4, 4, 4, 4, 4,
                5, 5, 5, 5, 5,
                6, 6, 6, 6,
                7, 7, 7,
                8, 8,
                9,
            };

    //만들어진 방의 최대/최소 XY
    //스테이지 관점에서 방의 배열 (크기가 작다, 충돌맵에 사용하는 그 크기가 아니다)
    public int XMin { get; set; } = 100;
    public int YMin { get; set; } = 100;
    public int XMax { get; set; } = 0;
    public int YMax { get; set; } = 0;


    public int StageColXMin { get; set; } = 1000;
    public int StageColYMin { get; set; } = 1000;
    public int StageColXMax { get; set; } = -1000;
    public int StageColYMax { get; set; } = -1000;
    public Vector2Int StartingPos { get; set; } = new Vector2Int(4, 4);

    //int _baseRoomCountMax = 20;
    //int _baseRoomCountMin = 10;

    public RoomClass StartingRoom { get; set; }
    public RoomClass BossRoom { get; set; }
    private RoomClass _currentRoom { get; set; }

    public RoomClass CurrentRoom
    {
        get { return _currentRoom; }
        set
        {
            if (_currentRoom != value)
            {
                ChangeMinimapadjacencentCellSprite(value, _currentRoom);
                ChangeRoomActive(value, _currentRoom);
                _currentRoom = value;
            }
        }
    }

    public bool CanCreateRoom(int x, int y)
    {
        if (x < 0 || x >= s_mapMaxXofRoomArray) return false;
        if (y < 0 || y >= s_mapMaxYofRoomArray) return false;
        // 이미 방이 있다면 넘어간다.
        if (s_roomGraph[x, y] == 1) return false;
        // 인접한 방이 2개 이상이면 방을 생성하지 않는다.
        if (CheckAdjacencyRoomCnt(x, y) >= 2) return false;
        // 위조건을 만족해도 50보다 작아야 방을 생성
        if (!Managers.Game.RNG.Chance(50)) return false;
        return true;
    }

    public RoomClass TryPlacingSecretRoom()
    {
        Dictionary<Vector3, int> values = new();

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (s_roomGraph[i, j] == 1) break;
                int adjacent = 0;
                int weighjt = 14;
                int[] dx = { 0, 0, 1, -1 };
                int[] dy = { 1, -1, 0, 0, };

                int itr = 0;
                for (int k = 0; k < 4; k++)
                {
                    int nx = i + dx[k];
                    int ny = j + dy[k];
                    if (nx < 0 || nx >= s_mapMaxXofRoomArray) break;
                    if (ny < 0 || ny >= s_mapMaxYofRoomArray) break;
                    if (nx == BossRoom.XPos && ny == BossRoom.YPos) break;
                    if (s_roomGraph[nx, ny] == 1)
                    {
                        adjacent++;
                        itr++;
                    }
                }
                if (adjacent >= 3)
                {
                    values.Add(new Vector3(i, j), weighjt - 3);
                }
                else if (adjacent == 2)
                {
                    values.Add(new Vector3(i, j), weighjt - 6);
                }
                else if (adjacent == 1)
                {
                    values.Add(new Vector3(i, j), weighjt - 9);
                }
                else
                {
                    values.Add(new Vector3(i, j), weighjt - 12);
                }

            }
        }

        Vector3 pos = new(0, 0);
        int maxWeight = 0;
        foreach (var pl in values)
        {
            if (pl.Value > maxWeight)
            {
                pos = pl.Key; maxWeight = pl.Value;
            }
        }

        s_roomGraph[(int)pos.x, (int)pos.y] = 1;
        RoomClass SecretRoom = new((int)pos.x, (int)pos.y);
        return (SecretRoom);
    }

    public int CheckAdjacencyRoomCnt(int x, int y)
    {
        int cnt = 0;
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];
            if (nx < 0 || nx >= s_mapMaxXofRoomArray) continue;
            if (ny < 0 || ny >= s_mapMaxYofRoomArray) continue;
            if (s_roomGraph[nx, ny] == 1) cnt++;
        }

        return cnt;
    }

    static RoomClass GetRoomClassByPos(List<RoomClass> rooms, int x, int y)
    {
        foreach (var room in rooms)
        {
            if (room.XPos == x && room.YPos == y) return room;
        }

        return null;
    }


    public int GenerateStage()
    {
        Queue<RoomClass> roomClassQueue = new();
        RoomClass rc;
        Rooms = new List<RoomClass>();
        int roomCnt = 0;

        s_roomGraph = new int[9, 9];
        Rooms.Clear();

        //시작 위치
        s_roomGraph[4, 4] = 1;
        StartingRoom = new RoomClass(4, 4)
        {
            RoomType = RoomClass.ERoomType.Start
        };
        Rooms.Add(StartingRoom);

        //1.BFS를 통한 방 생성
        while (roomCnt < Managers.Game.N)
        {
            roomClassQueue.Enqueue(Managers.Game.RNG.Choice(Rooms));

            while (roomClassQueue.Count != 0)
            {
                RoomClass front = roomClassQueue.Dequeue();
                //Right, Down, Left, Up
                int[] dx = { 0, 1, 0, -1 };
                int[] dy = { 1, 0, -1, 0 };

                for (int i = 0; i < 4; i++)
                {
                    if (roomCnt == Managers.Game.N) break;
                    int nx = front.XPos + dx[i];
                    int ny = front.YPos + dy[i];

                    if (CanCreateRoom(nx, ny))
                    {
                        rc = new RoomClass(nx, ny);
                        front._adjacencentRooms[i] = rc;
                        rc._adjacencentRooms[(i + 2) % 4] = front;
                        s_roomGraph[nx, ny] = 1;
                        Rooms.Add(rc);
                        roomCnt++;
                        roomClassQueue.Enqueue(rc);

                        XMin = Math.Min(XMin, nx);
                        YMin = Math.Min(YMin, ny);
                        XMax = Math.Max(XMax, nx);
                        YMax = Math.Max(YMax, ny);
                    }
                }
            }
        }

        Rooms.Remove(StartingRoom);



        #region OLD_VER
        ////3. special (인접한 방이 1개인 방들을 나열) 생성
        ////dead ends
        //List<RoomClass> special = new List<RoomClass>();
        //foreach (var room in Rooms)
        //{
        //    //인접한 방이 1개이면 special추가
        //    if (room._adjacencentRooms.Count(s => s != null) == 1) special.Add(room);
        //    //아닌경우 방의 난이도 설정
        //    else room.DiffiCulty = Managers.Game.Choice(_difficulty);
        //}

        ////4. 보스방 설정
        ////1) spcial에서 시작방과 인접하지 않은 방들을 넣은것
        //List<RoomClass> boss = new List<RoomClass>();
        //foreach (RoomClass spc in special)
        //{
        //    bool startAdjacnecy = false;
        //    for (int i = 0; i < 4; i++)
        //    {
        //        RoomClass temp = spc._adjacencentRooms[i];
        //        if (temp == null) continue;
        //        if (temp == StartingRoom) startAdjacnecy = true;
        //        if (!startAdjacnecy)
        //        {
        //            boss.Add(spc);
        //            break;
        //        }
        //    }
        //}

        ////5. 황금방 설정
        //if (special.Count > 0)
        //{
        //    rc = Managers.Game.Choice(special);
        //    rc.RoomType = RoomClass.ERoomType.Gold;
        //    special.Remove(rc);

        //    //방의 개수가 15개가 넘는경우 2개 배정을 시도한다.
        //    if (Managers.Game.N >= 15 && special.Count > 0)
        //    {
        //        if (Managers.Game.Chance(25))
        //        {
        //            rc = Managers.Game.Choice(special);
        //            rc.RoomType = RoomClass.ERoomType.Gold;
        //            special.Remove(rc);
        //        }
        //    }
        //}

        ////6.비밀방 설정
        //if (special.Count > 0)
        //{
        //    rc = Managers.Game.Choice(special);
        //    rc.RoomType = RoomClass.ERoomType.Secret;
        //    special.Remove(rc);

        //    //TODO
        //    //비밀방은 겉으로 표시되는게 아니라
        //    //연결되지 않은 취급하자
        //    //R D L U
        //    //0 1 2 3
        //    for (int i = 0; i < 4; i++)
        //    {
        //        if (rc._adjacencentRooms[i] != null)
        //        {
        //            rc._adjacencentRooms[i]._adjacencentRooms[(i + 2) % 4] = null;
        //            rc._adjacencentRooms[i] = null;
        //        }
        //    }
        //}



        ////7. 상점 설정
        //if (special.Count > 0)
        //{
        //    if (Managers.Game.N <= 15)
        //    {
        //        rc = Managers.Game.Choice(special);
        //        rc.RoomType = RoomClass.ERoomType.Shop;
        //        special.Remove(rc);
        //    }
        //    else if (Managers.Game.Chance(66))
        //    {
        //        rc = Managers.Game.Choice(special);
        //        rc.RoomType = RoomClass.ERoomType.Shop;
        //        special.Remove(rc);
        //    }
        //}

        ////8.천사방, 악마방 설정
        //if (Managers.Game.Chance(20))
        //{
        //    int[] dx = { 0, 0, 1, -1 };
        //    int[] dy = { 1, -1, 0, 0, };
        //    List<RoomClass> reward = new List<RoomClass>();
        //    for (int i = 0; i < 4; i++)
        //    {
        //        int nx = BossRoom.XPos + dx[i];
        //        int ny = BossRoom.YPos + dy[i];

        //        if (nx < 0 || nx >= s_mapMaxXofRoomArray) continue;
        //        if (ny < 0 || ny >= s_mapMaxYofRoomArray) continue;
        //        if (s_roomGraph[nx, ny] == 1) continue;
        //        if (nx > XMax || nx < XMin) continue;
        //        if (ny > YMax || ny < YMin) continue;
        //        RoomClass temp = new RoomClass(nx, ny);

        //        reward.Add(temp);
        //    }

        //    rc = Managers.Game.Choice(reward);
        //    Rooms.Add(rc);
        //    s_roomGraph[rc.XPos, rc.YPos] = 1;
        //    rc.RoomType = Managers.Game.Chance(50) ? RoomClass.ERoomType.Devil : RoomClass.ERoomType.Angel;
        //}

        ////9. 희생방 설정
        //if (special.Count > 0)
        //{
        //    bool isAngel = false;
        //    foreach (var temp in Rooms)
        //    {
        //        if (temp.RoomType == RoomClass.ERoomType.Angel) isAngel = true;
        //    }

        //    if (isAngel || Managers.Game.Chance(14))
        //    {
        //        rc = Managers.Game.Choice(special);
        //        rc.RoomType = RoomClass.ERoomType.Sacrifice;
        //        special.Remove(rc);
        //    }
        //}

        ////10. 저주방 설정
        //if (special.Count > 0)
        //{
        //    bool isDevil = false;
        //    foreach (var temp in Rooms)
        //    {
        //        if (temp.RoomType == RoomClass.ERoomType.Devil) isDevil = true;
        //    }

        //    if (isDevil && Managers.Game.Chance(20))
        //    {
        //        rc = Managers.Game.Choice(special);
        //        rc.RoomType = ERoomType.Curse;
        //        special.Remove(rc);
        //    }
        //}

        ////11. special 배열이 빌 때까지 일반 방 설정
        //if (special.Count > 0)
        //{
        //    foreach (var temp in special)
        //    {
        //        temp.DiffiCulty = Managers.Game.Choice(_difficulty);
        //    }
        //}

        #endregion

        //3. special (인접한 방이 1개인 방들을 나열) 생성
        //dead ends
        PriorityQueue<PQRNode> dead_ends = new();
        foreach (var room in Rooms)
        {
            //인접한 방이 1개이면 special추가
            if (room._adjacencentRooms.Count(s => s != null) == 1) dead_ends.Push(new PQRNode() { RoomClass = room, ManhattanDistance = Math.Abs(4 - room.XPos) + (Math.Abs(4 - room.YPos)) });
            //아닌경우 방의 난이도 설정
            else room.DiffiCulty = Managers.Game.RNG.Choice(_difficulty);
        }

        //4. 보스방 설정
        //가장 거리가 먼 것을 먼저 뽑기
        BossRoom = dead_ends.Pop().RoomClass;
        for (int i = 0; i < 4; i++)
        {
            RoomClass temp = BossRoom._adjacencentRooms[i];
            if (temp == null) continue;
            if (temp == StartingRoom) return 0;
        }

        BossRoom.RoomType = ERoomType.Boss;

        //5. 상점 설정
        if (dead_ends.Count > 0)
        {
            if (Managers.Game.StageNumber < 5)
            {
                dead_ends.Pop().RoomClass.RoomType = ERoomType.Shop;
            }
            else if (Managers.Game.RNG.Chance(66))
            {
                dead_ends.Pop().RoomClass.RoomType = ERoomType.Shop;
            }
        }
        else
        {
            return 0;
        }


        //6. 황금방 설정
        if (dead_ends.Count > 0)
        {
            if (dead_ends.Count == 0) return 0;
            if (Managers.Game.StageNumber < 5)
                dead_ends.Pop().RoomClass.RoomType = ERoomType.Gold;

            //TODO XL
        }
        else
        {
            return 0;
        }

        //7.희생방
        if (dead_ends.Count > 0)
        {
            if (Managers.Game.RNG.Chance(14))
            {
                dead_ends.Pop().RoomClass.RoomType = ERoomType.Curse;
            }
        }

        //8. 저주방
        if (dead_ends.Count > 0)
        {
            int chance = 50; //TODO Devil 방문여부에 따른 +
            if (Managers.Game.RNG.Chance(chance))
                dead_ends.Pop().RoomClass.RoomType = ERoomType.Curse;
        }

        //9. 비밀방
        RoomClass secret;
        (secret) = TryPlacingSecretRoom();

        if (secret == null) return 0;
        else
        {
            secret.RoomType = ERoomType.Secret;
            Rooms.Add(secret);
        }

        //8.천사방, 악마방 설정
        if (Managers.Game.RNG.Chance(20))
        {
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0, };
            List<RoomClass> reward = new();
            for (int i = 0; i < 4; i++)
            {
                int nx = BossRoom.XPos + dx[i];
                int ny = BossRoom.YPos + dy[i];

                if (nx < 0 || nx >= s_mapMaxXofRoomArray) continue;
                if (ny < 0 || ny >= s_mapMaxYofRoomArray) continue;
                if (s_roomGraph[nx, ny] == 1) continue;
                if (nx > XMax || nx < XMin) continue;
                if (ny > YMax || ny < YMin) continue;
                RoomClass temp = new(nx, ny);

                reward.Add(temp);
            }

            if (reward.Count > 0)
            {
                rc = Managers.Game.RNG.Choice(reward);
                Rooms.Add(rc);
                s_roomGraph[rc.XPos, rc.YPos] = 1;
                rc.RoomType = Managers.Game.RNG.Chance(50) ? RoomClass.ERoomType.Devil : RoomClass.ERoomType.Angel;
            }
        }

        //11. special 배열이 빌 때까지 일반 방 설정
        while (dead_ends.Count > 0)
        {
            var temp = dead_ends.Pop().RoomClass.DiffiCulty = Managers.Game.RNG.Choice(_difficulty);
        }


        Rooms.Add(StartingRoom);

        //temp
        using (var parser = File.CreateText($"Assets/@Resources/Data/MapData/Stage.txt"))
        {
            for (int i = 0; i < s_mapMaxXofRoomArray; i++)
            {
                for (int j = 0; j < s_mapMaxYofRoomArray; j++)
                {
                    if (s_roomGraph[i, j] == 1)
                    {
                        parser.Write((GetRoomClassByPos(Rooms, i, j).RoomType.ToString()+"("+i+","+j+")").PadLeft(10));
                    }
                    else
                    {
                        parser.Write("".PadLeft(10));
                    }
                }
                parser.WriteLine();
            }
        }

        return 1;
    }
    #endregion


    private int _minimumX;
    private int _minimumY;
    private int _maximumX;
    private int _maximumY;


    public List<int> RoomCollisionCnt { get; } = new List<int>();

    public void Init(Action callback)
    {
        RoomCollisionCnt.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(ERoomType)).Length; i++)
            RoomCollisionCnt.Add(0);

        if (SceneManager.GetActiveScene().name == "DevScene")
        {
            Map = GameObject.Find("Stage");
            CellGrid = Map.GetComponent<Grid>();
            XMax = 11;
            XMin = -11;
            YMax = 6;
            YMin = -6;
            Rooms = new List<RoomClass>();
            RoomClass devRoom = new RoomClass(0, 0);
            devRoom.RoomObject = FindChildByName(Map.transform, "Room_Normal_1").gameObject;
            devRoom.Transform = devRoom.RoomObject.transform;
            devRoom.RoomObject = devRoom.RoomObject;
            devRoom.TilemapPrefab = devRoom.RoomObject.transform.GetChild(0).gameObject;
            devRoom.Obstacle = devRoom.RoomObject.transform.GetChild(1).gameObject;
            devRoom.CollidePrefab = devRoom.RoomObject.transform.GetChild(2).gameObject;
            devRoom.Doors = devRoom.RoomObject.transform.GetChild(4).gameObject;
            //devRoom.TilemapCollisionPrefab = FindChildByName(devRoom.Transform, "Tilemap").gameObject;
            devRoom.Tilemap = devRoom.TilemapPrefab.GetComponent<Tilemap>();

            Tilemap tmp = devRoom.Tilemap;
            Rooms.Add(devRoom);

            //ParseRoomCollisionData();
            return;
        }

        Managers.Resource.LoadAllAsync<Object>("InGame", (key, count, totalCount) =>
        {
            foreach (string name in Enum.GetNames(typeof(ERoomType)))
            {
                if (key.Contains(name) && key.Contains("Tile_Map"))
                {
                    RoomCollisionCnt[(int)Enum.Parse(typeof(ERoomType), name)]++;
                }
            }

            //Debug.Log($"{key} {count}/{totalCount}");

            if (count == totalCount)
            {
                while (true)
                {
                    if (GenerateStage() == 1)
                        break;
                }
                LoadMap();
                callback?.Invoke();
            }
        });
    }

    public GameObject Map { get; private set; }
    public Grid CellGrid { get; private set; }

    public Vector3Int WorldToCell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 CellToWorld(Vector3Int cellPos) { return CellGrid.CellToWorld(cellPos); }


    ECellCollisionType[,] _cellCollisionType;

    //TODO 추후 인자 변경

    public void LoadMap()
    {
        CurrentRoom = null;
        Map = Managers.Resource.Instantiate("Stage");
        CellGrid = Map.GetComponent<Grid>();
        int index = 0;
        foreach (RoomClass r in Rooms)
        {
            GenerateRoom(r, index++);
        }
        GenerateMinimap();
        CurrentRoom = StartingRoom;


        //parse map collision data
        ParseRoomCollisionData();
    }

    //Door Tile을 생성하거나, 변경하는 함수
    //TODO 여러 종류의 타일
    //여러 타일 바꾸도록
    public void GenerateDoor(RoomClass room)
    {
        for (int i = 0; i < 4; i++)
        {
            if (room._adjacencentRooms[i] != null)
            {
                int temp;
                if ((int)room.RoomType < (int)room._adjacencentRooms[i].RoomType)
                    temp = (int)room._adjacencentRooms[i].RoomType;
                else
                    temp = (int)room.RoomType;

                GameObject door = room.Doors.transform.GetChild(i).gameObject;
                door.SetActive(true);
                //bg
                door.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(doorBackGround[temp]);
                //left
                door.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(doorSide[temp, 0]);
                //right
                door.transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(doorSide[temp, 1]);
                //frame
                door.transform.GetChild(3).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(doorFrame[temp, 0]);
            }
        }

        if (room.RoomType == ERoomType.Boss)
        {
            if (Managers.Game.StageNumber == 8)
                GenerateClearBox(room);
            else
                GenerateTrapDoor(room);
        }
    }

    public void GenerateMinimap()
    {
        foreach (RoomClass room in Rooms)
        {
            //unvisited, currenet room, visited
            string[] cellSprite = { "minimap1_4", "minimap1_3", "minimap1_2", };

            //Instantiate prefab
            GameObject cell = Managers.Resource.Instantiate("Minimap_Cell");
            cell.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(cellSprite[(int)ECellType.UnVisited]);
            cell.name = room.RoomObject.name;

            GameObject pannel = Managers.UI.PlayingUI.GetMinimapPannel();
            cell.transform.SetParent(pannel.transform);

            //set pos
            cell.transform.localPosition = new Vector3(room.YPos - 4, 4 - room.XPos) * 30;

            if (room.RoomType == ERoomType.Start) cell.SetActive(true);
            else cell.SetActive(false);
        }

    }

    public void GenerateRoom(RoomClass r, int index)
    {
        //1.room 생성
        //배열과 좌표의 차이 때문에 신경써줘야함
        //배열은 0,0이 왼쪽 위
        float xDiif = r.YPos - StartingPos.y;
        float yDiif = StartingPos.x - r.XPos;
        Vector2 posDiff = new Vector2(1 * xDiif, 1 * yDiif) * new Vector2(21, 13);
        r.WorldCenterPos = posDiff;
        //Debug.Log(r.RoomType + " diff: (" + posDiff.x + "|" + posDiff.y + ") ");

        //room prfab 생성
        string roomPrefabname = "Room_";
        if (r.RoomType == ERoomType.Start && Managers.Game.StageNumber == 1) roomPrefabname += r.RoomType.ToString();
        else if (r.RoomType == ERoomType.Gold || r.RoomType == ERoomType.Boss || r.RoomType == ERoomType.Normal || r.RoomType == ERoomType.Start) roomPrefabname += "Normal_" + Managers.Game.StageNumber.ToString();
        else roomPrefabname += r.RoomType.ToString();
        GameObject room = Managers.Resource.Instantiate(roomPrefabname);

        // Select Random Map Collision
        //Debug.Log(r.RoomType.ToString());
        string roomName;

        roomName = "Tile_Map_Collision_" + r.RoomType.ToString() + "_" + Managers.Game.RNG.RandInt(0, RoomCollisionCnt[(int)r.RoomType] - 1);
        GameObject roomTileMap = Managers.Resource.Instantiate(roomName);
        roomTileMap.transform.SetParent(room.transform);

        //room 위치, 이름 조정
        room.transform.Translate(posDiff);
        room.name = Managers.Game.Seed + " " + Managers.Game.StageNumber + r.RoomType.ToString() + (r.RoomType == ERoomType.Normal ? index : "");
        room.transform.parent = Map.transform;

        //클래스 멤버 변수 초기화
        r.RoomObject = room;
        r.Transform = room.transform;
        r.TilemapPrefab = FindChildByName(r.Transform, "Tilemap").gameObject;
        r.Obstacle = FindChildByName(r.Transform, "Obstacle").gameObject;
        r.CollidePrefab = FindChildByName(r.Transform, "Collider").gameObject;
        r.Doors = FindChildByName(r.Transform, "Doors").gameObject;
        r.TilemapCollisionPrefab = roomTileMap;

        r.Tilemap = r.TilemapPrefab.GetComponent<Tilemap>();


        SetObstacle(r);
        GenerateDoor(r);
        r.TilemapCollisionPrefab.SetActive(false);

        if (r.RoomType == ERoomType.Start)
        {
            r.RoomObject.SetActive(true);
        }
        else
        {
            r.RoomObject.SetActive(false);
        }

    }

    public void GenerateTrapDoor(RoomClass room, Vector3 doorPos)
    {
        GameObject go = GenerateTrapDoor(room);
        go.transform.transform.position += doorPos;
    }

    public GameObject GenerateTrapDoor(RoomClass room)
    {
        GameObject go = Managers.Resource.Instantiate("TrapDoor", room.Doors.transform);
        go.transform.position = room.Doors.transform.position + new Vector3(-0.5f, -0.5f);
        go.SetActive(false);
        return go;
    }

    public GameObject GenerateClearBox(RoomClass room)
    {
        GameObject go = Managers.Resource.Instantiate("ClearBox", room.Doors.transform);
        go.transform.position = room.Doors.transform.position + new Vector3(-0.5f, -0.5f);
        go.SetActive(false);
        return go;
    }

    public void ChangeDoorSprite(RoomClass room)
    {
        GameObject doorParent = room.Doors;

        for (int i = 0; i < 4; i++)
        {
            GameObject door = doorParent.transform.GetChild(i).gameObject;
            if (door.activeSelf)
            {
                door.transform.GetChild(1).gameObject.SetActive(false);
                door.transform.GetChild(2).gameObject.SetActive(false);
            }
        }

        doorParent.transform.Find("TrapDoor")?.gameObject.SetActive(true);
        doorParent.transform.Find("ClearBox")?.gameObject.SetActive(true);
    }

    public void ChangeMinimapCellSprite(GameObject cell, string spriteName)
    {
        cell.SetActive(true);
        cell.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(spriteName);
    }

    public void ChangeMinimapadjacencentCellSprite(RoomClass next, RoomClass before)
    {
        if (next == null) return;

        string[] cellSprite = { "minimap1_4", "minimap1_3", "minimap1_2", };
        GameObject go = Managers.UI.PlayingUI.GetMinimapPannel();

        //다음 방
        ChangeMinimapCellSprite(go.transform.Find(next.RoomObject.name).gameObject, cellSprite[(int)ECellType.currentRoom]);

        //이전 방 (원래 있던 방)
        if (before != null)
        {
            int cellSpriteIedex = (int)ECellType.UnVisited;
            if (before.IsClear) cellSpriteIedex = (int)ECellType.Visited;
            ChangeMinimapCellSprite(go.transform.Find(before.RoomObject.name).gameObject, cellSprite[cellSpriteIedex]);
        }
        
        //바뀐 뒤 인접한 방
        for (int i = 0; i < 4; i++)
        {

            RoomClass adjacencentRoom = next._adjacencentRooms[i];
            if (adjacencentRoom != null)
            {
                Transform child = go.transform.Find(adjacencentRoom.RoomObject.name);
                if (adjacencentRoom.RoomType != ERoomType.Normal && adjacencentRoom.RoomType != ERoomType.Start)
                {
                    //roomIcon
                    GameObject temp = child.GetChild(0).gameObject;
                    temp.SetActive(true);
                    temp.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("minimap_icons_" + (int)adjacencentRoom.RoomType);
                }


                child.gameObject.SetActive(true);
                string spriteName;
                if (adjacencentRoom.IsClear)
                    spriteName = cellSprite[(int)ECellType.Visited];
                else
                    spriteName = cellSprite[(int)ECellType.UnVisited];

                ChangeMinimapCellSprite(child.gameObject, spriteName);
                //Debug.Log(adjacencentRoom.RoomObject.name + child.gameObject.activeSelf);
            }

        }

    }

    public void ChangeRoomActive(RoomClass next, RoomClass before)
    {
        if (next == null) return;

       
        //이전 방 (원래 있던 방)
        if (before != null)
        {
            FindChildByName(Map.transform, before.RoomObject.name).gameObject.SetActive(false);
        }
        
        if (before != null)
        {
            for (int i = 0; i < 4; i++)
            {
                RoomClass adjacencentRoom = before._adjacencentRooms[i];
                if (adjacencentRoom != null)
                {
                    FindChildByName(Map.transform, adjacencentRoom.RoomObject.name).gameObject.SetActive(false);
                }

            }
        }

        //다음 방
        FindChildByName(Map.transform, next.RoomObject.name).gameObject.SetActive(true);

        //바뀐 뒤 인접한 방
        for (int i = 0; i < 4; i++)
        {
            RoomClass adjacencentRoom = next._adjacencentRooms[i];
            if (adjacencentRoom != null)
            {
                FindChildByName(Map.transform, adjacencentRoom.RoomObject.name).gameObject.SetActive(true);
            }

        }

    }


    //실행 도중 collider가 변경될 필요가 있을 때 사용
    //더이상 사용되지 않음
    public void ChangeCollider(RoomClass room)
    {
        for (int i = 0; i < 4; i++)
        {
            int[] dx = { 8, -1, -10, -1 };
            int[] dy = { -1, -6, -1, 5 };

            int[] qx = { 8, -2, -10, -2 };
            int[] qy = { -2, -6, -2, 4 };
            int[] qx2 = { 8, 0, -10, 0 };
            int[] qy2 = { 0, -6, 0, 4 };

            float[] qx3 = { 0, 0f, 0, 0f };
            float[] qy3 = { -0.3f, 0, -.3f, 0.2f };
            if (room._adjacencentRooms[i] != null)
            {
                int nx = 0 + dx[i];
                int ny = 0 + dy[i];
                Vector3Int newPos = new(nx, ny);
                Tilemap tm = room.CollidePrefab.GetComponent<Tilemap>();
                if (i == 3)
                    tm.SetTransformMatrix(newPos, Matrix4x4.Translate(new Vector3(qx3[i], qy3[i], 0)));
                else
                    tm.SetTile(newPos, null);
                tm.SetTransformMatrix(new Vector3Int(qx[i], qy[i], 0), Matrix4x4.Translate(new Vector3(qx3[i], qy3[i], 0)));
                tm.SetTransformMatrix(new Vector3Int(qx2[i], qy2[i], 0), Matrix4x4.Translate(new Vector3(-qx3[i], -qy3[i], 0)));
            }
        }
    }

    public void DestroyMap()
    {
        Rooms.Clear();

        if (Map != null)
            UnityEngine.Object.Destroy(Map);

        GameObject go = Managers.UI.PlayingUI.GetMinimapPannel();
        foreach (Transform child in go.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        CurrentRoom = null;
    }

    //RoomEditing 적용을 위한 함수
    //에디팅 툴에서 장애물을 만들면 적용해서 충돌배열에 저장
    void ParseRoomCollisionData()
    {

        if (CellGrid == null)
            return;

        foreach (var room in Rooms)
        {
            StageColXMin = Math.Min(StageColXMin, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.xMin + (int)room.Transform.position.x);
            StageColXMax = Math.Max(StageColXMax, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.xMax - 1 + (int)room.Transform.position.x);
            StageColYMin = Math.Min(StageColYMin, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.yMin + (int)room.Transform.position.y);
            StageColYMax = Math.Max(StageColYMax, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.yMax - 1 + (int)room.Transform.position.y);
        }

        //Debug.Log(xMin + ", " + yMin + ", " + xMax + ", " + yMax);
        //Debug.Log(xMax - xMin + 1 + ", " + (yMax - yMin + 1));

        collisionData = new int[StageColYMax - StageColYMin + 1, StageColXMax - StageColXMin + 1];

        foreach (var room in Rooms)
        {
            Tilemap tm = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
            for (int y = tm.cellBounds.yMax - 1; y >= tm.cellBounds.yMin; y--)
            {
                for (int x = tm.cellBounds.xMin; x < tm.cellBounds.xMax; x++)
                {

                    int collsionInt = 0;
                    if (!tm.HasTile(new Vector3Int(x, y))) continue;
                    var temp = tm.GetTile(new Vector3Int(x, y));
                    string t = tm.GetTile(new Vector3Int(x, y)).name;

                    switch (t)
                    {
                        case "CannotGo":
                        case "Rock":
                        case "Urn":
                            collsionInt = (int)ECellCollisionType.Wall;
                            break;
                        case "CanGo":
                            collsionInt = (int)ECellCollisionType.None;
                            break;
                        case "Door":
                        case "Spike":
                        case "Fire":
                        case "Poof":
                            collsionInt = (int)ECellCollisionType.SemiWall;
                            break;
                    }
                    collisionData[y + math.abs(StageColYMin) + (int)room.WorldCenterPos.y, x + math.abs(StageColXMin) + (int)room.WorldCenterPos.x] = collsionInt;
                }
            }
        }
        var parser = File.CreateText($"Assets/@Resources/Data/MapData/StageCollision.txt");
        for (int i = StageColYMax - StageColYMin; i >= 0; i--)
        {
            parser.Write("{0,-4}", i);
            for (int j = 0; j <= StageColXMax - StageColXMin; j++)
            {
                parser.Write(collisionData[i, j]);
            }
            parser.WriteLine();
        }
        parser.Close();


    }

    // Obstacle gameobject에 prefab을 소환하는 방식
    // 단 spike, hole, fire의 장작 부분은 타일맵에 타일을 올리는 방식으로
    public void SetObstacle(RoomClass room)
    {
        RNGManager obsRng = new RNGManager(room.ObjectSeed);
        Tilemap tmp = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
        int maxX = tmp.cellBounds.xMax;
        int minX = tmp.cellBounds.xMin;
        int maxY = tmp.cellBounds.yMax;
        int minY = tmp.cellBounds.yMin;

        for (int y = maxY - 1; y > minY; y--)
        {
            for (int x = minX; x < maxX - 1; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tile = tmp.GetTile(tilePos);
                int index;
                switch (tile.name)
                {
                    case "CannotGo":
                        break;
                    case "CanGo":
                        break;
                    case "Door":
                        break;
                    case "Spike":
                        Managers.Object.SpawnObstacle(tilePos, "Spike", room.Obstacle.transform);
                        break;
                    case "Fire":
                        Managers.Object.SpawnObstacle(tilePos, "Fire", room.Obstacle.transform);
                        break;
                    case "ItemHolder":
                        room.ItemHolder = Managers.Resource.Instantiate("ItemHolder");
                        int TemplateId = Managers.Game.SlectItem();
                        room.ItemHolder.GetComponent<ItemHolder>().ItemOfItemHolder = new Item(TemplateId);
                        room.ItemHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[TemplateId].SpriteName);
                        // SetParent      vs  parent
                        // 로컬 좌표 유지      로컬 좌표가 부모 기준으로 변경
                        room.ItemHolder.transform.SetParent(room.Obstacle.transform);
                        room.ItemHolder.transform.position = (room.Transform.position + new Vector3(-0.5f, -0.5f));
                        if (room.RoomType == ERoomType.Boss)
                            room.ItemHolder.SetActive(false);
                        break;
                    case "Poop":
                        Managers.Object.SpawnObstacle(tilePos, "Poop", room.Obstacle.transform);
                        break;
                    case "Rock":
                        index = obsRng.RandInt(1, 3);
                        Managers.Object.SpawnObstacle(tilePos, "Rock", room.Obstacle.transform, index);
                        break;
                    case "Urn":
                        index = obsRng.RandInt(1, 3);
                        Managers.Object.SpawnObstacle(tilePos, "Urn", room.Obstacle.transform, index);
                        break;
                    default:
                        break;
                }

            }
        }
        room.ObjectSeed = obsRng.Sn;
        
    }
    public void SpawnMonsterAndBossInRoom(RoomClass room, Action callback = null)
    {
        Tilemap tmp = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
        int maxX = tmp.cellBounds.xMax;
        int minX = tmp.cellBounds.xMin;
        int maxY = tmp.cellBounds.yMax;
        int minY = tmp.cellBounds.yMin;
        
        tmp = room.TilemapCollisionPrefab.transform.GetChild(0).GetComponent<Tilemap>();

        for (int y = maxY - 1; y > minY; y--)
        {
            for (int x = minX; x < maxX - 1; x++)
            {
                TileBase tile = tmp.GetTile(new Vector3Int(x, y));
                if (tile == null) continue;

                string name = tile.name;
                string result = Regex.Replace(name, @"[^0-9]", "");

                //TODO
                //id 값 조절
                if (result != "")
                {
                    Transform monsterSpawnTransform = FindChildByName(CurrentRoom.Transform, "Monster");
                    Int32.TryParse(result, out int id);
                    id += 10000;
                    if (id < 20000)
                    {
                        Managers.Object.Spawn<Monster>(new Vector3Int(x, y), id, Managers.Data.MonsterDic[id].PrefabName, monsterSpawnTransform);
                    }
                    else
                    {
                        Managers.Object.Spawn<Boss>(new Vector3Int(x, y), id, Managers.Data.MonsterDic[id].PrefabName, monsterSpawnTransform);
                        Managers.UI.PlayingUI.BossHpActive(true);
                    }
                }
            }
        }

        callback.Invoke();
    }

}

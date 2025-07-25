using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static Define;
using static Utility;
using Object = UnityEngine.Object;
using Transform = UnityEngine.Transform;

public class RoomClass
{
    public EDeadEndType DeadEndType { get; set; }
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
            == (int)ECellCollisionType.Wall || collisionData[y + math.abs(StageColYMin), x + math.abs(StageColXMin)]
            == (int)ECellCollisionType.SemiWall) return false;
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
                if (SceneManager.GetActiveScene().name != "DevScene")
                {
                    ChangeMinimapadjacencentCellSprite(value, _currentRoom);
                    ChangeRoomActive(value, _currentRoom);
                }
                _currentRoom = value;
            }
        }
    }

    private Dictionary<int, int> BossToStage = new Dictionary<int, int>();

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
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0, };

        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (s_roomGraph[i, j] == 1) break;
                int adjacent = 0;
                int weighjt = 14;


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
            RoomType = ERoomType.Start
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
            // 보스방이 시작방과 붙어있는 경우는 맵이 잘못 생성된것
            // 보스방과 붙어있는 인접방의 형태를 기반으로 DeadEndType 보스방 위에 인접방이 있으면 U
            // R D L U
            RoomClass temp = BossRoom._adjacencentRooms[i];
            if (temp == null) continue;
            if (temp == StartingRoom) return 0;
            BossRoom.DeadEndType = (EDeadEndType)(i + 1);
        }

        BossRoom.RoomType = ERoomType.Boss;

        //5. 상점 설정
        if (dead_ends.Count > 0)
        {
            if (Managers.Game.StageNumber <= 5)
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
            if (Managers.Game.StageNumber <= 5)
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
                dead_ends.Pop().RoomClass.RoomType = ERoomType.Sacrifice;
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
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };
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
                rc.RoomType = Managers.Game.RNG.Chance(50) ? ERoomType.Devil : ERoomType.Angel;
            }
        }

        //11. special 배열이 빌 때까지 일반 방 설정
        while (dead_ends.Count > 0)
        {
            var temp = dead_ends.Pop().RoomClass.DiffiCulty = Managers.Game.RNG.Choice(_difficulty);
        }


        Rooms.Add(StartingRoom);

        SetSecretAndDevilAngelRoomAdj();

#if UNITY_EDITOR
        //temp
        using (var parser = File.CreateText($"Assets/@Resources/Data/MapData/Stage.txt"))
        {
            for (int i = 0; i < s_mapMaxXofRoomArray; i++)
            {
                for (int j = 0; j < s_mapMaxYofRoomArray; j++)
                {
                    if (s_roomGraph[i, j] == 1)
                    {
                        parser.Write((GetRoomClassByPos(Rooms, i, j).RoomType.ToString() + "(" + i + "," + j + ")").PadLeft(10));
                    }
                    else
                    {
                        parser.Write("".PadLeft(10));
                    }
                }
                parser.WriteLine();
            }
        }
#endif
        return 1;
    }

    public void SetSecretAndDevilAngelRoomAdj()
    {
        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { 1, 0, -1, 0 };

        foreach (var room in Rooms)
        {
            if (room.RoomType == ERoomType.Secret)
            {
                for (int k = 0; k < 4; k++)
                {
                    int nx = (int)room.XPos + dx[k];
                    int ny = (int)room.YPos + dy[k];
                    if (nx < 0 || nx >= s_mapMaxXofRoomArray) break;
                    if (ny < 0 || ny >= s_mapMaxYofRoomArray) break;
                    if (s_roomGraph[nx, ny] == 1)
                    {
                        RoomClass rc = null;
                        foreach (var r in Rooms)
                        {
                            if (r.XPos == nx && r.YPos == ny)
                            {
                                rc = r;
                                room._adjacencentRooms[k] = rc;
                                rc._adjacencentRooms[(k + 2) % 4] = room;
                            }
                        }
                        // 0 1 2 3
                        // R D L U
                        // 

                    }
                }
            }
        }

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

            devRoom.AwardSeed = Managers.Game.RNG.Sn;
            CurrentRoom = devRoom;
            //ParseRoomCollisionData();
            return;
        }

        // 사용될 보스방 미리 배정
        List<int> bossRoomList = new();
        // 5스테이지를 제외한 스테이지에서 사용되는 보스방의 index로 list 초기화
        for (int i = 0; i < Managers.Data.RoomDic[ERoomType.Boss][0].Count; i++)
        {
            bossRoomList.Add(Managers.Data.RoomDic[ERoomType.Boss][0][i]);
        }

        Shuffle(bossRoomList, Managers.Game.RNG);

        // 마지막 스테이지를 제외하고 보스가 출연할 스테이지 적용
        for (int stage = 1; stage <= Managers.Game.MAXStageNumber - 1; stage++)
        {
            BossToStage[stage] = bossRoomList[stage - 1];
        }

        // 5스테이지에 보스방 설정
        BossToStage[Managers.Game.MAXStageNumber] = Managers.Data.RoomDic[ERoomType.Boss][5][0];
        // 예비로 사용될 보스방
        BossToStage[Managers.Game.MAXStageNumber + 1] = bossRoomList[Managers.Game.MAXStageNumber - 1];


        Managers.Resource.LoadAllAsync<Object>("InGame", (key, count, totalCount) =>
        {
            foreach (string name in Enum.GetNames(typeof(ERoomType)))
            {
                if (key.Contains(name) && key.Contains("Tile_Map"))
                {
                    RoomCollisionCnt[(int)Enum.Parse(typeof(ERoomType), name)]++;
                }
            }

            Debug.Log($"{key} {count}/{totalCount}");

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


    #region Room prefab Initialization
    public void GenerateRoom(RoomClass r, int index)
    {
        //1.room 생성
        r.WorldCenterPos = CalcRoomsWorldPos(r);

        //room prfab 생성
        GameObject room = CreateRoomPrefab(r);

        // Select Random Map Collision
        int roomId = SelectRoomId(r);

        InitRoomClassField(r, room, roomId);

        //room 위치, 이름 조정
        SetRoomNameAndPos(r, room, index);

        SetObstacle(r);
        GenerateDoor(r);
        r.TilemapCollisionPrefab.SetActive(false);


        AltSetActive(r.RoomObject, r.RoomType == ERoomType.Start);
    }

    private Vector2 CalcRoomsWorldPos(RoomClass r)
    {
        const int roomWidth = 21;
        const int roomHeight = 13;

        //배열과 좌표의 차이 때문에 신경써줘야함
        //배열은 0,0이 왼쪽 위
        float xDiif = r.YPos - StartingPos.y;
        float yDiif = StartingPos.x - r.XPos;
        Vector2 posDiff = new Vector2(xDiif, yDiif) * new Vector2(roomWidth, roomHeight);

        return posDiff;
    }

    private GameObject CreateRoomPrefab(RoomClass r)
    {
        // Room_<RoomType>_(stagenum_)
        string roomPrefabName = "Room_";

        if (r.RoomType == ERoomType.Start && Managers.Game.StageNumber == 1)
        {
            roomPrefabName += r.RoomType.ToString();
        }
        else if (r.RoomType is ERoomType.Gold or ERoomType.Boss or ERoomType.Normal or ERoomType.Start)
        {
            roomPrefabName += "Normal_" + Managers.Game.StageNumber;
        }
        else
        {
            roomPrefabName += r.RoomType.ToString();
        }

        return Managers.Resource.Instantiate(roomPrefabName);
    }

    private int GetBossRoomId(EDeadEndType currentDeadEndType)
    {
        int selectedId = BossToStage[Managers.Game.StageNumber];
        var selectedRoom = Managers.Data.RoomDicTotal[selectedId];
        // 소환할 수 없는 DeadEnd의 경우 예비로 남긴 방 사용
        if (selectedRoom.CannotSpawnType == currentDeadEndType)
        {
            // 예비 보스 룸 ID
            selectedId = BossToStage[Managers.Game.MAXStageNumber + 1];
        }
        return selectedId;
    }

    private int GetRoomId(ERoomType type)
    {
        //현재 에디팅한 방이 많지 않아서 현재는 0스테이지에 모두 몰아 넣었음
        //향후에는 Manager.Game.StageNumber 사용
        int stage = 0;
        var rooms = Managers.Data.RoomDic[type][stage];
        int index = Managers.Game.RNG.RandInt(0, rooms.Count - 1);
        return rooms[index];
    }

    private void InitRoomClassField(RoomClass r, GameObject room, int roomId)
    {
        //클래스 멤버 변수 초기화

        // Room instance 생성
        GameObject roomTileMap = InstantiateRoom(roomId, room.transform);

        r.RoomObject = room;
        r.Transform = room.transform;
        r.TilemapPrefab = FindChildByName(r.Transform, "Tilemap").gameObject;
        r.Obstacle = FindChildByName(r.Transform, "Obstacle").gameObject;
        r.CollidePrefab = FindChildByName(r.Transform, "Collider").gameObject;
        r.Doors = FindChildByName(r.Transform, "Doors").gameObject;
        r.TilemapCollisionPrefab = roomTileMap;
        r.Tilemap = r.TilemapPrefab.GetComponent<Tilemap>();
    }
    private GameObject InstantiateRoom(int roomId, Transform parent)
    {
        string prefabName = Managers.Data.RoomDicTotal[roomId].PrefabName;
        GameObject instance = Managers.Resource.Instantiate(prefabName);
        instance.transform.SetParent(parent);
        return instance;
    }

    private int SelectRoomId(RoomClass r)
    {
        // 보스방
        if (r.RoomType == ERoomType.Boss)
        {
            return GetBossRoomId(r.DeadEndType);
        }
        // 나머지 모든 방
        else
        {
            return GetRoomId(r.RoomType);
        }
    }
    private void SetRoomNameAndPos(RoomClass r, GameObject room, int index)
    {
        room.transform.Translate(r.WorldCenterPos);
        room.name = Managers.Game.Seed + " " + Managers.Game.StageNumber + r.RoomType.ToString() +
            (r.RoomType == ERoomType.Normal ? index : "");
        room.transform.parent = Map.transform;
    }

    //Door Anim 할당
    private void GenerateDoor(RoomClass room)
    {
        for (int i = 0; i < 4; i++)
        {
            if (room._adjacencentRooms[i] != null)
            {
                ERoomType doorType = room.RoomType;
                string animatorName = "Door";
                if ((int)room.RoomType < (int)room._adjacencentRooms[i].RoomType)
                    doorType = room._adjacencentRooms[i].RoomType;

                switch (doorType)
                {
                    case ERoomType.Normal:
                    case ERoomType.Start:
                        animatorName += "Normal";
                        break;
                    case ERoomType.Angel:
                        animatorName += "Angel";
                        break;
                    case ERoomType.Devil:
                        animatorName += "Devil";
                        break;
                    case ERoomType.Secret:
                        animatorName += "Secret";
                        break;
                    case ERoomType.Shop:
                        animatorName += "Shop";
                        break;
                    case ERoomType.Boss:
                        animatorName += "Boss";
                        break;
                    case ERoomType.Sacrifice:
                        animatorName += "Sacrifice";
                        break;
                    case ERoomType.Gold:
                        animatorName += "Golden";
                        break;
                    case ERoomType.Curse:
                        animatorName += "Curse";
                        break;
                    default:
                        Debug.Log("RoomType Err Not Existing Type (MapManager)");
                        break;
                }

                // 문에 맞는 애니메이션 컨트롤러 할당
                room.Doors.GetComponent<Door>().SetAnimator(i, doorType, room.RoomType, animatorName);
            }
            else
            {
                room.Doors.transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        if (room.RoomType == ERoomType.Boss)
        {
            //if (Managers.Game.StageNumber == 8)
            if (Managers.Game.StageNumber == 5)
                GenerateClearBox(room);
            else
                GenerateTrapDoor(room);
        }
    }

    private void GenerateTrapDoor(RoomClass room, Vector3 doorPos)
    {
        GameObject go = GenerateTrapDoor(room);
        go.transform.transform.position += doorPos;
    }

    private GameObject GenerateTrapDoor(RoomClass room)
    {
        GameObject go = Managers.Resource.Instantiate("TrapDoor", room.Doors.transform);
        go.transform.position = room.Doors.transform.position + new Vector3(0.5f, 0.5f);
        go.SetActive(false);
        return go;
    }

    private GameObject GenerateClearBox(RoomClass room)
    {
        GameObject go = Managers.Resource.Instantiate("ClearBox", room.Doors.transform);
        go.transform.position = room.Doors.transform.position + new Vector3(0.5f, 0.5f);
        go.SetActive(false);
        return go;
    }

    #endregion


    public void ChangeDoorAnim(RoomClass room, EDoorState doorState)
    {
        GameObject doorParent = room.Doors;
        Door doorScript = doorParent.GetComponent<Door>();

        switch (doorState)
        {
            case EDoorState.Open:
                doorParent.transform.Find("TrapDoor")?.gameObject.SetActive(true);
                doorParent.transform.Find("ClearBox")?.gameObject.SetActive(true);
                //doorScript.Open();
                break;
            case EDoorState.BrokenOpen:
                break;
            case EDoorState.CoinOpen:
                break;
            case EDoorState.KeyOpen:
                break;
            case EDoorState.KeyOpenGolden:
                break;
            case EDoorState.KeyOpenNoKey:
                break;
            case EDoorState.Opened:
                //doorScript.Opened();
                break;
            case EDoorState.Close:
                //doorScript.Close();
                break;
            case EDoorState.Closed:
                //doorScript.Closed();
                break;
            case EDoorState.CoinClosed:
                break;
            case EDoorState.KeyClosed:
                break;
            case EDoorState.Broken:
                break;
            case EDoorState.Hidden:
                break;


        }
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
                // 비밀방 미니맵 컨트롤은 Door에서 관리
                if (adjacencentRoom.RoomType == ERoomType.Secret && adjacencentRoom.IsClear == false) continue;
                if (next.RoomType == ERoomType.Secret) continue;

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

        //에디터에서는 다 보여주고, 실제에서는 원래방만
        //이전 방 (원래 있던 방)
        if (before != null)
        {
            AltSetActive(FindChildByName(Map.transform, before.RoomObject.name).gameObject, false);
        }

        //if (before != null)
        //{
        //    for (int i = 0; i < 4; i++)
        //    {
        //        RoomClass adjacencentRoom = before._adjacencentRooms[i];
        //        if (adjacencentRoom != null)
        //        {
        //            AltSetActive(FindChildByName(Map.transform, adjacencentRoom.RoomObject.name).gameObject, false);
        //        }

        //    }
        //}

        //다음 방
        AltSetActive(FindChildByName(Map.transform, next.RoomObject.name).gameObject, true);

        //바뀐 뒤 인접한 방
        //for (int i = 0; i < 4; i++)
        //{
        //    RoomClass adjacencentRoom = next._adjacencentRooms[i];
        //    if (adjacencentRoom != null)
        //    {
        //        AltSetActive(FindChildByName(Map.transform, adjacencentRoom.RoomObject.name).gameObject, true);
        //    }
        //}
    }

    public void AltSetActive(GameObject room, bool state)
    {
        FindChildByName(room.transform, "Obstacle")?.gameObject.SetActive(state);
        FindChildByName(room.transform, "Collider")?.gameObject.SetActive(state);
        FindChildByName(room.transform, "ProjectileCollider")?.gameObject.SetActive(state);
        foreach (Transform child in FindChildByName(room.transform, "Doors"))
        {
            foreach (Transform sprites in child)
                sprites.gameObject.SetActive(state);
        }
        FindChildByName(room.transform, "Pickups")?.gameObject?.SetActive(state);


        FindChildByName(room.transform, "Monster")?.gameObject.SetActive(state);
        FindChildByName(room.transform, "ShopItems")?.gameObject.SetActive(state);

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
        StageColXMin = 1000;
        StageColYMin = 1000;
        StageColXMax = -1000;
        StageColYMax = -1000;

        if (CellGrid == null)
            return;

        foreach (var room in Rooms)
        {
            StageColXMin = Math.Min(StageColXMin, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.xMin + 1 + (int)room.Transform.position.x);
            StageColXMax = Math.Max(StageColXMax, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.xMax - 1 + (int)room.Transform.position.x);
            StageColYMin = Math.Min(StageColYMin, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.yMin + 1 + (int)room.Transform.position.y);
            StageColYMax = Math.Max(StageColYMax, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.yMax - 1 + (int)room.Transform.position.y);
        }

        //Debug.Log(xMin + ", " + yMin + ", " + xMax + ", " + yMax);
        //Debug.Log(xMax - xMin + 1 + ", " + (yMax - yMin + 1));

        collisionData = new int[StageColYMax - StageColYMin + 1, StageColXMax - StageColXMin + 1];

        foreach (var room in Rooms)
        {
            Tilemap tm = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
            for (int y = tm.cellBounds.yMax - 1; y > tm.cellBounds.yMin + 1; y--)
            {
                for (int x = tm.cellBounds.xMin + 1; x < tm.cellBounds.xMax - 1; x++)
                {

                    int collsionInt = 0;
                    if (!tm.HasTile(new Vector3Int(x, y))) continue;
                    var temp = tm.GetTile(new Vector3Int(x, y));
                    string t = tm.GetTile(new Vector3Int(x, y)).name;
                    switch (t)
                    {
                        case "CannotGo":
                            collsionInt = (int)ECellCollisionType.Wall;
                            break;
                        case "CanGo":
                        case "pickuo_001_heart_0":
                        case "pickup_002_coin_0":
                        case "pickup_003_key_0":
                        case "pickup_005_chests_5":
                        case "pickup_005_chests_9":
                        case "pickup_016_bomb_0":
                            collsionInt = (int)ECellCollisionType.None;
                            break;
                        case "Urn":
                            collsionInt = (int)ECellCollisionType.SemiWall;
                            break;
                        case "Door":
                        case "Spike":
                        case "Fire":
                        case "Poop":
                        case "Rock":
                            break;
                        case "ItemHolder":
                            if (room.RoomType == ERoomType.Boss)
                                collsionInt = (int)ECellCollisionType.None;
                            break;
                        default:
                            break;
                    }
                    collisionData[y + math.abs(StageColYMin) + (int)room.WorldCenterPos.y, x + math.abs(StageColXMin) + (int)room.WorldCenterPos.x] = collsionInt;
                }
            }
        }
#if UNITY_EDITOR
        var parser = File.CreateText($"Assets/@Resources/Data/MapData/StageCollision.txt");
#else
        var exePath = Directory.GetParent(Application.dataPath).FullName;
        string filePath = Path.Combine(exePath, "StageCollision.txt");
        var parser = File.CreateText($"filePath");
#endif
        for (int i = StageColYMax - StageColYMin; i >= 0; i--)
        {
            parser.Write("{0,-4}", i);
            for (int j = 0; j <= StageColXMax - StageColXMin; j++)
            {
                parser.Write(collisionData[i, j]);
            }
            parser.WriteLine();
        }
        parser.Write("{0,-4}", StageColXMax);
        parser.Write("{0,-4}", StageColXMin);
        parser.Write("{0,-4}", StageColYMax);
        parser.Write("{0,-4}", StageColYMin);

        parser.Close();


    }

    // Obstacle gameobject에 prefab을 소환하는 방식
    // 단 spike, hole, fire의 장작 부분은 타일맵에 타일을 올리는 방식으로
    public void SetObstacle(RoomClass room)
    {
        // 오브젝트 스프라이트 종류를 정하기위한 rng
        RNGManager obsRng = new RNGManager(room.ObjectSeed);
        Tilemap tmp = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
        // 타일맵의 크기정보
        int maxX = tmp.cellBounds.xMax;
        int minX = tmp.cellBounds.xMin;
        int maxY = tmp.cellBounds.yMax;
        int minY = tmp.cellBounds.yMin;

        for (int y = maxY - 1; y > minY + 1; y--)
        {
            for (int x = minX + 1; x < maxX - 1; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tile = tmp.GetTile(tilePos);
                if (tile == null) continue;

                HandleObstacleTile(room, tile.name, tilePos, obsRng);

            }
        }
        // 시드 갱신
        room.ObjectSeed = obsRng.Sn;
    }

    private void HandleObstacleTile(RoomClass room, string tileName, Vector3Int tilePos, RNGManager rng)
    {
        // 스프라이트가 여러개 있어서 하나를 골라야하는 장애물
        string[] randomObstacles = { "Rock", "Urn" };
        // 스프라이트가 고정된 장애물
        string[] fixedObstacles = { "Spike", "Fire", "Poop" };

        if (tileName == "ItemHolder")
        {
            GameObject itemHolder = Managers.Resource.Instantiate("ItemHolder", room.Obstacle.transform);
            itemHolder.GetComponent<ItemHolder>().Init(room, tilePos);
            // 보스방은 클리어 한 후에 활성화
            if (room.RoomType == ERoomType.Boss)
                itemHolder.SetActive(false);

            room.ItemHolder = itemHolder;
        }
        else if (fixedObstacles.Contains(tileName))
        {
            Managers.Object.SpawnObstacle(tilePos, tileName, room.Obstacle.transform);
        }
        else if (randomObstacles.Contains(tileName))
        {
            int spriteIndex = rng.RandInt(1, 3);
            Managers.Object.SpawnObstacle(tilePos, tileName, room.Obstacle.transform, spriteIndex);
        }
        // "CanGo", "CannotGo", "Door" 등은 무시
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

    public void SpawnPickUp(RoomClass room, Action callback = null)
    {
        Tilemap tmp = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
        int maxX = tmp.cellBounds.xMax;
        int minX = tmp.cellBounds.xMin;
        int maxY = tmp.cellBounds.yMax;
        int minY = tmp.cellBounds.yMin;

        for (int y = maxY - 1; y > minY + 1; y--)
        {
            for (int x = minX + 1; x < maxX - 1; x++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                TileBase tile = tmp.GetTile(tilePos);
                Pickup pickup;
                switch (tile.name)
                {
                    case "pickuo_001_heart_0":
                        pickup = Managers.Object.Spawn<Pickup>(tilePos, EPICKUP_TYPE.PICKUP_HEART, FindChildByName(room.Transform, "Pickups"), default, true);
                        pickup.GetComponent<Collider2D>().enabled = true;
                        break;
                    case "pickup_002_coin_0":
                        pickup = Managers.Object.Spawn<Pickup>(tilePos, EPICKUP_TYPE.PICKUP_COIN, FindChildByName(room.Transform, "Pickups"), default, true);
                        pickup.GetComponent<Collider2D>().enabled = true;
                        break;
                    case "pickup_003_key_0":
                        pickup = Managers.Object.Spawn<Pickup>(tilePos, EPICKUP_TYPE.PICKUP_KEY, FindChildByName(room.Transform, "Pickups"), default, true);
                        pickup.GetComponent<Collider2D>().enabled = true;
                        break;
                    case "pickup_005_chests_5":
                        pickup = Managers.Object.Spawn<Pickup>(tilePos, EPICKUP_TYPE.PICKUP_CHEST, FindChildByName(room.Transform, "Pickups"), default, true);
                        pickup.GetComponent<Collider2D>().enabled = true;
                        break;
                    case "pickup_016_bomb_0":
                        pickup = Managers.Object.Spawn<Pickup>(tilePos, EPICKUP_TYPE.PICKUP_BOMB, FindChildByName(room.Transform, "Pickups"), default, true);
                        pickup.GetComponent<Collider2D>().enabled = true;
                        break;
                    case "ShopItem":
                        Managers.Game.SpawnShopItem(tilePos, room);
                        break;
                    default:
                        break;
                }

            }
        }
    }

    public void ChangeCollisionData(float worldx, float worldy, ECellCollisionType colltype)
    {
        collisionData[(int)worldy + math.abs(StageColYMin), (int)worldx + math.abs(StageColXMin)] = (int)colltype;
        //Debug.Log($"after{collisionData[worldy + math.abs(StageColYMin) - 1, worldx + math.abs(StageColXMin) - 1]}");
    }
}

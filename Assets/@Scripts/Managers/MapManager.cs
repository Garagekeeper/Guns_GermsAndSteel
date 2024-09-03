using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;
using static RoomClass;
using Object = UnityEngine.Object;


public class RoomClass
{
    public enum ERoomType
    {
        Normal,
        Boss,
        Secret,
        Gold,
        Start,
        Curse,
        Sacrifice,
        Angel,
        Devil,
        Shop,
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
    private GameObject _doorFrame;
    private GameObject _doorSide1;
    private GameObject _doorSide2;
    private GameObject _doorCenter;
    private GameObject _obstacle;
    private Tilemap _tilemap;


    public GameObject TilemapPrefab { get { return _tilemapPrefab; } set { _tilemapPrefab = value; } }
    public GameObject TilemapCollisionPrefab { get { return _tilemap_CollisionPrefab; } set { _tilemap_CollisionPrefab = value; } }
    public GameObject CollidePrefab { get { return _ColliderPrefab; } set { _ColliderPrefab = value; } }
    public GameObject DoorFrame { get { return _doorFrame; } set { _doorFrame = value; } }
    public GameObject DoorSide1 { get { return _doorSide1; } set { _doorSide1 = value; } }
    public GameObject DoorSide2 { get { return _doorSide2; } set { _doorSide2 = value; } }
    public GameObject DoorCenter { get { return _doorCenter; } set { _doorCenter = value; } }

    public GameObject Obstacle { get { return _obstacle; } set { _obstacle = value; } }
    public Tilemap Tilemap { get { return _tilemap; } set { _tilemap = value; } }


    public int DiffiCulty { get; set; }

    //R D L U 순서
    public RoomClass[] _adjacencentRooms { get; set; }

    public RoomClass(int xPos, int yPos)
    {
        RoomType = ERoomType.Normal;
        XPos = xPos;
        YPos = yPos;
        _adjacencentRooms = new RoomClass[4];
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

    List<Vector3Int> _possibleVector = new List<Vector3Int>()
    {
        new Vector3Int(0, 1, 0), //Top
        new Vector3Int(1, 1, 0), //Top Right
        new Vector3Int(1, 0, 0), //Right
        new Vector3Int(1, -1, 0), //Bottom Right
        new Vector3Int(0, -1, 0), //Bottom
        new Vector3Int(-1, -1, 0), //Bottom Left
        new Vector3Int(-1, 0, 0), //Left
        new Vector3Int(-1, 1, 0), //Top Left
    };

    public bool CanGo(Vector3Int next)
    {
        int x = next.x;
        int y = next.y;

        if (x < _minimumX || x > _maximumX) return false;
        if (y < _minimumY || y > _maximumY) return false;
        if (_cellCollisionType[x + _maximumX, y + _maximumY] == ECellCollisionType.Wall) return false;
        return true;
    }

    /*
     *  -3 -2 -1 0 1 2 3
     *   0  1  2 3 4 5 6
     */

    public List<Vector3Int> FindPath(Creature crreature, Vector3Int startPos, Vector3Int destPos, int maxDepth = 10)
    {
        //Save Best Candidate
        Dictionary<Vector3Int, int> best = new Dictionary<Vector3Int, int>();

        //Path tracking
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

        PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();


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
        List<Vector3Int> cells = new List<Vector3Int>();

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
    public static int s_mapMaxX = 9;
    public static int s_mapMaxY = 9;
    public static int[,] s_roomGraph = new int[9, 9];
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

    public int XMin { get; set; } = 100;
    public int YMin { get; set; } = 100;
    public int XMax { get; set; } = 0;
    public int YMax { get; set; } = 0;

    public Vector2Int StartingPos { get; set; } = new Vector2Int(4, 4);

    public int StageNumber { get; set; } = 0;

    //int _baseRoomCountMax = 20;
    //int _baseRoomCountMin = 10;

    public RoomClass StartingRoom { get; set; }
    public RoomClass BossRoom { get; set; }
    public RoomClass CurrentRoom { get; set; }


    public bool CanCreateRoom(int x, int y)
    {
        if (x < 0 || x >= s_mapMaxX) return false;
        if (y < 0 || y >= s_mapMaxY) return false;
        if (s_roomGraph[x, y] == 1) return false;
        // 인접한 방이 2개 이상이면 방을 생성하지 않는다.
        if (CheckAdjacencyRoomCnt(x, y) >= 2) return false;
        // 위조건을 만족해도 50보다 작아야 방을 생성
        if (!Managers.Game.Chance(50)) return false;
        return true;
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
            if (nx < 0 || nx >= s_mapMaxX) continue;
            if (ny < 0 || ny >= s_mapMaxY) continue;
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


    public void GenerateStage()
    {
        Queue<RoomClass> roomClassQueue = new Queue<RoomClass>();
        RoomClass rc;
        Rooms = new List<RoomClass>();
        int roomCnt = 0;



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
            roomClassQueue.Enqueue(Managers.Game.Choice(Rooms));

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

        //3. special (인접한 방이 1개인 방들을 나열) 생성
        List<RoomClass> special = new List<RoomClass>();
        foreach (var room in Rooms)
        {
            //인접한 방이 1개이면 special추가
            if (room._adjacencentRooms.Count(s => s != null) == 1) special.Add(room);
            //아닌경우 방의 난이도 설정
            else room.DiffiCulty = Managers.Game.Choice(_difficulty);
        }

        //4. 보스방 설정
        //1) spcial에서 시작방과 인접하지 않은 방들을 넣은것
        List<RoomClass> boss = new List<RoomClass>();
        foreach (RoomClass spc in special)
        {
            bool startAdjacnecy = false;
            for (int i = 0; i < 4; i++)
            {
                RoomClass temp = spc._adjacencentRooms[i];
                if (temp == null) continue;
                if (temp == StartingRoom) startAdjacnecy = true;
                if (!startAdjacnecy)
                {
                    boss.Add(spc);
                    break;
                }
            }
        }

        //2) 지정된 보스방을 special에서 제거
        BossRoom = Managers.Game.Choice(boss);
        BossRoom.RoomType = RoomClass.ERoomType.Boss;
        special.Remove(BossRoom);

        //5.비밀방 설정
        if (special.Count > 0)
        {
            rc = Managers.Game.Choice(special);
            rc.RoomType = RoomClass.ERoomType.Secret;
            special.Remove(rc);

            //TODO
            //비밀방은 겉으로 표시되는게 아니라
            //연결되지 않은 취급하자
            //R D L U
            //0 1 2 3
            for (int i = 0; i < 4; i++)
            {
                if (rc._adjacencentRooms[i] != null)
                {
                    rc._adjacencentRooms[i]._adjacencentRooms[(i + 2) % 4] = null;
                    rc._adjacencentRooms[i] = null;
                }
            }
        }

        //6. 황금방 설정
        if (special.Count > 0)
        {
            rc = Managers.Game.Choice(special);
            rc.RoomType = RoomClass.ERoomType.Gold;
            special.Remove(rc);

            //방의 개수가 15개가 넘는경우 2개 배정을 시도한다.
            if (Managers.Game.N >= 15 && special.Count > 0)
            {
                if (Managers.Game.Chance(25))
                {
                    rc = Managers.Game.Choice(special);
                    rc.RoomType = RoomClass.ERoomType.Gold;
                    special.Remove(rc);
                }
            }
        }

        //7. 상점 설정
        if (special.Count > 0)
        {
            if (Managers.Game.N <= 15)
            {
                rc = Managers.Game.Choice(special);
                rc.RoomType = RoomClass.ERoomType.Shop;
                special.Remove(rc);
            }
            else if (Managers.Game.Chance(66))
            {
                rc = Managers.Game.Choice(special);
                rc.RoomType = RoomClass.ERoomType.Shop;
                special.Remove(rc);
            }
        }

        //8.천사방, 악마방 설정
        if (Managers.Game.Chance(20))
        {
            int[] dx = { 0, 0, 1, -1 };
            int[] dy = { 1, -1, 0, 0, };
            List<RoomClass> reward = new List<RoomClass>();
            for (int i = 0; i < 4; i++)
            {
                int nx = BossRoom.XPos + dx[i];
                int ny = BossRoom.YPos + dy[i];

                if (nx < 0 || nx >= s_mapMaxX) continue;
                if (ny < 0 || ny >= s_mapMaxY) continue;
                if (s_roomGraph[nx, ny] == 1) continue;
                if (nx > XMax || nx < XMin) continue;
                if (ny > YMax || ny < YMin) continue;
                RoomClass temp = new RoomClass(nx, ny);

                reward.Add(temp);
            }

            rc = Managers.Game.Choice(reward);
            Rooms.Add(rc);
            s_roomGraph[rc.XPos, rc.YPos] = 1;
            rc.RoomType = Managers.Game.Chance(50) ? RoomClass.ERoomType.Devil : RoomClass.ERoomType.Angel;
        }

        //9. 희생방 설정
        if (special.Count > 0)
        {
            bool isAngel = false;
            foreach (var temp in Rooms)
            {
                if (temp.RoomType == RoomClass.ERoomType.Angel) isAngel = true;
            }

            if (isAngel || Managers.Game.Chance(14))
            {
                rc = Managers.Game.Choice(special);
                rc.RoomType = RoomClass.ERoomType.Sacrifice;
                special.Remove(rc);
            }
        }

        //10. 저주방 설정
        if (special.Count > 0)
        {
            bool isDevil = false;
            foreach (var temp in Rooms)
            {
                if (temp.RoomType == RoomClass.ERoomType.Devil) isDevil = true;
            }

            if (isDevil && Managers.Game.Chance(50))
            {
                rc = Managers.Game.Choice(special);
                rc.RoomType = RoomClass.ERoomType.Curse;
                special.Remove(rc);
            }
        }

        //11. special 배열이 빌 때까지 일반 방 설정
        if (special.Count > 0)
        {
            foreach (var temp in special)
            {
                temp.DiffiCulty = Managers.Game.Choice(_difficulty);
            }
        }

        Rooms.Add(StartingRoom);


        //temp
        using (var parser = File.CreateText($"Assets/@Resources/Data/MapData/Stage.txt"))
        {
            for (int i = 0; i < s_mapMaxX; i++)
            {
                for (int j = 0; j < s_mapMaxY; j++)
                {
                    if (s_roomGraph[i, j] == 1)
                    {
                        parser.Write(GetRoomClassByPos(Rooms, i, j).RoomType.ToString().PadLeft(10));
                    }
                    else
                    {
                        parser.Write("".PadLeft(10));
                    }
                }
                parser.WriteLine();
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
        for (int i = 0; i < Enum.GetValues(typeof(ERoomType)).Length; i++)
            RoomCollisionCnt.Add(0);

        Managers.Resource.LoadAllAsync<Object>("Tile_Map_Collision", (key, count, totalCount) =>
        {
            foreach (string name in Enum.GetNames(typeof(ERoomType)))
            {
                if (key.Contains(name))
                {
                    RoomCollisionCnt[(int)Enum.Parse(typeof(ERoomType), name)]++;
                }
            }

            if (count == totalCount)
            {
                GenerateStage();
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
        Map = Managers.Resource.Instantiate("Stage");
        CellGrid = Map.GetComponent<Grid>();
        int index = 0;
        foreach (RoomClass r in Rooms)
        {
            //배열과 좌표의 차이 때문에 신경써줘야함
            //배열은 0,0이 왼쪽 위
            float xDiif = r.YPos - StartingPos.y;
            float yDiif = StartingPos.x - r.XPos;
            Vector2 posDiff = new Vector2(1 * xDiif, 1 * yDiif) * new Vector2(21, 13);
            r.WorldCenterPos = posDiff;
            //Debug.Log(r.RoomType + " diff: (" + posDiff.x + "|" + posDiff.y + ") ");

            GameObject room = Managers.Resource.Instantiate("Room");

            #region Select Random Map Collision
            string roomName;

            roomName = "Tile_Map_Collision_" + r.RoomType.ToString() + "_" + Managers.Game.RandInt(0, RoomCollisionCnt[(int)r.RoomType] - 1);
            GameObject roomTileMap = Managers.Resource.Instantiate(roomName);
            roomTileMap.transform.SetParent(room.transform);

            #endregion
            room.transform.Translate(posDiff);
            room.name = r.RoomType.ToString() + (r.RoomType == ERoomType.Normal ? index++ : "");
            room.transform.parent = Map.transform;
            r.Transform = room.transform;
            r.RoomObject = room;

            r.TilemapPrefab = room.transform.GetChild(0).gameObject;
            r.DoorFrame = room.transform.GetChild(1).gameObject;
            r.DoorSide1 = room.transform.GetChild(2).gameObject;
            r.DoorSide2 = room.transform.GetChild(3).gameObject;
            r.Obstacle = room.transform.GetChild(4).gameObject;
            r.CollidePrefab = room.transform.GetChild(5).gameObject;
            r.TilemapCollisionPrefab = roomTileMap;

            r.Tilemap = r.TilemapPrefab.GetComponent<Tilemap>();


            SetObstacle(r);
            ChangeDoorTile(r);
            r.TilemapCollisionPrefab.SetActive(false);
        }

        CurrentRoom = StartingRoom;

        //parse map collision data
        ParseRoomCollisionData();
    }

    //방의 클리어 조건을 충족했을 때 실행되는 함수
    //DoorTile이 열린 모양으로 변경됨
    public void RoomClear()
    {
        CurrentRoom.IsClear = true;
        if (CurrentRoom.ItemHolder != null )
        {
            int TemplateId = CurrentRoom.ItemHolder.GetComponent<ItemHolder>().ItemId;
            Managers.Data.ItemDic[TemplateId].Weight = 0;
        }
        ChangeDoorTile(CurrentRoom);
        ChangeCollider(CurrentRoom);
        foreach (var player in Managers.Object.MainCharacters)
        {
            if (player.OneTimeActive)
                Managers.Game.WithdrawOneTimeItemEffect(player);
        }
       
    }

    //Door Tile을 생성하거나, 변경하는 함수
    //TODO 여러 종류의 타일
    //여러 타일 바꾸도록
    public void ChangeDoorTile(RoomClass room)
    {
        string[] DoorTiles;
        if (room.IsClear)
            DoorTiles = new string[] { "door_01_normaldoor_0", "door_01_normaldoor_1", null };
        else
            DoorTiles = new string[] { "door_01_normaldoor_0", "door_01_normaldoor_2", "door_01_normaldoor_3" };
        //오른쪽 문
        //왼쪽문
        //문 프레임

        //TODO
        //이런식으로 레이어를 일일히 쌓아서 만드는 것 말고 다른 건 없을까
        for (int i = 0; i < 4; i++)
        {
            int[] dx = { 8, -1, -10, -1 };
            int[] dy = { -1, -6, -1, 4 };
            int[] Q = { 270, 180, 90, 0 };
            float[] qx = { -0.1f, 0.22f, 0.1f, -0.22f };
            float[] qy = { 0.22f, 0.1f, -0.22f, -0.1f };
            //            1       -1      1     -1
            //           -1        1      -1     1
            float[] qx2 = { -0.1f, -0.22f, 0.1f, 0.22f };
            float[] qy2 = { -0.22f, 0.1f, 0.22f, -0.1f };

            float[] qx3 = { -0.1f, 0, 0.1f, 0 };
            float[] qy3 = { 0, 0.1f, 0, -0.1f };
            if (room._adjacencentRooms[i] != null)
            {
                int nx = 0 + dx[i];
                int ny = 0 + dy[i];
                Quaternion qt = Quaternion.Euler(0, 0, Q[i]);
                Vector3 scale = new Vector3(3.5f, 3.5f, 0);
                Vector3Int newPos = new Vector3Int(nx, ny);

                Tilemap temp = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
                temp.SetTile(newPos, Managers.Resource.Load<Tile>("Door"));

                temp = room.DoorFrame.GetComponent<Tilemap>();
                temp.SetTile(newPos, Managers.Resource.Load<Tile>(DoorTiles[0]));
                temp.SetTransformMatrix(newPos, Matrix4x4.TRS(Vector3.zero, qt, scale));

                temp = room.DoorSide1.GetComponent<Tilemap>();
                newPos = new Vector3Int(nx, ny);
                temp.SetTile(newPos, Managers.Resource.Load<Tile>(DoorTiles[1]));
                if (room.IsClear)
                    temp.SetTransformMatrix(newPos, Matrix4x4.TRS(new Vector3(qx3[i], qy3[i]), qt, new Vector3(4f, 3.5f, 0)));
                else
                    temp.SetTransformMatrix(newPos, Matrix4x4.TRS(new Vector3(qx[i], qy[i], 0), qt, scale));

                temp = room.DoorSide2.GetComponent<Tilemap>();
                newPos = new Vector3Int(nx, ny);
                if (room.IsClear)
                    temp.SetTile(newPos, null);
                else
                    temp.SetTile(newPos, Managers.Resource.Load<Tile>(DoorTiles[2]));
                temp.SetTransformMatrix(newPos, Matrix4x4.TRS(new Vector3(qx2[i], qy2[i], 0), qt, scale));


            }
        }
    }

    //실행 도중 collider가 변경될 필요가 있을 때 사용
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
                Vector3Int newPos = new Vector3Int(nx, ny);
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
        if (Map != null)
            UnityEngine.Object.Destroy(Map);
    }

    //RoomEditing 적용을 위한 함수
    //에디팅 툴에서 장애물을 만들면 적용해서 충돌배열에 저장
    void ParseRoomCollisionData()
    {
        int xMin = int.MaxValue;
        int xMax = 0;
        int yMin = int.MaxValue;
        int yMax = 0;
        if (CellGrid == null)
            return;

        foreach (var room in Rooms)
        {
            xMin = Math.Min(xMin, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.xMin + (int)room.Transform.position.x);
            xMax = Math.Max(xMax, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.xMax - 1 + (int)room.Transform.position.x);
            yMin = Math.Min(yMin, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.yMin + (int)room.Transform.position.y);
            yMax = Math.Max(yMax, room.TilemapCollisionPrefab.GetComponent<Tilemap>().cellBounds.yMax - 1 + (int)room.Transform.position.y);
        }

        //Debug.Log(xMin + ", " + yMin + ", " + xMax + ", " + yMax);
        //Debug.Log(xMax - xMin + 1 + ", " + (yMax - yMin + 1));

        collisionData = new int[yMax - yMin + 1, xMax - xMin + 1];

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
                            collsionInt = (int)ECellCollisionType.Wall;
                            break;
                        case "CanGo":
                            collsionInt = (int)ECellCollisionType.None;
                            break;
                        case "Door":
                            collsionInt = (int)ECellCollisionType.SemiWall;
                            break;
                        case "Spike":
                            collsionInt = (int)ECellCollisionType.SemiWall;
                            break;
                        case "Fire":
                            collsionInt = (int)ECellCollisionType.SemiWall;
                            break;
                    }
                    collisionData[y + math.abs(yMin) + (int)room.WorldCenterPos.y, x + math.abs(xMin) + (int)room.WorldCenterPos.x] = collsionInt;
                }
            }
        }
        var parser = File.CreateText($"Assets/@Resources/Data/MapData/StageCollision.txt");
        for (int i = yMax - yMin; i >= 0; i--)
        {
            parser.Write("{0,-4}", i);
            for (int j = 0; j <= xMax - xMin; j++)
            {
                parser.Write(collisionData[i, j]);
            }
            parser.WriteLine();
        }
        parser.Close();


    }


    public void SetObstacle(RoomClass room)
    {
        Tilemap tmp = room.TilemapCollisionPrefab.GetComponent<Tilemap>();
        Tilemap obstmp = room.Obstacle.GetComponent<Tilemap>();
        int maxX = tmp.cellBounds.xMax;
        int minX = tmp.cellBounds.xMin;
        int maxY = tmp.cellBounds.yMax;
        int minY = tmp.cellBounds.yMin;

        for (int y = maxY - 1; y > minY; y--)
        {
            for (int x = minX; x < maxX - 1; x++)
            {
                TileBase tile = tmp.GetTile(new Vector3Int(x, y));

                switch (tile.name)
                {
                    case "CannotGo":
                        break;
                    case "CanGo":
                        break;
                    case "Door":
                        break;
                    case "Monster":
                        break;
                    case "Spike":
                        break;
                    case "Fire":
                        break;
                    case "ItemHolder":
                        room.ItemHolder = Managers.Resource.Instantiate("ItemHolder");
                        int TemplateId = Managers.Game.SlectItem();
                        room.ItemHolder.GetComponent<ItemHolder>().ItemId = TemplateId;
                        room.ItemHolder.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = Managers.Resource.Load<Sprite>(Managers.Data.ItemDic[TemplateId].SpriteName);
                        // SetParent      vs  parent
                        // 로컬 좌표 유지      로컬 좌표가 부모 기준으로 변경
                        room.ItemHolder.transform.SetParent(room.Obstacle.transform);
                        room.ItemHolder.transform.position = (room.Transform.position + new Vector3(-0.5f, -0.5f));
                        break;       
                }

            }
        }
    }

}

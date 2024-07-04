using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static Define;
public class MapManager
{
    private int _minimumX;
    private int _minimumY;
    private int _maximumX;
    private int _maximumY;

    public Vector3Int WorldToCell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }
    public Vector3 CellToWorld(Vector3Int cellPos) { return CellGrid.CellToWorld(cellPos); }

    public Grid CellGrid { get; private set; }

    ECellCollisionType[,] _cellCollisionType;

    //TODO 추후 인자 변경
    public void LoadMap(GameObject map ,string mapName)
    {
        CellGrid = map.GetComponent<Grid>();
        ParseRoomCollisionData(map, mapName);
    }

    void ParseRoomCollisionData(GameObject map, string mapName, string tileName = "Tile_Map_Collision")
    {
        GameObject temp = map.transform.Find(tileName).gameObject;
        if (temp != null)
            temp.SetActive(false);

        //collision info file
        TextAsset txt = Managers.Resource.Load<TextAsset>($"{mapName}Collision");
        StringReader sr = new StringReader( txt.text );

        _minimumX = int.Parse(sr.ReadLine());
        _maximumX = int.Parse(sr.ReadLine());
        _minimumY = int.Parse(sr.ReadLine());
        _maximumY = int.Parse(sr.ReadLine());

        int xCount = _maximumX - _minimumX + 1;
        int yCount = _maximumY - _minimumY + 1;
        _cellCollisionType = new ECellCollisionType[xCount, yCount];

        for (int y=0; y< yCount; y++)
        {
            string line = sr.ReadLine();
            for (int x=0; x< xCount; x++)
            {
                switch (line[x] )
                {
                    case MAP_TOOL_WALL:
                        _cellCollisionType[x, y] = ECellCollisionType.Wall;
                        break;
                    case MAP_TOOL_NONE:
                        _cellCollisionType[x, y] = ECellCollisionType.None;
                        break;
                    case MAP_TOOL_SEMI_WALL:
                        _cellCollisionType[x, y] = ECellCollisionType.SemiWall;
                        break;
                }
            }
        }

    }

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
}

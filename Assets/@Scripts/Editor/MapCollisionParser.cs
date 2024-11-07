using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

#if UNITY_EDITOR
using Newtonsoft.Json;
using UnityEditor;
#endif

public class MapCollisionParser
{
#if UNITY_EDITOR

    [MenuItem("Tools/GenerateMap %#m")]

    private static void GenerateMap()
    {
        GameObject[] gameObject = Selection.gameObjects;
        Tilemap tm = null; ;
        foreach (GameObject go in gameObject)
        {
            foreach (Tilemap component in go.GetComponentsInChildren<Tilemap>())
            {
                if (component.name == "Tile_Map_Collision")
                    tm = component;
            }

            using (var parser = File.CreateText($"Assets/@Resources/Data/MapData/{go.name}Collision.txt"))
            {
                parser.WriteLine(tm.cellBounds.xMin);
                parser.WriteLine(tm.cellBounds.xMax);
                parser.WriteLine(tm.cellBounds.yMin);
                parser.WriteLine(tm.cellBounds.yMax);

                for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
                {
                    for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
                    {
                        TileBase tile = tm.GetTile(new Vector3Int(x, y, 0));
                        if (tile != null)
                        {
                            if (tile.name.Contains("not"))
                                parser.Write(Define.MAP_TOOL_WALL);
                            else
                                parser.Write(Define.MAP_TOOL_NONE);

                        }
                        else
                            parser.Write(Define.MAP_TOOL_WALL);
                    }
                    parser.WriteLine();
                }
            }
        }
        Debug.Log("Map Collision Generation Complete");
    }
#endif
}

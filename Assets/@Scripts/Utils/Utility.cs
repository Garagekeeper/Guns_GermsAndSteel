using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public static class Utility
{
    public static T GetTFromParentComponent<T>(GameObject go) where T : Component
    {
        Transform current = go.transform;


        //부모로 이동
        while (current.parent != null)
        {
            T t = current.GetComponent<T>();
            if
                (t != null) return t;
            else
                current = current.parent;
        }

        //부모에서 컴포넌트를 찾음


        return current.GetComponent<T>();
    }

    public static Transform FindChildByName(Transform parent, string name, bool recur = true)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            if (recur)
            {
                // 자식의 자식 오브젝트에서 재귀적으로 찾기
                Transform found = FindChildByName(child, name);
                if (found != null)
                {
                    return found;
                }
            }
            
        }

        return null; // 이름에 해당하는 오브젝트를 찾지 못한 경우
    }

    public static Transform FindChildByNameContain(Transform parent, string name , bool recur = true)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(name))
            {
                return child;
            }

            if (recur)
            {
                // 자식의 자식 오브젝트에서 재귀적으로 찾기
                Transform found = FindChildByNameContain(child, name);
                if (found != null)
                {
                    return found;
                }
            }
        }

        return null; // 이름에 해당하는 오브젝트를 찾지 못한 경우
    }

    public static Vector2 VectorRotation2D(Vector2 vec, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        // 회전 행렬 계산
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        // 새 좌표 계산
        float x = vec.x * cos - vec.y * sin;
        float y = vec.x * sin + vec.y * cos;

        return new Vector2(x, y);
    }

    // Fisher-Yates 셔플
    public static void Shuffle(List<int> list, RNGManager rng = null)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = rng != null ? rng.RandInt(0, i) : Random.Range(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]); // Swap
        }
    }

    public static List<Vector3Int> SpriralPos(Vector3 start ,int count)
    {
        List<Vector3Int> pos = new();
        int x = (int)start.x;
        int y = (int)start.y;

        pos.Add(new Vector3Int(x, y, 0));

        int dirX = 1;
        int dirY = 0;
        
        // 한 변의 길이
        int lineLength = 1;

        while(pos.Count < 2 * count)
        {
            for (int i=0; i < lineLength; i++)
            {
                x += dirX;
                y += dirY;
                pos.Add(new Vector3Int(x, y, 0));
            }

            // 시계방향 회전
            int temp = dirX;
            dirX = dirY;
            dirY = -temp;

            //새로운 방향 벡터가 맨 처음의 방향 벡터와 평행할 때 거리가 늘어남
            if (dirY == 0) lineLength++;
        }

        return pos;
    }

    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        if (go.GetComponent<T>() == null)
            go.AddComponent<T>();

        return go.GetComponent<T>();
    }

    public static bool HasItem(int TemplateId)
    {
        foreach (MainCharacter player in Managers.Object.MainCharacters)
        {
            foreach (Item item in player.AcquiredPassiveItemList)
            {
                if (item.TemplateId == TemplateId) return true;
            }
        }

        return false;
    }
}

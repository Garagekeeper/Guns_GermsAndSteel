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

    public static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }

            // 자식의 자식 오브젝트에서 재귀적으로 찾기
            Transform found = FindChildByName(child, name);
            if (found != null)
            {
                return found;
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
}

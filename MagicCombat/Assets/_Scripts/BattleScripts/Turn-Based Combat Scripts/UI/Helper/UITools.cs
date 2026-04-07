using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UnityUIUtility
{
    public static void SetRectPosition(RectTransform rectTransform, Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }

    public static void SetLeft(this RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public static void SetRight(this RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public static void SetTop(this RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public static void SetBottom(this RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }

    public static float GetBottom(this RectTransform rt)
    {
        return rt.offsetMin.y;
    }
    public static float GetTop(this RectTransform rt)
    {
        return rt.offsetMax.y;
    }
    public static float GetRight(this RectTransform rt)
    {
        return rt.offsetMax.x;
    }
    public static float GetLeft(this RectTransform rt)
    {
        return rt.offsetMin.x;
    }
}

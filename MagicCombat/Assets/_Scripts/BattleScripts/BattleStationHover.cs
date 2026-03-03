using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleStationHover : MonoBehaviour
{
    bool isSelected;
    SpriteRenderer render;

    private void Awake()
    {
        render = GetComponent<SpriteRenderer>();
    }

    private void OnMouseEnter()
    {
        render.color = Color.yellow;
    }


    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isSelected = !isSelected;
        }
    }

    private void OnMouseExit()
    {
        if (!isSelected)
        {
            render.color = Color.white;
        }
    }


    public bool GetIsSelected () { return isSelected; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeData : MonoBehaviour
{
    //[HideInInspector]
    public Tile m_tile;
    //[HideInInspector]
    public Direction m_direction;

    [HideInInspector]
    public Node m_node;

    [HideInInspector]
    public Tile m_nextTile;

    public void AssignNewValues(Tile p_tile, Direction p_direction)
    {
        //Debug.Log(p_tile);
        m_nextTile = p_tile;
        m_direction = p_direction;

        if (m_node != null)
        {
            Place();
        }
    }

    public void ChangeSprite(Sprite p_sprite)
    {
        if (m_node != null)
        {
            GetComponent<SpriteRenderer>().sprite = p_sprite;
        }
    }

    public void Place()
    {
        //Debug.Log(m_nextTile);
        if (m_tile != null)
            m_tile.m_impassable = false;
        m_tile = m_nextTile;
        m_tile.m_impassable = true;
        transform.position = m_tile.m_coordinates;
    }
}
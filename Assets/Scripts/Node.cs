using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node //: MonoBehaviour
{
    public NodeData m_data;
    public Node m_next;

    //[HideInInspector]
    public Tile m_tile;
    //[HideInInspector]
    public Direction m_direction;

    [HideInInspector]
    public Node m_node;

    [HideInInspector]
    public Tile m_nextTile;

    public Node(NodeData p_data)
    {
        m_data = p_data;
        m_data.m_node = this;
        m_next = null;
    }
    /*
    public void AssignNewValues(Tile p_tile, Direction p_direction)
    {
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
    }*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [HideInInspector]
    public Node m_next;

    //[HideInInspector]
    public Tile m_tile;
    //[HideInInspector]
    public Direction m_direction;

    public Node()
    {
        m_next = null;
    }
    
    public void UpdateBody(Tile p_tile, Direction p_direction)
    {
        if (m_tile != null)
            m_tile.m_impassable = false;
        m_tile = p_tile;
        m_tile.m_impassable = true;
        transform.position = m_tile.m_coordinates;

        m_direction = p_direction;

    }

    public void ChangeSprite(Sprite p_sprite)
    {
        GetComponent<SpriteRenderer>().sprite = p_sprite;
    }

    public void RemoveTile()
    {
        m_tile.m_impassable = false;
    }
}

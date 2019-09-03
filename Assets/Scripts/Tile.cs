using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {
    Left,
    Right,
    Up,
    Down }

public class Tile
{
    public Vector2 m_coordinates;

    public bool m_impassable;
    public bool m_hasApple;

    public float GCost { get; set; }
    public float HCost { get; set; }

    public float FCost { get { return GCost + HCost; } }

    public Tile[] m_neighbours;

    public Tile(Vector2 p_coordinates, bool p_impassable = false)
    {
        m_coordinates = p_coordinates;
        m_impassable = p_impassable;
        m_neighbours = new Tile[4];
    }
}

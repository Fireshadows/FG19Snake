using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameDirector : MonoBehaviour
{
    //LinkedList m_linkedList;

    [SerializeField]
    private PlayerSnake m_player;

    [SerializeField]
    private Vector2 m_startingPosition;

    [SerializeField]
    private int m_rows;
    [SerializeField]
    private int m_columns;
    [SerializeField]
    private float m_updateRate;
    private float m_updateTime;

    [SerializeField]
    private GameObject m_apple;

    private Tile[,] m_tiles;
    private Tile m_appleTile;

    List<Tile> m_currentPath;

    private void Start()
    {
        CreateLevel();

        m_player = Instantiate(m_player.gameObject, Vector3.zero, Quaternion.identity).GetComponent<PlayerSnake>();
        m_player.Initialize(m_tiles[(int)m_startingPosition.x, (int)m_startingPosition.y], this);

        MoveApple(new Vector2(16, 10));

        m_updateTime = m_updateRate;
        NoFreeTiles();
    }

    private void CreateLevel()
    {
        m_tiles = new Tile[m_rows, m_columns];

        for (int x = 0; x < m_tiles.GetLength(0); x++)
        {
            for (int y = 0; y < m_tiles.GetLength(1); y++)
            {
                Tile m_newTile = new Tile(new Vector2(x, y));

                m_tiles[x, y] = m_newTile;
            }
        }

        for (int x = 0; x < m_tiles.GetLength(0); x++)
        {
            for (int y = 0; y < m_tiles.GetLength(1); y++)
            {
                if (x == 0) m_tiles[x, y].m_neighbours[(int)Direction.Left] = null;
                else m_tiles[x, y].m_neighbours[(int)Direction.Left] = m_tiles[x - 1, y];

                if (x == m_tiles.GetLength(0) - 1) m_tiles[x, y].m_neighbours[(int)Direction.Right] = null;
                else m_tiles[x, y].m_neighbours[(int)Direction.Right] = m_tiles[x + 1, y];

                if (y == m_tiles.GetLength(1) - 1) m_tiles[x, y].m_neighbours[(int)Direction.Up] = null;
                else m_tiles[x, y].m_neighbours[(int)Direction.Up] = m_tiles[x, y + 1];

                if (y == 0) m_tiles[x, y].m_neighbours[(int)Direction.Down] = null;
                else m_tiles[x, y].m_neighbours[(int)Direction.Down] = m_tiles[x, y - 1];
            }

        }

    }

    private void Update()
    {
        if (m_updateTime <= 0)
        {
            m_updateTime = m_updateRate;
            m_player.Move();
        }
        else
            m_updateTime -= Time.deltaTime;
    }
    public void OnAppleGrappled()
    {
        MoveApple(GetAnyFreeTile().m_coordinates);
    }

    private void MoveApple(Vector2 p_coordinates)
    {
        if (m_appleTile != null)
            m_appleTile.m_hasApple = false;

        m_apple.transform.position = p_coordinates;

        m_appleTile = m_tiles[(int)p_coordinates.x, (int)p_coordinates.y];
        m_appleTile.m_hasApple = true;
    }

    private bool NoFreeTiles()
    {
        var m_freeTiles = GetFreeTiles();

        if (m_freeTiles.Count() == 0)
            return true;
        return false;
    }

    private Tile GetAnyFreeTile()
    {
        var m_freeTiles = GetFreeTiles();

        if (m_freeTiles.Count() == 0)
            return null;

        return m_freeTiles.ElementAt(UnityEngine.Random.Range(0, m_freeTiles.Count()));
    }

    private IEnumerable<Tile> GetFreeTiles()
    {
        return from Tile m_tile in m_tiles
               where !m_tile.m_impassable
               select m_tile;
    }
    
    private List<Tile> ConstructPath(Tile p_startTile, Tile p_goalTile)
    {
        List<Tile> m_constructedPath = new List<Tile>();
        //Initialize open and closed lists
        List<Tile> m_openTiles = new List<Tile>();
        List<Tile> m_closedTiles = new List<Tile>();
        //Add start tile to it
        m_openTiles.Add(p_startTile);

        //Search for as long as open tiles is not empty
        while (m_openTiles.Count > 0)
        {
            Tile m_currentTile = null;
            int m_lowestFValue = 999;
            //Find the tile with the least cost
            foreach (Tile m_openTile in m_openTiles)
            {
                if (m_lowestFValue > m_openTile.fValue)
                {
                    m_lowestFValue = m_openTile.fValue;
                    m_currentTile = m_openTile;
                }
            }

            //If this tile is the goal then we've already found it
            if (m_currentTile == p_goalTile)
                break;

            //Remove the tile with least cost from the open list
            m_openTiles.Remove(m_currentTile);
            m_closedTiles.Add(m_currentTile);

            m_lowestFValue = 999;
            foreach (Tile m_neighbour in m_currentTile.m_neighbours)
            {
                if (m_closedTiles.Contains(m_neighbour))
                    continue;
                foreach (Tile m_tile in m_openTiles)
                {
                    if (m_tile == m_neighbour )
                    {

                    }
                }
                m_openTiles.Add(m_neighbour);
            }
        }
        return m_constructedPath;
    }

    float CalculateManhattanDistance(Vector2 p_start, Vector2 p_end)
    {
        return (p_start.x - p_end.x) + (p_start.y - p_end.y);
    }

    void DebuPath()
    {
        for (int i = 0; i < m_currentPath.Count-2; i++)
        {
            Debug.DrawRay(m_currentPath[i].m_coordinates, m_currentPath[i+1].m_coordinates);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerSnake : MonoBehaviour
{
    public Sprite m_headSprite;
    public Sprite m_tailSprite;

    public Sprite[] m_headSprites;
    public Sprite[] m_bodySprites;
    public Sprite[] m_tailSprites;

    public GameObject m_snakeBody;
    LinkedList m_linkedList;

    Direction Direction { get; set; }
    Direction m_oppositeDirection;

    public Tile Tile {
        get { return m_linkedList.m_root.m_tile; }
        set { m_linkedList.m_root.m_tile = value; }
    }
    
    private GameDirector m_gameDirector;

    [HideInInspector]
    public bool m_autoPilot;

    List<Tile> m_currentPath;

    public void Initialize(Tile p_tile, GameDirector p_gameDirector)
    {
        m_gameDirector = p_gameDirector;
        Direction = Direction.Right;

        m_linkedList = new LinkedList();
        AddHead(p_tile);
        AddBody();
        AddBody();
        UpdateAllSprites();
    }
    private void AddHead(Tile p_tile)
    {
        Node m_head = Instantiate(m_snakeBody.gameObject, transform).GetComponent<Node>();
        //m_head.GetComponent<SpriteRenderer>().sprite = m_headSprite;
        m_linkedList.PushFront(m_head);

        m_linkedList.m_root.UpdateBody(p_tile, Direction.Right);
    }
    public void AddBody()
    {
        Node m_body = Instantiate(m_snakeBody.gameObject, transform).GetComponent<Node>();

        //m_body.GetComponent<SpriteRenderer>().sprite = m_tailSprite;

        Node m_lastNode = m_linkedList.GetLastNode();
        m_body.UpdateBody(
            m_lastNode.m_tile.m_neighbours[(int)GetOppositeDirection(m_lastNode.m_direction)],
            m_lastNode.m_direction);

        m_linkedList.PushBack(m_body);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            m_autoPilot = !m_autoPilot;

        if (m_autoPilot)
            return;

        if (Input.GetKey(KeyCode.LeftArrow))
            ChangeDirection(Direction.Left);
        else if (Input.GetKey(KeyCode.UpArrow))
            ChangeDirection(Direction.Up);
        else if (Input.GetKey(KeyCode.RightArrow))
            ChangeDirection(Direction.Right);
        else if (Input.GetKey(KeyCode.DownArrow))
            ChangeDirection(Direction.Down);
    }
    
    public void OnUpdate()
    {
        if (m_autoPilot)
        {
            SteerAwayFromWall();

            /*
             * Since autopilot is on, we want to pathfind to the apple.
             * However, there are some checks we want to do in order for the snake to not trap itself so easily.
             * First is to check if its possible for the snake to find a path to the tile behind its tail.
             * In most cases, this means it will almost always have a way out when getting the apple.
             * The other thing we want to check is if the apple has at least 2 empty tiles around it.
             * If that would not be the case, the apple would be in a dead end or just inaccessible to the snake.
             */
            Tile m_tileBehindTail = m_linkedList.GetLastNode().m_tile.m_neighbours[(int)GetOppositeDirection(m_linkedList.GetLastNode().m_direction)];

            if (!TileIsImpassable(m_tileBehindTail) && ConstructPath(Tile, m_tileBehindTail) != null 
                && m_gameDirector.m_appleTile.m_neighbours.Count(m_tile => TileIsImpassable(m_tile)) < 3)
                m_currentPath = ConstructPath(Tile, m_gameDirector.m_appleTile);
            if (m_currentPath != null)
            {
                Tile m_nextTileInPath = m_currentPath[1];

                if (TileIsImpassable(m_nextTileInPath))
                {
                    m_currentPath = null;
                    return;
                }

                if (Tile.m_coordinates.x > m_nextTileInPath.m_coordinates.x)
                    Direction = Direction.Left;
                else if (Tile.m_coordinates.x < m_nextTileInPath.m_coordinates.x)
                    Direction = Direction.Right;
                else if (Tile.m_coordinates.y < m_nextTileInPath.m_coordinates.y)
                    Direction = Direction.Up;
                else if (Tile.m_coordinates.y > m_nextTileInPath.m_coordinates.y)
                    Direction = Direction.Down;
                m_currentPath.RemoveAt(0);
                DebugPath();
                if (m_currentPath.Count == 1)
                    m_currentPath = null;
            }
        }
        else m_currentPath = null;
        Move();
    }

    private void Move()
    {
        Tile m_newTile = Tile.m_neighbours[(int)Direction];

        if (TileIsImpassable(m_newTile))
        {
            m_gameDirector.GameOver();
            return;
        }
        if (Tile.m_hasApple)
        {
            m_gameDirector.OnAppleGrappled();
            AddBody();
        }

        MoveBody(m_newTile);

        SetOppositeDirection();
    }

    /// <summary>
    /// Takes the tail body of the snake and puts it where the head is, after placing the head on the new tile.
    /// The rest of the body doesn't need to be moved because nothing would visibly change regardless.
    /// </summary>
    private void MoveBody(Tile p_newTile)
    {
        m_linkedList.PlaceLastNodeAfterRoot();

        Node m_currentNode = m_linkedList.m_root;

        Tile m_previousTile = m_currentNode.m_tile;
        Direction m_previousDirection = m_currentNode.m_direction;

        m_currentNode.UpdateBody(p_newTile, Direction);

        m_currentNode = m_currentNode.m_next;
        m_currentNode.UpdateBody(m_previousTile, m_previousDirection);
        UpdateAllSprites();
    }

    /// <summary>
    /// This function updates all the body parts of the snake so that it look like a real seamless snake.
    /// </summary>
    private void UpdateAllSprites()
    {
        Node m_currentNode = m_linkedList.m_root;
        Direction m_directionAhead;
        ChangeBodySprite(m_currentNode, m_currentNode.m_direction);
        while (m_currentNode.m_next != null)
        {
            m_directionAhead = m_currentNode.m_direction;
            m_currentNode = m_currentNode.m_next;
            ChangeBodySprite(m_currentNode, m_directionAhead);
        }
    }

    private void ChangeBodySprite(Node p_node, Direction p_directionOfBodyInFront)
    {
        Sprite m_sprite = null;
        if (p_node == m_linkedList.m_root)
            m_sprite = m_headSprites[(int)p_directionOfBodyInFront];
        else if (p_node.m_next != null)
        {
            if (p_directionOfBodyInFront == p_node.m_direction)
                m_sprite = m_bodySprites[(int)p_directionOfBodyInFront < 2 ? 0 : 1];
            else
            {
                if (p_directionOfBodyInFront == Direction.Left)
                    m_sprite = m_bodySprites[2 + (p_node.m_direction == Direction.Up ? 1 : 0)];
                else if (p_directionOfBodyInFront == Direction.Right)
                    m_sprite = m_bodySprites[2 + (p_node.m_direction == Direction.Up ? 3 : 2)];
                else if (p_directionOfBodyInFront == Direction.Up)
                    m_sprite = m_bodySprites[2 + (p_node.m_direction == Direction.Left ? 2 : 0)];
                else if (p_directionOfBodyInFront == Direction.Down)
                    m_sprite = m_bodySprites[2 + (p_node.m_direction == Direction.Left ? 3 : 1)];
            }
        }
        else if (p_node.m_next == null)
            m_sprite = m_tailSprites[(int)p_directionOfBodyInFront];
        p_node.ChangeSprite(m_sprite);
    }

    public void ChangeDirection(Direction p_direction)
    {
        if (p_direction != m_oppositeDirection)
            Direction = p_direction;
    }

    /// <summary>
    /// Updating an opposite direction will help us avoid pressing inputs so that the snake turns 180 and into itself.
    /// </summary>
    private void SetOppositeDirection()
    {
        m_oppositeDirection = GetOppositeDirection(Direction);
    }
    private Direction GetOppositeDirection(Direction p_direction)
    {
        int m_opposite = ((int)p_direction % 2 == 0 ? 1 : -1);
        return (p_direction + m_opposite);
    }

    private bool TileIsImpassable(Tile p_tile)
    {
        return p_tile == null || p_tile.m_impassable;
    }

    /// <summary>
    /// The snake falls back to this function if no path could be calculated.
    /// The snake will continue forward and uses this function to sense any walls in up to 2 tiles ahead in its direction.
    /// If a wall is sensed, it will turn either left or right of its direction, depending on which way has the most free space.
    /// </summary>
    private void SteerAwayFromWall()
    {
        bool m_impassableWallInFront = TileIsImpassable(Tile.m_neighbours[(int)Direction]);
        bool m_impassableWallABitFurther = !m_impassableWallInFront && TileIsImpassable(Tile.m_neighbours[(int)Direction].m_neighbours[(int)Direction]);
        if (m_impassableWallInFront || m_impassableWallABitFurther)
        {
            Direction[] m_directions = GetPerpendicularDirections(Direction);

            int m_length = CountTilesUntilWall(Tile, m_directions[0], 1);
            int m_otherLength = CountTilesUntilWall(Tile, m_directions[1], 1);
            if (!m_impassableWallInFront && m_length <= 0 && m_otherLength <= 0)
                return;

            if (m_length > m_otherLength)
                ChangeDirection(m_directions[0]);
            else if (m_length < m_otherLength)
                ChangeDirection(m_directions[1]);
            else
            {
                if (Random.Range(0f, 1f) < .5f) ChangeDirection(m_directions[0]);
                else ChangeDirection(m_directions[1]);
            }
        }
    }

    private int CountTilesUntilWall(Tile p_tile , Direction p_direction, int p_recursiveTimes = 0)
    {
        int m_count = -1;
        Tile m_currentTile = p_tile;
        int m_countFromRecursiveFunctions = 0;

        while (!TileIsImpassable(m_currentTile) || m_currentTile == Tile)
        {
            m_currentTile = m_currentTile.m_neighbours[(int)p_direction];
            if (m_currentTile != null && p_recursiveTimes > 0)
            {
                Direction[] m_directions = GetPerpendicularDirections(p_direction);
                m_countFromRecursiveFunctions += CountTilesUntilWall(m_currentTile, m_directions[0], p_recursiveTimes - 1);
                m_countFromRecursiveFunctions += CountTilesUntilWall(m_currentTile, m_directions[1], p_recursiveTimes - 1);
            }
            m_count++;
        }
        return m_count + m_countFromRecursiveFunctions;
    }
    
    private Direction[] GetPerpendicularDirections(Direction p_direction)
    {
        Direction[] m_perpendicularDirections = new Direction[2];

        if (p_direction == Direction.Left || p_direction == Direction.Right)
        {
            m_perpendicularDirections[0] = Direction.Up;
            m_perpendicularDirections[1] = Direction.Down;
        }
        else if (p_direction == Direction.Up || p_direction == Direction.Down)
        {
            m_perpendicularDirections[0] = Direction.Left;
            m_perpendicularDirections[1] = Direction.Right;
        }

        return m_perpendicularDirections;
    }

    /// <summary>
    /// Using A* search algorithm to find the shortest path from the start tile to the goal tile
    /// </summary>
    /// 
    private List<Tile> ConstructPath(Tile p_startTile, Tile p_goalTile)
    {
        //Initialize open and closed lists
        List<Tile> m_openTiles = new List<Tile>(m_gameDirector.GridArea);
        List<Tile> m_closedTiles = new List<Tile>(m_gameDirector.GridArea);

        //Initialize dictionary to store path within
        Dictionary<Tile, Tile> m_constructedPath = new Dictionary<Tile, Tile>();

        //Set default values
        foreach (Tile m_tile in m_gameDirector.m_grid)
        {
            m_tile.GCost = m_tile.HCost = Mathf.Infinity;
            m_constructedPath[m_tile] = null;
        }

        //Add start tile to it
        m_openTiles.Add(p_startTile);
        p_startTile.GCost = 0;
        p_startTile.HCost = CalculateManhattanDistance(p_startTile, p_goalTile);

        //Search for as long as open tiles is not empty
        while (m_openTiles.Count > 0)
        {
            Tile m_currentTile = m_openTiles[0];// = m_openTiles.RemoveFirst();
            //Find the tile with the least cost
            for (int i = 0; i < m_openTiles.Count - 1; i++)
            {
                if (m_currentTile.FCost > m_openTiles[i].FCost)
                    m_currentTile = m_openTiles[i];
            }

            //If this tile is the goal then we've already found it
            if (m_currentTile == p_goalTile)
                break;

            //Remove the tile with least cost from the open list
            m_openTiles.Remove(m_currentTile);
            m_closedTiles.Add(m_currentTile);


            foreach (Tile m_neighbour in m_currentTile.m_neighbours)
            {
                if (TileIsImpassable(m_neighbour) || m_closedTiles.Contains(m_neighbour))
                    continue;
                if (!m_openTiles.Contains(m_neighbour))
                    m_openTiles.Add(m_neighbour);
                float m_cost = m_currentTile.GCost + CalculateManhattanDistance(m_currentTile, m_neighbour);
                if (m_cost < m_neighbour.GCost)
                {
                    m_neighbour.GCost = m_cost;
                    m_neighbour.HCost = CalculateManhattanDistance(m_neighbour, p_goalTile);
                    m_constructedPath[m_neighbour] = m_currentTile;
                }
            }
        }

        if (m_constructedPath[p_goalTile] == null)
            return null;
        List<Tile> m_currentPath = new List<Tile>();
        Tile m_tempTile = p_goalTile;
        while (m_tempTile != null)
        {
            m_currentPath.Add(m_tempTile);
            m_tempTile = m_constructedPath[m_tempTile];
        }
        m_currentPath.Reverse();
        return m_currentPath;
    }

    float CalculateManhattanDistance(Tile p_start, Tile p_end)
    {
        return (p_start.m_coordinates.x - p_end.m_coordinates.x) + (p_start.m_coordinates.y - p_end.m_coordinates.y);
    }

    public void OnGameOver()
    {
        m_linkedList.RemoveNodesFromTiles();
        Destroy(gameObject);
    }

    void DebugPath()
    {
        if (m_currentPath == null)
        {
            Debug.Log("Failed because I can't debug a path that does not exist");
            return;
        }
        for (int i = 0; i < m_currentPath.Count - 1; i++)
            Debug.DrawLine(m_currentPath[i].m_coordinates, m_currentPath[i + 1].m_coordinates, Color.cyan, m_gameDirector.m_updateRate + 0.025f);
    }

}

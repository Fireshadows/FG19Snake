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
        get { return m_linkedList.m_root.m_data.m_tile; }
        set { m_linkedList.m_root.m_data.m_tile = value; }
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
        AddHead();
        m_linkedList.m_root.m_data.AssignNewValues(p_tile, Direction.Right);
        //Tile = p_tile;
        //ChangeDirection(Direction.Right);
        //m_linkedList.m_root.m_data.Place();

        AddBody();
        AddBody();
    }
    private void AddHead()
    {
        NodeData m_head = Instantiate(m_snakeBody.gameObject, transform).GetComponent<NodeData>();

        m_head.GetComponent<SpriteRenderer>().sprite = m_headSprite;
        m_linkedList.PushFront(m_linkedList, m_head);
    }
    public void AddBody()
    {
        NodeData m_body = Instantiate(m_snakeBody.gameObject, transform).GetComponent<NodeData>();

        m_body.GetComponent<SpriteRenderer>().sprite = m_tailSprite;

        Node m_lastNode = m_linkedList.GetLastNode(m_linkedList);

        m_body.AssignNewValues(
            m_lastNode.m_data.m_tile.m_neighbours[(int)GetOppositeDirection(m_lastNode.m_data.m_direction)],
            m_lastNode.m_data.m_direction);
        //m_body.transform.position = m_body.m_tile.m_coordinates;

        m_linkedList.PushBack(m_linkedList, m_body.GetComponent<NodeData>());
        m_body.Place();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            m_autoPilot = !m_autoPilot;

        if (m_autoPilot)
            return;

        if (Input.GetKey(KeyCode.LeftArrow) && Direction != Direction.Left)
            ChangeDirection(Direction.Left);
        else if (Input.GetKey(KeyCode.UpArrow) && Direction != Direction.Up)
            ChangeDirection(Direction.Up);
        else if (Input.GetKey(KeyCode.RightArrow) && Direction != Direction.Right)
            ChangeDirection(Direction.Right);
        else if (Input.GetKey(KeyCode.DownArrow) && Direction != Direction.Down)
            ChangeDirection(Direction.Down);
    }
    
    public void OnUpdate()
    {
        if (m_autoPilot)
        {
            SteerAwayFromWall();
            //Tile m_tileBehindTail = m_linkedList.GetLastNode(m_linkedList).m_data.m_tile.m_neighbours[(int)GetOppositeDirection(m_linkedList.GetLastNode(m_linkedList).m_data.m_direction)];
            if (/*!TileIsImpassable(m_tileBehindTail) && ConstructPath(Tile, m_tileBehindTail) != null 
                && */m_gameDirector.m_appleTile.m_neighbours.Count(m_tile => TileIsImpassable(m_tile)) < 3/*m_currentPath == null*/)
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

        MoveAllbodies(m_newTile);

        if (Tile.m_hasApple)
        {
            m_gameDirector.OnAppleGrappled();
            AddBody();
        }

        SetOppositeDirection();
    }

    private void MoveAllbodies(Tile p_newTile)
    {
        Node m_currentNode;
        NodeData m_previousNodeData = new NodeData();
        NodeData m_temporaryData = new NodeData();

        m_currentNode = m_linkedList.m_root;

        m_previousNodeData.AssignNewValues(m_currentNode.m_data.m_tile, m_currentNode.m_data.m_direction);
        m_currentNode.m_data.AssignNewValues(p_newTile, Direction);

        while(m_currentNode.m_next != null)
        {
            //Assign new current node
            m_currentNode = m_currentNode.m_next;

            //Save values of the new node
            m_temporaryData.AssignNewValues(m_currentNode.m_data.m_tile, m_currentNode.m_data.m_direction);

            //Assign the body of the node to the new values
            m_currentNode.m_data.AssignNewValues(m_previousNodeData.m_nextTile, m_previousNodeData.m_direction);

            //Get saved coordinates to assign next node with
            m_previousNodeData.AssignNewValues(m_temporaryData.m_nextTile, m_temporaryData.m_direction);
        }
        /*Direction m_directionAhead;
        m_currentNode = m_linkedList.m_root;
        ChangeBodySprite(m_currentNode, m_currentNode.m_data.m_direction);
        while (m_currentNode.m_next != null)
        {
            m_directionAhead = m_currentNode.m_data.m_direction;
            m_currentNode = m_currentNode.m_next;
            //ChangeDirection to a correct sprite here, using previousnodedata.direction and currentnodes direction
            ChangeBodySprite(m_currentNode, m_directionAhead);
            //m_currentNode.m_data.ChangeSprite();
        }*/
    }

    private void ChangeBodySprite(Node p_node, Direction p_aheadDirection)
    {
        Sprite m_sprite = null;
        if (p_node == m_linkedList.m_root)
        {
            m_sprite = m_headSprites[(int)p_aheadDirection];
        }
        else if (p_node.m_next != null)
        {
            if (p_aheadDirection == p_node.m_next.m_data.m_direction)
                m_sprite = m_bodySprites[(int)p_aheadDirection < 2 ? 0 : 1];
            else
            {
                if (p_aheadDirection == Direction.Left)
                    m_sprite = m_bodySprites[2 + (int)(p_node.m_next.m_data.m_direction == Direction.Up ? Direction.Down : Direction.Up)];
                else if (p_aheadDirection == Direction.Right)
                    m_sprite = m_bodySprites[2 + (int)(p_node.m_next.m_data.m_direction == Direction.Up ? Direction.Down : Direction.Up)];
                else if (p_aheadDirection == Direction.Up)
                    m_sprite = m_bodySprites[2 + (int)(p_node.m_next.m_data.m_direction == Direction.Left ? Direction.Left : Direction.Right)];
                else if (p_aheadDirection == Direction.Down)
                    m_sprite = m_bodySprites[2 + (int)(p_node.m_next.m_data.m_direction == Direction.Left ? Direction.Left : Direction.Right)];

                //m_sprite = m_bodySprites[(int)p_aheadDirection];
            }
        }
        else if (p_node.m_next == null)
        {
            m_sprite = m_tailSprites[(int)p_aheadDirection];
        }
        p_node.m_data.ChangeSprite(m_sprite);
    }

    public void ChangeDirection(Direction p_direction)
    {
        if (p_direction != m_oppositeDirection)
            Direction = p_direction;
    }

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

    private void SteerAwayFromWall()
    {
        bool m_impassableWallInFront = TileIsImpassable(Tile.m_neighbours[(int)Direction]);
        bool m_impassableWallABitFurther = !m_impassableWallInFront && TileIsImpassable(Tile.m_neighbours[(int)Direction].m_neighbours[(int)Direction]);
        if (m_impassableWallInFront || m_impassableWallABitFurther)
        {
            int m_length = 0, m_otherLength = 0;

            Direction[] m_directions = GetPerpendicularDirections(Direction);

            m_length = CountTilesUntilWall(Tile, m_directions[0], 1);
            m_otherLength = CountTilesUntilWall(Tile, m_directions[1], 1);
            //Debug.Log(m_length + " " + m_otherLength);
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
        if (!m_autoPilot && p_recursiveTimes > 0)
            Debug.Log("");
        int m_count = -1;
        Tile m_currentTile = p_tile;
        int m_countFromRecursiveFunctions = 0;

        while (m_count < 10 && !TileIsImpassable(m_currentTile) || m_currentTile == Tile)
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

    private List<Tile> ConstructPath(Tile p_startTile, Tile p_goalTile)
    {
        //Initialize open and closed lists
        List<Tile> m_openTiles = new List<Tile>();
        List<Tile> m_closedTiles = new List<Tile>();
        Debug.Log(p_goalTile);
        Dictionary<Tile, Tile> m_constructedPath = new Dictionary<Tile, Tile>();

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
            Tile m_currentTile = m_openTiles[0];
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
                if (m_neighbour == null || m_neighbour.m_impassable || m_closedTiles.Contains(m_neighbour))
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

    void DebugPath()
    {
        if (m_currentPath == null)
        {
            Debug.Log("Failed debugging line because path does not exist");
            return;
        }
        for (int i = 0; i < m_currentPath.Count - 1; i++)
        {
            Debug.DrawLine(m_currentPath[i].m_coordinates, m_currentPath[i + 1].m_coordinates, Color.cyan, m_gameDirector.m_updateRate + 0.015f);
        }
    }

}

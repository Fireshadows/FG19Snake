using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSnake : MonoBehaviour
{
    public Sprite m_headSprite;
    public Sprite m_tailSprite;

    public GameObject m_snakeBody;
    LinkedList m_linkedList;

    Direction Direction { get; set; }
    Direction m_oppositeDirection;

    Tile Tile {
        get { return m_linkedList.m_root.m_data.m_tile; }
        set { m_linkedList.m_root.m_data.m_tile = value; }
    }

    GameDirector m_gameDirector;

    public void Initialize(Tile p_tile, GameDirector p_gameDirector)
    {
        m_gameDirector = p_gameDirector;

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
        if (Input.GetKey(KeyCode.LeftArrow) && Direction != Direction.Left)
            ChangeDirection(Direction.Left);
        else if (Input.GetKey(KeyCode.UpArrow) && Direction != Direction.Up)
            ChangeDirection(Direction.Up);
        else if (Input.GetKey(KeyCode.RightArrow) && Direction != Direction.Right)
            ChangeDirection(Direction.Right);
        else if (Input.GetKey(KeyCode.DownArrow) && Direction != Direction.Down)
            ChangeDirection(Direction.Down);
    }
    
    public void Move()
    {
        Tile m_newTile = Tile.m_neighbours[(int)Direction];

        if (m_newTile == null || m_newTile.m_impassable)
            return;

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
        Node m_temporaryNode;
        NodeData m_previousNodeData = new NodeData();
        NodeData m_temporaryData = new NodeData();

        m_temporaryNode = m_linkedList.m_root;

        m_previousNodeData.AssignNewValues(m_temporaryNode.m_data.m_tile, m_temporaryNode.m_data.m_direction);
        m_temporaryNode.m_data.AssignNewValues(p_newTile, Direction);

        while(m_temporaryNode.m_next != null)
        {
            //Assign new temporary node
            m_temporaryNode = m_temporaryNode.m_next;

            //Save values of the new node
            m_temporaryData.AssignNewValues(m_temporaryNode.m_data.m_tile, m_temporaryNode.m_data.m_direction);

            //Assign the body of the node to the new values
            m_temporaryNode.m_data.AssignNewValues(m_previousNodeData.m_nextTile, m_previousNodeData.m_direction);

            //Get saved coordinates to assign next node with
            m_previousNodeData.AssignNewValues(m_temporaryData.m_nextTile, m_temporaryData.m_direction);
        }
    }

    public void ChangeDirection(Direction p_direction)
    {
        if (p_direction != m_oppositeDirection)
            Direction = p_direction;
        //Debug.Log(m_direction + " " + m_oppositeDirection);
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
}

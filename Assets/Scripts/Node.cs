using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public NodeData m_data;
    public Node m_next;

    public Node(NodeData p_data)
    {
        m_data = p_data;
        m_data.m_node = this;
        m_next = null;
    }
}

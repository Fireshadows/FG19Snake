using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class LinkedList
{
    public Node m_root;

    public void PushFront(Node p_value)
    {
        Node m_newNode = p_value;
        m_newNode.m_next = m_root;
        m_root = m_newNode;
    }

    public void PushBack(Node p_value)
    {
        Node m_newNode = p_value;
        if (m_root == null)
        {
            m_root = m_newNode;
            return;
        }
        Node m_lastNode = GetLastNode();
        m_lastNode.m_next = m_newNode;
    }

    public Node GetLastNode()
    {
        Node m_temp = m_root;
        while (m_temp.m_next != null)
            m_temp = m_temp.m_next;
        return m_temp;
    }
    public Node GetPenultimateNode()
    {
        Node m_temp = m_root;
        while (m_temp.m_next != null && m_temp.m_next.m_next != null)
            m_temp = m_temp.m_next;
        return m_temp;
    }
    public void PlaceLastNodeAfterRoot()
    {
        Node m_node = GetLastNode();
        GetPenultimateNode().m_next = null;
        m_node.m_next = m_root.m_next;
        m_root.m_next = m_node;
    }

    public void RemoveNodesFromTiles()
    {
        Node m_temp = m_root;
        m_temp.RemoveTile();
        while (m_temp.m_next != null)
        {
            m_temp = m_temp.m_next;
            m_temp.RemoveTile();
        }
    }

    internal void DebugValues()
    {
        Node m_temp = m_root;
        Debug.Log(m_temp);
        while (m_temp.m_next != null)
        {
            m_temp = m_temp.m_next;
            Debug.Log(m_temp);
        }
    }
}
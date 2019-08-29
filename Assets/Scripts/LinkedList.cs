using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class LinkedList
{
    public Node m_root;

    public void PushFront(LinkedList p_linkedList, NodeData p_value)
    {
        Node m_newNode = new Node(p_value);
        m_newNode.m_next = p_linkedList.m_root;
        p_linkedList.m_root = m_newNode;
    }

    public void PushBack(LinkedList p_linkedList, NodeData p_value)
    {
        Node m_newNode = new Node(p_value);
        if (p_linkedList.m_root == null)
        {
            p_linkedList.m_root = m_newNode;
            return;
        }
        Node m_lastNode = GetLastNode(p_linkedList);
        m_lastNode.m_next = m_newNode;
    }

    public Node GetLastNode(LinkedList p_linkedList)
    {
        Node m_temp = p_linkedList.m_root;
        while (m_temp.m_next != null)
            m_temp = m_temp.m_next;
        return m_temp;
    }

    internal void DebugValues()
    {
        Node m_temp = m_root;
        Debug.Log(m_temp.m_data);
        while (m_temp.m_next != null)
        {
            m_temp = m_temp.m_next;
            Debug.Log(m_temp.m_data);
        }
    }
}
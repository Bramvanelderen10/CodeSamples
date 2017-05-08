using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A simple generic binary tree implementation with binary search
/// </summary>
/// <typeparam name="T"></typeparam>
public class BTree<T> where T : IComparable
{
    private BNode _start;
    private int _count = 0;

    public int Count
    {
        get { return _count; }
    }

    public bool Contains(T item)
    {
        return (GetNode(item) != null);
    }

    public T Min()
    {
        if (_start == null)
            throw new Exception("No content in tree");

        var current = _start;
        while (true)
        {
            if (current.Left == null)
                break;
            current = current.Left;
        }

        return current.Value;
    }

    public T Max()
    {
        if (_start == null)
            throw new Exception("No content in tree");

        var current = _start;
        while (true)
        {
            if (current.Right == null)
                break;
            current = current.Right;
        }

        return current.Value;
    }

    public void Add(T value)
    {
        _count++;
        var node = new BNode(value);

        if (_start == null)
            _start = node;
        Add(node, _start);
    }

    private void Add(BNode value, BNode start)
    {
        bool placed = false;
        var current = start;
        while (!placed)
        {
            int compare = value.Value.CompareTo(current.Value);

            if (compare == 0)
            {
                current.Value = value.Value;
                placed = true;

            } else if (compare == 1)
            {
                if (current.Right == null)
                {
                    current.Right = value;
                    value.Parent = current;
                    placed = true;
                }
                else
                {
                    current = current.Right;
                }
            } else if (compare == -1)
            {
                if (current.Left == null)
                {
                    current.Left = value;
                    value.Parent = current;
                    placed = true;
                }
                else
                {
                    current = current.Left;
                }
            }
        }
    }

    private BNode GetNode(T item)
    {
        if (_start == null)
            return null;

        if (_start.Value.CompareTo(item) == 0)
            return _start;

        BNode current = _start;
        while (true)
        {
            if (current == null)
                return null;
            int compare = current.Value.CompareTo(item);
            if (compare == 0)
                return current;
            if (compare == -1) //-1 means the item is bigger as current
                current = current.Right;
            if (compare == 1)
                current = current.Left;
        }
    }

    public void Remove(T item)
    {
        var node = GetNode(item);
        if (node == null)
            return;
        _count--;
        var left = node.Left;
        var right = node.Right;
        var parent = node.Parent;

        BNode replacementNode = null;
        BNode moveNode = null;
        if (left != null)
        {
            replacementNode = left;
            moveNode = right;
        } else if (right != null)
        {
            replacementNode = right;
        }

        if (parent != null)
        {
            if (parent.Left != null && parent.Left == node)
                parent.Left = replacementNode;
            else if (parent.Left != null && parent.Right == node)
                parent.Right = replacementNode;
        } else if (_start.Value.CompareTo(item) == 0)
            _start = replacementNode;
        
        if (moveNode != null)
            Add(moveNode, _start);
    }


    private class BNode
    {
        public BNode Parent;
        public BNode Left;
        public BNode Right;
        public T Value;

        public BNode(T value)
        {
            Value = value;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A simple generic binary tree implementation with binary search
/// </summary>
/// <typeparam name="T"></typeparam>
public class BTree<T> where T : IComparable<T>
{
    private BNode _start;
    private int _count = 0;

    public int Count
    {
        get { return _count; }
    }

    /// <summary>
    /// Retrieves node based on item, if result exists the tree contains the item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
        return (GetNode(item) != null);
    }

    /// <summary>
    /// Retrieves the item with the lowest comparable value
    /// </summary>
    /// <returns></returns>
    public T Min()
    {
        if (_start == null)
            throw new Exception("No content in tree");

        return Min(_start).Value;
    }

    /// <summary>
    /// Retrieves the item with the highest comparable value
    /// </summary>
    /// <returns></returns>
    public T Max()
    {
        if (_start == null)
            throw new Exception("No content in tree");

        return Max(_start).Value;
    }

    private BNode Min(BNode start)
    {
        var current = start;
        while (true)
        {
            if (current.Left == null)
                break;
            current = current.Left;
        }

        return current;
    }

    private BNode Max(BNode start)
    {
        var current = start;
        while (true)
        {
            if (current.Right == null)
                break;
            current = current.Right;
        }

        return current;
    }

    /// <summary>
    /// Create a node with the given value as item then insert the node into the tree
    /// </summary>
    /// <param name="value"></param>
    public void Add(T value)
    {
        _count++;
        var node = new BNode(value);

        if (_start == null)
            _start = node;
        else
            Add(node, _start);
    }

    /// <summary>
    /// Add a node to the tree
    /// </summary>
    /// <param name="value"></param>
    /// <param name="start"></param>
    private void Add(BNode value, BNode start)
    {
        if (value == null)
            return;

        if (value == start)
            return;

        if (start == null)
            throw new ArgumentException("Start must have a value!");

        int compare = value.Value.CompareTo(start.Value);
        if (compare == 0)
            return;
        else if (compare == 1)
        {
            if (start.Right == null)
            {
                start.Right = value;
                value.Parent = start;
            }
            else
            {
                Add(value, start.Right);
            }

        } else if (compare == -1)
        {
            if (start.Left == null)
            {
                start.Left = value;
                value.Parent = start;
            }
            else
            {
                Add(value, start.Left);
            }
        }
    }

    /// <summary>
    /// Retrieve the node that contains this item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Removes the given item from the binary tree
    /// </summary>
    /// <param name="item"></param>
    public void Remove(T item)
    {
        var node = GetNode(item);
        if (node == null)
            return;

        _count--;

        //Acquire relevant parents and children
        var left = node.Left;
        var right = node.Right;
        var parent = node.Parent;

        //If parent null replace node with the highest value of the left tree
        //If there is no left replace with min value or right tree
        if (parent == null)
        {
            if (left != null)
            {
                _start = Max(left);
                _start.Parent = null;
                Add(right, _start);
            }
            else if (right != null)
            {
                _start = Min(right);
                _start.Parent = null;
                Add(left, _start);
            }
        }
        else
        {
            if (parent.Left == node)
            {
                if (left != null)
                {
                    parent.Left = Max(left);
                    parent.Left.Parent = parent;
                    Add(right, parent.Left);
                }
                else if (right != null)
                {
                    parent.Left = Min(right);
                    parent.Left.Parent = parent;
                    Add(left, parent.Left);
                }
                
            } else if (parent.Right == node)
            {
                if (right != null)
                {
                    parent.Right = Min(right);
                    parent.Right.Parent = parent;
                    Add(left, parent.Right);
                }
                else if (left != null)
                {
                    parent.Right = Max(left);
                    parent.Right.Parent = parent;
                    Add(right, parent.Right);
                }
            }
        }
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
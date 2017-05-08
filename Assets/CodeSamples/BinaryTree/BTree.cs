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

        var current = _start;
        while (true)
        {
            if (current.Left == null)
                break;
            current = current.Left;
        }

        return current.Value;
    }

    /// <summary>
    /// Retrieves the item with the highest comparable value
    /// </summary>
    /// <returns></returns>
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
        if (value == start)
            return;

        bool placed = false;
        var current = start;
        while (!placed)
        {
            int compare = value.Value.CompareTo(current.Value);
            if (compare == 0)
            {
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

        //Determine which node should replace the current node and which node should move to a new position
        BNode replacementNode = null;
        BNode moveNode = null;
        if (left != null)
        {
            replacementNode = left;
            moveNode = right;
        }
        else
        {
            replacementNode = right;
        }

        if (parent != null)
        {
            //Find and replace the node in the parent child
            if (parent.Left != null && parent.Left == node) 
            {
                parent.Left = replacementNode;
            }
            else if (parent.Left != null && parent.Right == node)
            {
                parent.Right = replacementNode;
            }
        }
        else
        {
            _start = replacementNode; // if there wasn't a parent the replacement node is the new start
        }
        if (replacementNode != null)
            replacementNode.Parent = parent;
        if (moveNode != null)
            Add(moveNode, _start); //The determined node to move to a new position gets moved normally to the new position
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
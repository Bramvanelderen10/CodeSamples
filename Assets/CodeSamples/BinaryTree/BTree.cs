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

    public int Count
    {
        get
        {
            if (_start == null)
                return 0;

            return CountRecursive(_start);
        }
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

        //Acquire relevant parents and children
        var left = node.Left;
        var right = node.Right;
        var parent = node.Parent;
        if (left == null && right == null) //No child nodes so we can simply remove the node
        {
            if (parent == null)
            {
                _start = null;
            } else if (parent.Left == node)
            {
                parent.Left = null;
            }
            else
            {
                parent.Right = null;
            }
        } else if (left == null || right == null)
        {
            var child = (left != null) ? left : right;
            child.Parent = parent;

            if (parent == null)
            {
                _start = child;
            } else if (parent.Left == node)
            {
                parent.Left = child;
            }
            else
            {
                parent.Right = child;
            }
        } else if (left != null && right != null)
        {
            BNode replacementNode = null;
            if (parent == null || parent.Left == node)
            {
                replacementNode = Max(left);
                if (replacementNode == left)
                {
                    left = replacementNode.Left;

                } else if (replacementNode.Left != null)
                {
                    replacementNode.Parent.Right = replacementNode.Left;
                    replacementNode.Left.Parent = replacementNode.Parent;
                }
                else
                {
                    replacementNode.Parent.Right = null;
                }

                if (parent == null)
                {
                    replacementNode.Parent = null; //Replacement nodes parent is the parent of old node
                    _start = replacementNode;
                }
                else
                {
                    replacementNode.Parent = parent; //Replacement nodes parent is the parent of old node
                    parent.Left = replacementNode;
                }
                
            } else
            {
                replacementNode = Min(right);
                if (replacementNode == right)
                {
                    right = replacementNode.Right;
                } else if (replacementNode.Right != null)
                {
                    replacementNode.Parent.Left = replacementNode.Right;
                    replacementNode.Right.Parent = replacementNode.Parent;
                }
                else
                {
                    replacementNode.Parent.Left = null;
                }
                replacementNode.Parent = parent; //Replacement nodes parent is the parent of old node
                parent.Right = replacementNode;
            }
            replacementNode.Left = left;
            replacementNode.Right = right;

            if (left != null)
                left.Parent = replacementNode;
            if (right != null)
                right.Parent = replacementNode;
        }
    }

    private int CountRecursive(BNode root)
    {
        int count = 1;
        if (root.Left != null)
            count += CountRecursive(root.Left);
        if (root.Right != null)
            count += CountRecursive(root.Right);

        return count;
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
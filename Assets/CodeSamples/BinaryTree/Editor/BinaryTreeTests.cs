using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System;

public class BinaryTreeTests {
    
    [Test]
    public void BinaryTreeMinTest()
    {
        BTree<int> tree = new BTree<int>();
        tree.Add(5);
        tree.Add(3);
        tree.Add(4);
        tree.Add(1);
        tree.Add(2);
        tree.Add(3);
        tree.Add(10);
        tree.Add(9);
        Assert.AreEqual(1, tree.Min());
    }

    [Test]
    public void BinaryTreeMaxTest()
    {
        BTree<int> tree = new BTree<int>();
        tree.Add(5);
        tree.Add(3);
        tree.Add(4);
        tree.Add(1);
        tree.Add(2);
        tree.Add(3);
        tree.Add(10);
        tree.Add(9);
        Assert.AreEqual(10, tree.Max());
    }

    [Test]
    public void BinaryTreeRemoveTest()
    {
        BTree<int> tree = new BTree<int>();
        tree.Add(5);
        tree.Add(3);
        tree.Add(4);
        tree.Add(1);
        tree.Add(2);
        //tree.Add(3);
        tree.Add(10);
        tree.Add(9);
        tree.Remove(3);
        Assert.AreEqual(false, tree.Contains(3));
    }

    [Test]
    public void BinaryTreeSortedOnRemoveTest()
    {
        BTree<int> tree = new BTree<int>();
        tree.Add(5);
        tree.Add(3);
        tree.Add(4);
        tree.Add(1);
        tree.Add(2);
        tree.Add(10);
        tree.Add(9);
        tree.Remove(3);
        tree.Remove(5);

        var message = "";
        var result = true;
        if (tree.Min() != 1)
        {
            result = false;
            message += "Min value is supposed to be 1 but is " + tree.Min() + ";";
        }
        if (tree.Contains(3))
        {
            result = false;
            message += "tree contains 3 that is supposed to be removed;";
        }
        if (tree.Contains(5))
        {
            result = false;
            message += "tree contains 5 that is supposed to be removed;";
        }

        Assert.AreEqual(true, result, message);
    }

    [Test]
    public void BinaryTreeCountTest()
    {
        var tree = new BTree<float>();
        int count = 14;
        for (int i = 0; i < count; i++)
        {
            tree.Add(i);
        }

        Assert.AreEqual(count, tree.Count, "Tree count invalid");
    }

    [Test]
    public void BinaryTreeCustomClassTest()
    {
        var obj1 = new TestClass("Object1", 10);
        var obj2 = new TestClass("obj2", 7);
        var obj3 = new TestClass("obj3", 6);
        var obj4 = new TestClass("obj4", 5);
        var obj5 = new TestClass("obj5", 8);
        var obj6 = new TestClass("obj6", 2);
        var obj7 = new TestClass("obj7", 7);
        var obj8 = new TestClass("obj8", 3);

        var tree = new BTree<TestClass>();
        tree.Add(obj1);
        tree.Add(obj2);
        tree.Add(obj3);
        tree.Add(obj4);
        tree.Add(obj5);
        tree.Add(obj6);
        tree.Add(obj7);
        tree.Add(obj8);
        tree.Remove(obj2);
        tree.Remove(obj1);

        var message = "";
        var result = true;
        var min = tree.Min();
        if (tree.Min() != obj6)
        {
            result = false;
            message += "Min value is supposed to be obj6 but is " + tree.Min() + ";";
        }

        if (tree.Max() != obj5)
        {
            message += "Max value is supposed to be obj5 but is " + tree.Min() + ";";
            result = false;
        }

        if (!tree.Contains(obj7))
        {
            result = false;
            message += "tree is supposed to contain obj7 but does not;";
        }

        if (tree.Contains(obj2))
        {
            result = false;
            message += "tree is not supposed to contain obj2 but does;";
        }

        Assert.AreEqual(true, result, message);
    }

    private class TestClass : IComparable<TestClass>
    {
        public string Name;
        public int Value;

        public TestClass(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public int CompareTo(TestClass other)
        {
            if (this == other)
                return 0;

            var compared = this.Value.CompareTo(other.Value);
            if (compared == 0) //allow duplicates
                compared = 1;

            return compared;
        }
    }
}

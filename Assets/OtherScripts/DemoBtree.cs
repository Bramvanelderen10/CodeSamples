using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class DemoBtree : MonoBehaviour
{
    void Start()
    {
        var test1 = new test("1", 10);
        var test2 = new test("2", 2);
        var test3 = new test("1", 3);
        var test4 = new test("1", 4);
        var test5 = new test("1", 6);
        var test6 = new test("1", 7);
        var test7 = new test("1", 5);

        var tree = new BTree<test>();
        tree.Add(test3);
        tree.Add(test1);
        tree.Add(test2);
        tree.Add(test4);
        tree.Add(test5);
        tree.Add(test6);
        tree.Add(test7);

        tree.Remove(test3);
        print(tree.Contains(test3));

        tree.Min().OutputTestString();
        tree.Max().OutputTestString();
    }



    public class test : IComparable
    {
        private string value;
        private float compareValue;

        public test(string text, float index)
        {
            value = text;
            compareValue = index;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();
            test other = obj as test;
            if (other == null)
                throw new ArgumentException("Object is not a test");

            return this.compareValue.CompareTo(other.compareValue);
        }

        public void OutputTestString()
        {
            print(value + " real value: " + compareValue);
        }
    }
}

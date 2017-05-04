using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DemoAstar))]
public class DemoAstarEditor : Editor
{
    private int _rows = 10;
    private int _columns = 10;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        var script = (DemoAstar)target;
        _rows = EditorGUILayout.IntField("Rows", _rows);
        _columns = EditorGUILayout.IntField("Columns", _columns);
        if (GUILayout.Button("Update Grid"))
        {
            script.GenerateGrid(_rows, _columns);
        }

        if (!script.TargetBased)
        {
            if (GUILayout.Button("Switch to target based"))
            {
                script.TargetBased = true;
            }
            var start = EditorGUILayout.Vector3Field("Start of path", script.StartPosition);
            var end = EditorGUILayout.Vector3Field("End of path", script.TargetPosition);
            if (GUILayout.Button("Update start and target positions"))
            {
                script.StartPosition = start;
                script.TargetPosition = end;
            }
        }
        else
        {
            if (GUILayout.Button("Switch to position based"))
            {
                script.TargetBased = false;
            }
            script.StartObj = (GameObject)EditorGUILayout.ObjectField(script.StartObj, typeof(GameObject), true);
            script.EndObj = (GameObject)EditorGUILayout.ObjectField(script.EndObj, typeof(GameObject), true);
        }
        

        if (script.ContinuesPathDemo)
        {
            if (GUILayout.Button("Disable continues pathing"))
            {
                script.ContinuesPathDemo = false;
            }
        }
        else
        {
            if (GUILayout.Button("Find path"))
                script.GeneratePath();
            if (GUILayout.Button("Display path"))
                script.DisplayPath();

            if (GUILayout.Button("Enable continues pathing"))
            {
                script.ContinuesPathDemo = true;
                script.GeneratePath();
            }
        }
        if(GUILayout.Button("Clear visual path"))
            script.CleanUp();
    }
}

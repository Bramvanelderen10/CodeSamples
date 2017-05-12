using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DemoAstar))]
public class DemoAstarEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        var script = (DemoAstar)target;
        script._rows = EditorGUILayout.IntField("Rows", script._rows);
        script._columns = EditorGUILayout.IntField("Columns", script._columns);
        script._height = EditorGUILayout.IntField("Height", script._height);
        if (GUILayout.Button("Update Grid"))
        {
            script.GenerateGrid();
        }

        if (!script.TargetBased)
        {
            if (GUILayout.Button("Switch to target based"))
            {
                script.TargetBased = true;
            }
            script.StartPosition = EditorGUILayout.Vector3Field("Start of path", script.StartPosition);
            script.TargetPosition = EditorGUILayout.Vector3Field("End of path", script.TargetPosition);
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

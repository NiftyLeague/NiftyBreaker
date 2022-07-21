using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StageMaker))]
public class StageMakerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        StageMaker stageMaker = (StageMaker)target;
        if (GUILayout.Button("Create Stage"))
        {
            stageMaker.CreateStage();
        }
    }
}

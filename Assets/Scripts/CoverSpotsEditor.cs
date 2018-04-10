using UnityEngine;
using System.Collections;
//using UnityEditor;

//[CustomEditor(typeof(FindCoverSpots))]
//public class CoverSpotsEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        FindCoverSpots myScript = (FindCoverSpots)target;
//        if (GUILayout.Button("Clear Data"))
//        {
//            myScript.ClearData();
//            SceneView.RepaintAll();
//        }
//        if (GUILayout.Button("Find Navmesh Edges"))
//        {
//            myScript.FindBorderEdges();
//            SceneView.RepaintAll();
//        }
//        if (GUILayout.Button("Find CoverSpots"))
//        {
//            myScript.CreateCoverSpotsEditor();
//            SceneView.RepaintAll();
//        }
//        if (GUILayout.Button("CleanUp CoverSpots"))
//        { 
//            myScript.CleanCoverSpotsEditor();
//            SceneView.RepaintAll();
//        }
//    }
//}
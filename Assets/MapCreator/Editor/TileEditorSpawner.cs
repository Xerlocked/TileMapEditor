using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands.BranchExplorer.Layout;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(TileSpawner))]
public class TileEditorSpawner : Editor
{
    private TileSpawner Spawner => (TileSpawner) target;
    public GameObject selectObject = null;
    private bool lockMouse = false;
    
    private enum ToolModes
    {
        Move,
        Building,
        Drawing
    }

    private ToolModes toolMode = ToolModes.Move;
    
    protected void OnSceneGUI()
    {
        Event e = Event.current;

        Handles.BeginGUI();
        {
            toolMode = (ToolModes)GUI.Toolbar(new Rect(10, 10, 200, 30), (int)toolMode, new[] {"Move", "Building", "Drawing"});

            if (toolMode == ToolModes.Building)
                GUI.Window(0, new Rect(10, 70, 200, 400), BuildingPanel, "Buildings");
        }
        Handles.EndGUI();

        if (toolMode == ToolModes.Move)
        {
            if (Tools.current == Tool.None)
            {
                Tools.current = Tool.Move;
            }
            return;
        }

        Tools.current = Tool.None;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.V)
        {
            lockMouse = !lockMouse;
        }
        
        if (e.type == EventType.MouseDown && e.button == 0 && toolMode == ToolModes.Building && !lockMouse)
        {
            if (!Select<Transform>(e))
            {
                Debug.Log("실행");
                Selection.activeGameObject = Spawner.transform.gameObject;
            }

            selectObject = Selection.activeGameObject;
            //activeTransform1.transform.localPosition = selectObject.transform.localPosition;
        }

        Selection.activeGameObject = Spawner.transform.gameObject;
    }

    private void BuildingPanel(int id)
    {
        if (selectObject == null)
        {
            return;
        }

        const int windowWidth = 200;
        const int squareSize = 75;
        var e = Event.current;
        
        EditorGUILayout.BeginVertical(new GUIStyle(
                EditorStyles.helpBox
                )
        {
            margin = new RectOffset(0,0,110,0)
        } );

        Texture2D activeThumbnail = AssetPreview.GetAssetPreview(selectObject);
        GUI.DrawTexture(new Rect(windowWidth / 2 - squareSize/2, 25, squareSize,squareSize), activeThumbnail);

        Helpers.TransformData activeTransformData = new Helpers.TransformData(selectObject.transform);
        
        EditorGUI.BeginChangeCheck();
        activeTransformData.localPosition = EditorGUILayout.Vector3Field("Local Position", activeTransformData.localPosition);
        activeTransformData.localRotation = EditorGUILayout.Vector3Field("Local Rotation", activeTransformData.localRotation);
        activeTransformData.localScale = EditorGUILayout.Vector3Field("Local Scale", activeTransformData.localScale);
        
        if (EditorGUI.EndChangeCheck())
        {
            activeTransformData.ApplyTransform(selectObject.transform);
        }

        EditorGUILayout.Space();
 
        EditorGUI.LabelField(new Rect(110, 355, 150, 45), $"Lock: {lockMouse}", new GUIStyle(
            EditorStyles.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        });
        
        EditorGUILayout.EndVertical();
        if (e.type == EventType.MouseMove || e.type == EventType.MouseDown)
            Repaint();
    }

    private static GameObject Select<T>(Event e) where T : UnityEngine.Component
    {
        Camera cam = Camera.current;

        if (cam != null)
        {
            RaycastHit hit;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != null)
                {
                    GameObject gameObj = hit.collider.gameObject;
                    if (gameObj.GetComponent<T>() != null)
                    {
                        e.Use();
                        Selection.activeGameObject = gameObj;
                        return gameObj;
                    }
                }
            }
        }
        return null;
    }
}

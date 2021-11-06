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

    public enum ToolModes
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
            Debug.Log("Lock State: " + lockMouse);
        }
        
        if (e.type == EventType.MouseDown && e.button == 0 && toolMode == ToolModes.Building && !lockMouse)
        {
            if (!Select<Transform>(e))
            {
                Debug.Log("실행");
                Selection.activeGameObject = Spawner.transform.gameObject;
            }

            selectObject = Selection.activeGameObject;
        }

        Selection.activeGameObject = Spawner.transform.gameObject;
    }

    private void BuildingPanel(int id)
    {
        const int left = 10;
        const int width = 200;
        const int height = 400;
        var e = Event.current;
        
        Vector3 activeLocalPosition = selectObject.transform.localPosition;
        Quaternion activeLocalRotation = selectObject.transform.localRotation;
        Vector3 activeLocalScale =  selectObject.transform.localScale;
        Texture2D activeThumbnail = AssetPreview.GetAssetPreview(selectObject);
        
        GUI.DrawTexture(new Rect(width/2 - 75/2, 25, 75,75), activeThumbnail);
        
        EditorGUILayout.BeginVertical(new GUIStyle(
            EditorStyles.helpBox
            ) {margin = new RectOffset(0,0,110,0)} );
        activeLocalPosition = EditorGUILayout.Vector3Field("Local Position", activeLocalPosition);
        Vector3 transRotation = EditorGUILayout.Vector3Field("Local Rotation", activeLocalRotation.eulerAngles);
        activeLocalScale = EditorGUILayout.Vector3Field("Local Scale", activeLocalScale);

        EditorGUILayout.Space();
        
        if (GUILayout.Button("변경하기"))
        {
            selectObject.transform.localPosition = activeLocalPosition;
        }
        
        EditorGUILayout.EndVertical();

        if (e.type == EventType.MouseMove || e.type == EventType.MouseDown)
            Repaint();
    }
    
    
    public static GameObject Select<T>(Event e) where T : UnityEngine.Component
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

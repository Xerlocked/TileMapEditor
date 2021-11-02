using System;
using System.Collections;
using System.Collections.Generic;
using Codice.Client.BaseCommands.BranchExplorer.Layout;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

public class TileEditorSpawner : EditorWindow
{
    private bool showLevelOptions = false;
    private bool bPaintMode = false;
    private int sizeLevelFloor = 1;
    private int sizeLevelWidth = 1;
    private int sizeLevelDepth = 1;
    private int sizeGridHeight = 1;
    private int sizeGridSize = 1;
    private Transform TileSpawner;
    
    [MenuItem("TileEditor/ShowSpawner")]
    static void Init()
    {
        var win = (TileEditorSpawner) GetWindow(typeof(TileEditorSpawner));
        win.Show();
    }

    public void OnGUI()
    {
        
        showLevelOptions = EditorGUILayout.Foldout(showLevelOptions, "Level Creator");
        if (showLevelOptions)
        {
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal(); // Level Value
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Level Value");
                sizeLevelFloor = EditorGUILayout.IntField("층 수", sizeLevelFloor);
                sizeLevelWidth = EditorGUILayout.IntField("가로", sizeLevelWidth);
                sizeLevelDepth = EditorGUILayout.IntField("세로", sizeLevelDepth);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(); // Grid Value
            {
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.LabelField("Grid Value");
                sizeGridHeight = EditorGUILayout.IntField("층 높이", sizeGridHeight);
                sizeGridSize = EditorGUILayout.IntField("그리드 크기", sizeGridSize);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Button("New Map Spawner");
            GUILayout.Button("Find Map Spawner");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            bPaintMode = GUILayout.Toggle(bPaintMode, "Start Creating", "Button", GUILayout.Height(60f));

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("현재 레벨:");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Button("<");
            GUILayout.Button(">");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.LabelField("선택된 타일");
        }
    }


    /*
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    
        TileSpawner map = (TileSpawner) target;
        
        //.GenerateMap();
    }
    
    private TileSpawner _tileSpawner;
    private int _objIndex;
    private int _selectIndex;
    public void OnEnable()
    {
        _tileSpawner = (TileSpawner) target;
        //SceneView.duringSceneGui += SceneUpdate;
    
        string cursorPath = "Assets/MapCreator/Prefabs/TileCursor.prefab";
        GameObject cursorPrefab = (GameObject) AssetDatabase.LoadAssetAtPath(cursorPath, typeof(GameObject));
        if (cursorPrefab)
        {
            //_tileCursor = (GameObject) PrefabUtility.InstantiatePrefab(cursorPrefab);
        }
    }
    
    private void OnSceneGUI()
    {
        _tileSpawner = (TileSpawner) target;
    
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        Event e = Event.current;
        
        Vector3 mousePos = Event.current.mousePosition;
        //mousePos.z = Camera.current.pixelHeight - mousePos.z;
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
    
        Debug.Log(ray.origin);
        
        if (Event.current.button == 0)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                GUIUtility.hotControl = controlID;
                e.Use();
                DrawObject(true, ray.origin);
            }
        }
    }
    
    void DrawObject(bool drag, Vector3 pos)
    {
        _tileSpawner = (TileSpawner) target;
        Vector3 newPos = new Vector3(
            Mathf.Floor(pos.x / _tileSpawner.width) * _tileSpawner.width + _tileSpawner.width / 2f,
            0, Mathf.Floor(pos.y / _tileSpawner.height) * _tileSpawner.height + _tileSpawner.height / 2f);
        
        Debug.Log(newPos);
        
        string cursorPath = "Assets/MapCreator/Prefabs/Cube.prefab";
        GameObject cursorPrefab = (GameObject) AssetDatabase.LoadAssetAtPath(cursorPath, typeof(GameObject));
        GameObject createGameObject = (GameObject) PrefabUtility.InstantiatePrefab(cursorPrefab);
        createGameObject.transform.parent = _tileSpawner.transform.parent;
        createGameObject.transform.position = newPos;
    }
    
    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneUpdate;
        DestroyImmediate(_tileCursor);
    }
    
    void UpdateCursorPosition(Vector3 pos)
    {
        _tileCursor.transform.localScale = new Vector3(1, 1, 1);
    
        Vector3 newPos = new Vector3(pos.x, 0, pos.z);
        
        _tileCursor.transform.position = newPos;
        Debug.Log(newPos + "newPos");
    }
    
    void SceneUpdate(SceneView view)
    {
        Event e = Event.current;
    
    
        
        Debug.Log(_ray.origin);
        
        UpdateCursorPosition(_ray.origin);
    }
    */
}

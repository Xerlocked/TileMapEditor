using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.WorkspaceServer;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEditor.UIElements;
using UnityEngine;

public class TileEditorWindow : EditorWindow
{
    private bool showLevelOptions = false;
    private bool bNewTileMap = false;
    private bool bPaintMode = false;
    private int sizeLevelFloor = 1;
    private int sizeLevelWidth = 1;
    private int sizeLevelDepth = 1;
    private int sizeGridHeight = 1;
    private int sizeGridSize = 1;
    private Transform TileSpawner;

    [SerializeField] private List<GameObject> palette = new List<GameObject>();
    string path = "Assets/MapCreator/Prefabs";

    [SerializeField] private int paletteIndex;

    private Vector2 cellSize = new Vector2(1f, 1f);
    
    private Rect rectLevelValue;
    private Rect rectGridValue;

    private Texture2D texture;

    private GUIStyle customGUIStyle_label;
    private GUIStyle customGUIStyle_foldout;
    private GUIStyle customGUIStyle_helpbox;
    
    [MenuItem("TileEditor/ShowEditor")]
    static void Init()
    {
        var window = (TileEditorWindow) GetWindow(typeof(TileEditorWindow));
        window.position = new Rect(0, 0, 800, 600);
        window.Show();
    }

    void InitGUIStyle()
    {
        customGUIStyle_label = new GUIStyle(EditorStyles.label)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };

        customGUIStyle_foldout = new GUIStyle(EditorStyles.foldout)
        {
            padding = new RectOffset(15,0,15,15),
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };

        customGUIStyle_helpbox = new GUIStyle(EditorStyles.helpBox)
        {
            margin = new RectOffset(20, 20, 15, 0),
            padding = new RectOffset(15,15,5,5)
            
        };
    }
    
    public void OnGUI()
    {
        InitGUIStyle();

        showLevelOptions = EditorGUILayout.Foldout(showLevelOptions, "Level Creator",customGUIStyle_foldout);
        if (showLevelOptions)
        {
            EditorGUILayout.Space();
            EditorGUI.indentLevel++;
            
            EditorGUILayout.BeginHorizontal(); // Level Value
            {
                EditorGUILayout.BeginVertical(customGUIStyle_helpbox);
                EditorGUILayout.LabelField("Level Value", customGUIStyle_label);
                sizeLevelFloor = EditorGUILayout.IntField("층 수",sizeLevelFloor);
                sizeLevelWidth = EditorGUILayout.IntField("가로", sizeLevelWidth);
                sizeLevelDepth = EditorGUILayout.IntField("세로", sizeLevelDepth);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal(); // Grid Value
            {
                EditorGUILayout.BeginVertical(customGUIStyle_helpbox);
                EditorGUILayout.LabelField("Grid Value", customGUIStyle_label);
                sizeGridHeight = EditorGUILayout.IntField("층 높이", sizeGridHeight);
                sizeGridSize = EditorGUILayout.IntField("그리드 크기", sizeGridSize);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            bNewTileMap = GUILayout.Button("New Map Spawner");
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

            if (bNewTileMap)
            {
                Debug.Log("bNewTileMap Click!");
                TileSpawner= new GameObject("TileSpawner").transform;
            }

            List<GUIContent> paletteIcons = new List<GUIContent>();

            foreach (GameObject prefab in palette)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(prefab);
                paletteIcons.Add(new GUIContent(texture));
            }

            paletteIndex = GUILayout.SelectionGrid(paletteIndex, paletteIcons.ToArray(), 6);
        }
    }

    private void OnSceneGUI(SceneView view)
    {
        if (bPaintMode)
        {
            Vector2 cellCenter = GetSelectedCell();
            
            DisplayHelper(cellCenter);
            HandleSceneViewInputs(cellCenter);
            
            SceneView.RepaintAll();
        }
    }

    private void HandleSceneViewInputs(Vector2 cellCenter)
    {
        if (Event.current.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(0);
        }

        Vector3 newPos = new Vector3(cellCenter.x, 0, cellCenter.y);
        
        if (paletteIndex < palette.Count && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GameObject prefab = palette[paletteIndex];
            GameObject go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            go.transform.position = newPos;
            go.transform.parent = TileSpawner;
        }
    }

    private void OnFocus()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
        SceneView.duringSceneGui += this.OnSceneGUI;
        
        RefreshPalette();
    }

    private void RefreshPalette()
    {
        palette.Clear();

        string[] prefabFiles = System.IO.Directory.GetFiles(path, "*.prefab");
        foreach (string prefabFile in prefabFiles)
        {
            palette.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
        }
    }
    
    private void OnDestroy()
    {
        SceneView.duringSceneGui -= this.OnSceneGUI;
    }

    public void OnInspectorUpdate()
    {
        this.Repaint();
    }

    private void DisplayHelper(Vector2 cellCenter)
    {
        // Vertices of our square
        Vector3 topLeft = cellCenter + Vector2.left * cellSize * 0.5f + Vector2.up * cellSize * 0.5f;
        SwapToZero(out topLeft.z, ref topLeft.y);

        Vector3 topRight = cellCenter - Vector2.left * cellSize * 0.5f + Vector2.up * cellSize * 0.5f;
        SwapToZero(out topRight.z, ref topRight.y);
        
        Vector3 bottomLeft = cellCenter + Vector2.left * cellSize * 0.5f  - Vector2.up * cellSize * 0.5f;
        SwapToZero(out bottomLeft.z, ref bottomLeft.y);
        
        Vector3 bottomRight = cellCenter - Vector2.left * cellSize * 0.5f - Vector2.up * cellSize * 0.5f;
        SwapToZero(out bottomRight.z, ref bottomRight.y);

        // Rendering
        Handles.color = Color.green;
        Vector3[] lines = { topLeft, topRight, topRight, bottomRight, bottomRight, bottomLeft, bottomLeft, topLeft };
        Handles.DrawLines(lines);
    }

    private Vector2 GetSelectedCell()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); // 마우스 좌표
        Vector3 mousePosition = ray.origin - ray.direction * (ray.origin.y / ray.direction.y); // y 좌표 지움

        Vector2Int cell = new Vector2Int(Mathf.RoundToInt(mousePosition.x / cellSize.x),
            Mathf.RoundToInt(mousePosition.z / cellSize.y));

        return cell * cellSize;
    }
    
    private void SwapToZero(out float dst, ref float src)
    {
        dst = src;
        src = 0;
    }
}

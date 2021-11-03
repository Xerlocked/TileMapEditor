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
    private bool bEndWork = false;
    private bool canPaint = false;
    
    private int lcFloor = 1;
    private int lcFloorRange = 1;
    private float lcFloorHeight = 1f;
    private float lcGridSizeX = 1f;
    private float lcGridSizeY = 1f;

    private Transform TileSpawner;

    [SerializeField] 
    private List<GameObject> palette = new List<GameObject>();
    
    string path = "Assets/MapCreator/Prefabs";

    [SerializeField] 
    private int paletteIndex;

    [SerializeField]
    private Vector2 cellSize = new Vector2(1f, 1f);
    
    private Rect rectLevelValue;
    private Rect rectGridValue;

    private Texture2D texture;

    private GUIStyle customGUIStyle_label;
    private GUIStyle customGUIStyle_foldout;
    private GUIStyle customGUIStyle_helpbox;
    private GUIStyle customGUIStyle_Button;
    private GUIStyle customGUIStyle_ToggleButton;
    
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
            fontStyle = FontStyle.Bold,
            margin = new RectOffset(0,0,0,10)
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

        customGUIStyle_Button = new GUIStyle(EditorStyles.miniButton)
        {
            margin = new RectOffset(20,20,0,0),
            fontStyle = FontStyle.Bold,
            fixedHeight = 40f
        };
    }
    
    public void OnGUI()
    {
        InitGUIStyle();
        showLevelOptions = EditorGUILayout.Foldout(showLevelOptions, "TileEditor - Xerlock",customGUIStyle_foldout);
        if (showLevelOptions)
        {
            EditorGUI.indentLevel++;
            
            #region 1
            EditorGUI.BeginDisabledGroup(canPaint); // DisableGroup (1) 시작
            EditorGUILayout.BeginHorizontal(); // Level Value
            {
                EditorGUILayout.BeginVertical(customGUIStyle_helpbox);
                EditorGUILayout.LabelField("제작 설정", customGUIStyle_label);
                EditorGUILayout.Space();
                lcFloor = EditorGUILayout.IntField("층 수",lcFloor);
                EditorGUILayout.Space();
                lcFloorHeight = EditorGUILayout.FloatField("층 높이", lcFloorHeight);
                EditorGUILayout.Space();
                lcGridSizeX = EditorGUILayout.FloatField("Grid Size X", lcGridSizeX);
                EditorGUILayout.Space();
                lcGridSizeY =  EditorGUILayout.FloatField("Grid Size Y", lcGridSizeY);
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            #endregion
            
            EditorGUILayout.Space();

            #region 2
            
            EditorGUILayout.BeginHorizontal();
            bNewTileMap = GUILayout.Button("새로운 맵 만들기", customGUIStyle_Button);
            GUILayout.Button("기존 맵 이어하기", customGUIStyle_Button);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup(); // DisableGroup (1) 끝
            #endregion
            
            EditorGUILayout.Space();
            
            #region 3
            EditorGUI.BeginDisabledGroup(!canPaint); // DisableGroup (2) 시작
            bPaintMode = GUILayout.Toggle(bPaintMode, "그리기 모드", customGUIStyle_Button);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("현재 레벨",GUILayout.Width(100f));
            EditorGUILayout.IntSlider(lcFloorRange, 1, lcFloor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.EndDisabledGroup(); // DisableGroup (2) 끝
            #endregion
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("선택된 타일",customGUIStyle_label);

            List<GUIContent> paletteIcons = new List<GUIContent>();

            foreach (GameObject prefab in palette)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(prefab);
                paletteIcons.Add(new GUIContent(texture));
            }

            EditorGUILayout.BeginVertical(customGUIStyle_helpbox);
            paletteIndex = GUILayout.SelectionGrid(paletteIndex, paletteIcons.ToArray(), 6,GUILayout.Height(60f));
            EditorGUILayout.EndVertical();
            
            EditorGUI.BeginDisabledGroup(!canPaint);
            bEndWork = GUILayout.Button("작업 끝내기", customGUIStyle_Button);
            EditorGUI.EndDisabledGroup();
            if (bNewTileMap)
            {
                TileSpawner = new GameObject("TileSpawner").transform;
                canPaint = true;
            }

            if (bEndWork)
            {
                if (!HasChild(TileSpawner))
                {
                    DestroyImmediate(TileSpawner.gameObject);
                }
                
                bPaintMode = false;
                canPaint = false;
            }
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

        if (paletteIndex < palette.Count && 
            Event.current.type == EventType.MouseDown && 
            Event.current.button == 0)
        {
            GameObject go = PrefabUtility.InstantiatePrefab(palette[paletteIndex]) as GameObject;
            go.transform.position = new Vector3(cellCenter.x, 0, cellCenter.y);
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

    private bool HasChild(Transform transform)
    {
        return (transform.childCount > 0) ? true : false;
    }
    
    private void SwapToZero(out float dst, ref float src)
    {
        dst = src;
        src = 0;
    }
}

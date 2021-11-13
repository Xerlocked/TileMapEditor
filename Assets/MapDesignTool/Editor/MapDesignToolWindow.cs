using System;
using System.Collections.Generic;
using MapDesignTool.Scripts;
using UnityEditor;
using UnityEditor.SearchService;
using UnityEngine;

namespace MapDesignTool.Editor
{
    public class MapDesignToolWindow : EditorWindow
    {
        private bool bPaintMode = false;
        private bool bEndWork = false;
        private bool canPaint = false;
    
        private int lcFloor = 1;
        private int lcFloorRange = 1;
        private float lcFloorHeight = 1f;
        private float lcGridSizeX = 1f;
        private float lcGridSizeY = 1f;

        private int selectedIndex = 0;
        Vector2 mouseDown = Vector2.zero;
        Vector2 mouseUp = Vector2.zero;
        Vector2 mouseMove = Vector2.zero;
        
        private string[] pathList = new[]
        {
            "Tiles", "Props", "Environments"
        };

        private Color lineColor = Color.green;
        
        private MapDesignPreferences _preferences;

        private Transform TileSpawner;

        [SerializeField]
        private GameObject selectPrefab;
        
        [SerializeField] 
        private List<GameObject> palette = new List<GameObject>();
        
        [SerializeField] 
        private int paletteIndex;

        [SerializeField]
        private Vector2 cellSize = new Vector2(1f, 1f);

        delegate void CALLBACK_2(GameObject go, Vector2 v);
        
        private GUIStyle customGUIStyle_label;
        private GUIStyle customGUIStyle_foldout;
        private GUIStyle customGUIStyle_helpbox;
        private GUIStyle customGUIStyle_Button;
        private GUIStyle customGUIStyle_ToggleButton;
    
        [MenuItem("MapDesignTool/Open New Window")]
        static void Init()
        {
            var window = (MapDesignToolWindow) GetWindow(typeof(MapDesignToolWindow), false, "MapDesignTool");
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
                lcGridSizeY =  EditorGUILayout.FloatField("Grid Size Y (Z)", lcGridSizeY);
                EditorGUILayout.Space();
                TileSpawner = EditorGUILayout.ObjectField(TileSpawner, typeof(Transform), true) as Transform;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            #endregion
        
            EditorGUILayout.Space();

            #region 2
        
            EditorGUILayout.BeginHorizontal();
        
            if (GUILayout.Button("새로운 맵 만들기", customGUIStyle_Button))
            {
                GameObject temporaryObject = new GameObject("TileSpawner");
                temporaryObject.AddComponent(typeof(TileSpawner));
                TileSpawner = temporaryObject.transform;
                _preferences.ToolMode = MapDesignPreferences.TOOLMODE.Drawing;
                canPaint = true;
            }
        
            if (GUILayout.Button("기존 맵 이어하기", customGUIStyle_Button))
            {
                if (TileSpawner == null)
                {
                    Debug.LogError("TileSpawner가 설정되지 않았습니다. 설정 후 다시 시도해주세요.");
                    return;
                }

                _preferences.ToolMode = MapDesignPreferences.TOOLMODE.Drawing;
                canPaint = true;
            }
        
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup(); // DisableGroup (1) 끝
            #endregion
        
            EditorGUILayout.Space();
        
            #region 3
            EditorGUI.BeginDisabledGroup(!canPaint); // DisableGroup (2) 시작
            bPaintMode = GUILayout.Toggle(bPaintMode, "그리기 모드", customGUIStyle_Button);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("현재 층 수",GUILayout.Width(100f));
            lcFloorRange = EditorGUILayout.IntSlider(lcFloorRange, 1, lcFloor);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup("Prefab Path", selectedIndex, pathList);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshPalette();
            }
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
            paletteIndex = GUILayout.SelectionGrid(paletteIndex, paletteIcons.ToArray(), 5,GUILayout.Height(128f));
            EditorGUILayout.EndVertical();
        
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(!canPaint);
            bEndWork = GUILayout.Button("작업 끝내기", customGUIStyle_Button);
            EditorGUI.EndDisabledGroup();

            if (bEndWork)
            {
                try
                {
                    if (!Helpers.HasChild(TileSpawner) && TileSpawner != null)
                    {
                        DestroyImmediate(TileSpawner.gameObject);
                    }
                
                    bPaintMode = false;
                    canPaint = false;
                    _preferences.ToolMode = MapDesignPreferences.TOOLMODE.Move;
                }
                catch (MissingReferenceException)
                {
                    bPaintMode = false;
                    canPaint = false;
                }
            }

            if (_preferences.ToolMode != MapDesignPreferences.TOOLMODE.Drawing)
            {
                canPaint = false;
                bPaintMode = false;
            }
            else if (!bPaintMode && _preferences.ToolMode == MapDesignPreferences.TOOLMODE.Drawing && TileSpawner != null)
            {
                canPaint = true;
            }
            else if (_preferences.ToolMode == MapDesignPreferences.TOOLMODE.Drawing && TileSpawner == null)
            {
                Debug.LogError("TileSpawner가 설정되지 않았습니다. 설정 후 다시 시도해주세요.");
                bPaintMode = false;
                _preferences.ToolMode = MapDesignPreferences.TOOLMODE.Move;
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (bPaintMode)
            {
                cellSize.x = lcGridSizeX;
                cellSize.y = lcGridSizeY;

                Vector2 cellCenter = GetSelectedCell();
            
                //Debug.Log(cellCenter);
                DisplayHelper(cellCenter);
                
                HandleSceneViewInputs(cellCenter);
                
                SceneView.RepaintAll();
            }

            Cursor.visible = false;
        }

        private void HandleSceneViewInputs(Vector2 cellCenter)
        {
            var e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            
            Action Delete = () =>
            {
                GameObject go = Helpers.Select<Transform>(e);
                if (go == null) return;
                DestroyImmediate(PrefabUtility.GetOutermostPrefabInstanceRoot(go));
            };
            
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlID);
            }

            // Delete Prefab if Pressed "Space Bar"
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
            {
                Delete();
            }
            
            if (paletteIndex < palette.Count && e.type == EventType.MouseDown && e.button == 0 && e.isMouse)
            {
                GUIUtility.hotControl = controlID;
                mouseDown = cellCenter;
                lineColor = Color.red;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.isMouse)
            {
                mouseUp = cellCenter;

                if (e.button == 0)
                {
                    GUIUtility.hotControl = controlID;

                    if (mouseDown != mouseUp && e.shift)
                    {
                        CreateDragPrefab(mouseDown, mouseUp, palette[paletteIndex], CreatePrefab);
                    }
                    else
                    {
                        CreatePrefab(palette[paletteIndex], mouseUp);
                    }
                }

                lineColor = Color.green;
            }
            else if (e.type == EventType.MouseMove && e.isMouse)
            {
                mouseMove = cellCenter;
                if (e.button != 0) return;
                if (e.alt)
                {
                    CALLBACK_2 Move = (o, v) =>
                    {
                        if (o == null) return;
                        
                        Vector3 newPos = new Vector3(v.x, FloorHeight(), v.y);
                        o.transform.position = newPos;
                    };
                    
                    Move(GetCurrentPrefab(e), mouseMove);
                }
            }
            // else if (e.button == 0 && e.type == EventType.MouseMove && e.isMouse)
            // {
            //     mouseMove = cellCenter;
            //     GUIUtility.hotControl = controlID;
            //     MovePrefab(GetCurrentPrefab(e), mouseMove);
            // }
        }

        private float FloorHeight()
        {
            return (lcFloorHeight * lcFloorRange) - lcFloorHeight;
        }
    
        private void CreatePrefab(GameObject prefab,Vector2 mousePosition)
        {
            if (prefab == null)
                return;
            
            Vector3 newPos = new Vector3(mousePosition.x, FloorHeight(), mousePosition.y);
            
            Undo.IncrementCurrentGroup();
            selectPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            System.Diagnostics.Debug.Assert(selectPrefab != null, nameof(selectPrefab) + " != null");
            selectPrefab.transform.position = newPos;
            selectPrefab.transform.localScale = new Vector3(lcGridSizeX,1,lcGridSizeY);
            selectPrefab.transform.parent = TileSpawner;

            // Vector3 worldUp = selectPrefab.transform.InverseTransformDirection(Vector3.up).normalized;
            //
            // Vector3[] vertices = selectPrefab.GetComponent<MeshFilter>().sharedMesh.vertices;
            // Plane p = new Plane(worldUp, Vector3.zero);
            // Vector3 topVert = Vector3.zero;
            // float maxDist = float.NegativeInfinity;
            //
            // for (int i = 0; i < vertices.Length; i++)
            // {
            //     float dist = p.GetDistanceToPoint(vertices[i]);
            //     if (dist > maxDist)
            //     {
            //         maxDist = dist;
            //         topVert = vertices[i];
            //     }
            // }
            //
            // topVert = selectPrefab.transform.TransformPoint(topVert);
            
            var maxBounds = selectPrefab.GetComponent<MeshFilter>().sharedMesh.bounds.max;
 
            Matrix4x4 localToWorld = selectPrefab.transform.localToWorldMatrix;
 
            Vector3 hi = localToWorld.MultiplyPoint3x4(maxBounds);
            
            Debug.Log(hi);
            Undo.RegisterCreatedObjectUndo(selectPrefab, "Create New Prefab");
        }

        private void CreateDragPrefab(Vector2 startPos, Vector2 endPos, GameObject prefab,  CALLBACK_2 callBack)
        {
            float xMin = startPos.x < endPos.x ? startPos.x : endPos.x;
            float yMin = startPos.y < endPos.y ? startPos.y : endPos.y;
            float xMax = startPos.x > endPos.x ? startPos.x : endPos.x;
            float yMax = startPos.y > endPos.y ? startPos.y : endPos.y;

            int cols = (int) (Mathf.Abs(startPos.x - endPos.x) / lcGridSizeX);
            int rows = (int) (Mathf.Abs(startPos.y - endPos.y) / lcGridSizeY);

            for (int i = 0; i <= rows; i++)
            {
                for (int j = 0; j <= cols; j++)
                {
                    callBack(prefab, new Vector2(xMin + j * lcGridSizeX, yMin + i * lcGridSizeY));
                }
            }
        }
        
        private GameObject GetCurrentPrefab(Event e)
        {
            GameObject go = Helpers.Select<Transform>(e);
            return PrefabUtility.GetOutermostPrefabInstanceRoot(go);
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

            string[] prefabFiles = System.IO.Directory.GetFiles(_preferences.prefabPath[selectedIndex], "*.prefab");

            foreach (string prefabFile in prefabFiles)
            {
                palette.Add(AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject);
            }
        }

        private void OnEnable()
        {
            _preferences = AssetDatabase.LoadAssetAtPath("Assets/MapDesignTool/MapDesignPreferences.asset", typeof(MapDesignPreferences))as MapDesignPreferences;
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
            Helpers.SwapToZero(out topLeft.z, ref topLeft.y);
            topLeft.y = FloorHeight();
            //Debug.Log(topLeft);

            Vector3 topRight = cellCenter - Vector2.left * cellSize * 0.5f + Vector2.up * cellSize * 0.5f;
            Helpers.SwapToZero(out topRight.z, ref topRight.y);
            topRight.y = FloorHeight();
        
            Vector3 bottomLeft = cellCenter + Vector2.left * cellSize * 0.5f  - Vector2.up * cellSize * 0.5f;
            Helpers.SwapToZero(out bottomLeft.z, ref bottomLeft.y);
            bottomLeft.y = FloorHeight();
        
            Vector3 bottomRight = cellCenter - Vector2.left * cellSize * 0.5f - Vector2.up * cellSize * 0.5f;
            Helpers.SwapToZero(out bottomRight.z, ref bottomRight.y);
            bottomRight.y = FloorHeight();
        
            // Rendering
            Handles.color = lineColor;
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
    }
}

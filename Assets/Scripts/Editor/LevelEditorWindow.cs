using Entities;
using Grid_System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        
        private GameObject[] _prefabs;
        private GameObject _selectedPrefab;
        private Vector2 _scrollPosition;

        [MenuItem("Window/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnGUI()
        {
            GUILayout.Label("Drag and Drop Prefabs Here", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (GUILayout.Button("Load Prefabs"))
            {
                LoadPrefabs();
            }

            EditorGUILayout.Space();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_prefabs is { Length: > 0 })
            {
                var columns = Mathf.FloorToInt(position.width / 110); // Calculate number of columns based on window width
                var rows = Mathf.CeilToInt((float)_prefabs.Length / columns);

                for (var row = 0; row < rows; row++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (var col = 0; col < columns; col++)
                    {
                        var index = row * columns + col;
                        if (index < _prefabs.Length)
                        {
                            if (GUILayout.Button(AssetPreview.GetAssetPreview(_prefabs[index]), GUILayout.Width(100), GUILayout.Height(100)))
                            {
                                _selectedPrefab = _prefabs[index];
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();

            if (_selectedPrefab != null)
            {
                GUILayout.Label("Selected Prefab: " + _selectedPrefab.name);
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Click in the scene to place the selected prefab.", MessageType.Info);
        }

        private void LoadPrefabs()
        {
            var path = EditorUtility.OpenFolderPanel("Select Prefab Folder", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                path = "Assets" + path.Substring(Application.dataPath.Length);
                var guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
                _prefabs = new GameObject[guids.Length];
                for (var i = 0; i < guids.Length; i++)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    _prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                }
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            var e = Event.current;

            if (e.type != EventType.MouseDown || e.button != 0 || _selectedPrefab == null) return;
            
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var gridController = GridController.Instance;
                var grid = gridController.grid;

                if (grid == null)
                {
                    gridController.InitializeGridOnStart();
                }
                var (x, y) = grid.GetGridIndex(hit.point);
                var cell = grid.GetCell(x, y);

                if (_selectedPrefab.GetComponent<ArrowCellObject>() != null)
                {
                    var cellObject = cell.objectStackList[^1];
                    if (cellObject == null) return;
                    var tileCellObject = cellObject as TileCellObject;
                    if (tileCellObject == null) return;

                    tileCellObject.spawnableCellObject.cellObjectToSpawn = _selectedPrefab.GetComponent<CellObject>();
                }
                else
                {
                    var instance = (GameObject) PrefabUtility.InstantiatePrefab(_selectedPrefab);
                
                    if (grid.InsideBounds(hit.point))
                    {
                        cell.AddCellObject(instance.GetComponent<CellObject>());
                        
                        Undo.RegisterCreatedObjectUndo(instance, "Placed Prefab");
                    }
                }
                
                    
            }

            e.Use();
        }
        
        
    }
}

using System.Collections.Generic;
using Entities;
using Grid_System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelEditorWindow : EditorWindow
    {
        
        private List<GameObject> _prefabs = new();
        private GameObject _selectedPrefab;
        private Vector2 _scrollPosition;
        
        public Transform tempTransform;
        private Transform _lastInstantiatedObject;
        private Transform _lastInteractedCell;

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

            if (_prefabs is { Count: > 0 })
            {
                var columns = Mathf.FloorToInt(position.width / 110); // Calculate number of columns based on window width
                var rows = Mathf.CeilToInt((float)_prefabs.Count / columns);

                for (var row = 0; row < rows; row++)
                {
                    EditorGUILayout.BeginHorizontal();
                    for (var col = 0; col < columns; col++)
                    {
                        var index = row * columns + col;
                        if (index >= _prefabs.Count) continue;
                        if (GUILayout.Button(AssetPreview.GetAssetPreview(_prefabs[index]), GUILayout.Width(100), GUILayout.Height(100)))
                        {
                            _selectedPrefab = _prefabs[index];
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
            EditorGUILayout.HelpBox("Press [R] to rotate arrows and frogs.", MessageType.Warning);
            EditorGUILayout.HelpBox("First select the Grid Controller and click [Setup Grid]", MessageType.Info);
            EditorGUILayout.HelpBox("Click on the cells to place the selected prefab.", MessageType.Info);
        }

        private void LoadPrefabs()
        {
            _prefabs.Clear();
            var path = EditorUtility.OpenFolderPanel("Select Prefab Folder", "Assets", "");
            if (string.IsNullOrEmpty(path)) return;
            path = "Assets" + path[Application.dataPath.Length..];
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { path });
            foreach (var t in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(t);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab.GetComponent<GrapeCellObject>()) continue;
                if (!prefab.GetComponent<CellObject>()) continue;
                _prefabs.Add(prefab);
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            
            
            //Find the temp game object
            var tempTransformGo = GameObject.Find("temp");
            if (tempTransformGo == null) return;
            
            tempTransform = tempTransformGo.transform;
            if (tempTransform == null) return;
            for (var i = 0; i < tempTransform.childCount; i++)
            {
                tempTransform.GetChild(i).gameObject.SetActive(true);
 
            }
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            
            if (tempTransform == null) return;
            for (var i = 0; i < tempTransform.childCount; i++)
            {
                tempTransform.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            
            // Disable the selection.
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            
            var e = Event.current;

            if (e.isKey && e.keyCode == KeyCode.R && e.type == EventType.KeyUp)
            {
                if (RotateLastPlacedObject(e)) return;
            }
            
            if (e.type != EventType.MouseDown || e.button != 0 || _selectedPrefab == null) return;

            var tempTransformGo = GameObject.Find("temp");
            if (tempTransformGo == null)
            {
                tempTransformGo = new GameObject("temp");
            }
            tempTransform = tempTransformGo.transform;
            
            if (HandleMouseClick(e)) return;

            e.Use();
        }

        private bool RotateLastPlacedObject(Event e)
        {
            if (_lastInstantiatedObject == null || _lastInteractedCell == null) return true;
                
            var rot  = _lastInstantiatedObject.transform.rotation.eulerAngles;
            var newRot = Quaternion.Euler(rot.x, rot.y + 90, rot.z);
            _lastInstantiatedObject.transform.rotation = newRot;
                
            var cellObject = _lastInteractedCell.GetComponent<Cell>().objectStackList[^1];
            if (cellObject == null) return true;
            var tileCellObject = cellObject as TileCellObject;
            if (tileCellObject == null) return true;

            tileCellObject.spawnableCellObject.yRotation = newRot.eulerAngles.y;
            e.Use();
            return false;
        }

        private bool HandleMouseClick(Event e)
        {
            var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (!Physics.Raycast(ray, out var hit)) return false;
            
            var gridController = GridController.Instance;
            var grid = gridController.Grid;

            if (grid == null)
            {
                gridController.InitializeGridOnStart();
                grid = gridController.Grid;
            }
            
            if (!grid.InsideBounds(hit.point))
            {
                return false;
            }
            
            var (x, y) = grid.GetGridIndex(hit.point);
            var cell = grid.GetCell(x, y);

            _lastInteractedCell = cell.transform;
            
            if (_selectedPrefab.GetComponent<TileCellObject>() != null)
            {
                var instance = (GameObject) PrefabUtility.InstantiatePrefab(_selectedPrefab);
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                instance.transform.rotation = Quaternion.Euler(0, 0, 0);
                _lastInstantiatedObject = instance.transform;
                instance.transform.SetParent(cell.transform);
                cell.AddCellObject(instance.GetComponent<CellObject>());
                        
                Undo.RegisterCreatedObjectUndo(instance, "Placed Prefab");

            }
            else
            {
                var cellObject = cell.objectStackList[^1];
                if (cellObject == null) return true;
                var tileCellObject = cellObject as TileCellObject;
                if (tileCellObject == null) return true;
                if (tileCellObject.spawnableCellObject.cellObjectToSpawn != null) return true;

                tileCellObject.spawnableCellObject.cellObjectToSpawn = _selectedPrefab.GetComponent<CellObject>();

                // Mark the object as dirty to ensure changes are saved
                EditorUtility.SetDirty(tileCellObject);
                
                var tempInstance = (GameObject) PrefabUtility.InstantiatePrefab(_selectedPrefab);
                PrefabUtility.UnpackPrefabInstance(tempInstance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                tempInstance.transform.rotation = Quaternion.Euler(0, 0, 0);
                tempInstance.transform.SetParent(tempTransform);
                tempInstance.transform.position = tileCellObject.transform.position + Vector3.up * .1f;
                _lastInstantiatedObject = tempInstance.transform;
                
                Undo.RegisterCreatedObjectUndo(tempInstance, "Placed Prefab");
            }

            return false;
        }
    }
}

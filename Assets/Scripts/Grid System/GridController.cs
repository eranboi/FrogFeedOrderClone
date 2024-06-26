using System;
using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Grid_System
{
    [ExecuteInEditMode] // Allows the script to run in the editor
    public class GridController : MonoBehaviour
    {
        public static GridController Instance;
        
        [Header("Grid Settings")] 
        [SerializeField]
        public int width;
        [Header("Grid Settings")] 
        [SerializeField] private int height;
        [Header("Grid Settings")] 
        [SerializeField] private int cellSize;

        [SerializeField] private GameObject cellPrefab;

        [FormerlySerializedAs("_instantiatedCells")] [SerializeField] private List<GameObject> instantiatedCells = new();
        [SerializeField] private Transform cellParent; 

        public Grid Grid;
        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        private void Start()
        {
            if (Grid is null)
            {
                InitializeGridOnStart();
            }

            RemoveTempObjects();
            SetCameraPos();
        }

        public void InitializeGridOnStart()
        {
            //ClearGrid();

            Grid = new Grid(width, height, 1);

            for (int i = 0; i < cellParent.childCount; i++)
            {
                var cell = cellParent.GetChild(i);
                
                var (x, y) = Grid.GetGridIndex(cell.transform.position);
                Grid.AddCell(x, y, cell.GetComponent<Cell>());

                if (instantiatedCells.Contains(cell.gameObject)) continue;
                
                instantiatedCells.Add(cell.gameObject);
            }
        }

        private void RemoveTempObjects()
        {
            var tempTransformGo = GameObject.Find("temp");
            if (tempTransformGo == null)
            {
                Debug.Log("tempTransformGo is null.");
                return;
            }

            var childCount = tempTransformGo.transform.childCount;

            Debug.Log($"childCount is {childCount}", tempTransformGo);
            
            for (var i = 0; i < childCount; i++)
            {
                var child = tempTransformGo.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }
        }

        private void SetCameraPos()
        {
            var xPos = width / 2f;
            var zPos = height / 2f;


            Camera.main.transform.position = new Vector3(xPos, 10, zPos);
        }

        #if UNITY_EDITOR
        [Button]
        public void SetupGrid()
        {
            if (cellParent == null)
            {
                var cellParentInstantiated = new GameObject("Cells");
                cellParent = cellParentInstantiated.transform;
            }
            if (instantiatedCells.Count > 0)
            {
                ClearGrid();
            }
            Grid = new Grid(width, height, 1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pos = Grid.GetWorldPositionCenter(x, y);
                    var cellInstantiated = Instantiate(cellPrefab, pos, Quaternion.identity);
                    cellInstantiated.transform.SetParent(cellParent);
                    instantiatedCells.Add(cellInstantiated);
                    Grid.AddCell(x, y, cellInstantiated.GetComponent<Cell>());
                }
            }
        }
        

        [Button]
        private void ClearGrid()
        {
            Grid = null;
            for (var i = 0; i < instantiatedCells.Count; i++)
            {
                var t = instantiatedCells[i];
                if (t == null)
                {
                    instantiatedCells.RemoveAt(i);
                    continue;
                }

                var cell = t.GetComponent<Cell>();

                if (cell is null) continue;
                cell.ClearCell();
                DestroyImmediate(t);
            }
            
            var tempTransformGo = GameObject.Find("temp");
            if (tempTransformGo == null) return;
            
            var childCount = tempTransformGo.transform.childCount;
            
            for (var i = childCount - 1; i >= 0; i--)
            {
                var child = tempTransformGo.transform.GetChild(i);
                DestroyImmediate(child.gameObject);
            }

            instantiatedCells.Clear();
        }
        #endif

        public bool CheckIsClear()
        {
            var isClear = true;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = Grid.GetCell(x, y);
                    if (cell.objectStackList.Count != 0)
                    {
                        isClear = false;
                    }
                }
            }

            return isClear;
        }

        private void OnDrawGizmos()
        {
            if (Grid == null) return;
            if (Grid.GetGrid().Length == 0) return;

            var gridArray = Grid.GetGrid();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.DrawWireCube(Grid.GetWorldPositionCenter(x, y), new Vector3(cellSize, cellSize, cellSize));
                }
            }
        }

        #if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            var instance = FindObjectOfType<GridController>();
            if (instance != null)
            {
                Instance = instance;
            }
        }
        #endif
    }
}

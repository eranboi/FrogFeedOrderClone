using System;
using System.Collections.Generic;
using Managers;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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

        [SerializeField] private List<GameObject> _instantiatedCells = new();
        [SerializeField] private Transform cellParent; 

        public Grid grid;
        

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
            if (grid is null)
            {
                InitializeGridOnStart();
            }
        }

        public void InitializeGridOnStart()
        {
            //ClearGrid();

            grid = new Grid(width, height, 1);

            for (int i = 0; i < cellParent.childCount; i++)
            {
                var cell = cellParent.GetChild(i);
                _instantiatedCells.Add(cell.gameObject);
                var (x, y) = grid.GetGridIndex(cell.transform.position);
                grid.AddCell(x, y, cell.GetComponent<Cell>());
            }
        }

        #if UNITY_EDITOR
        [Button]
        public void SetupGrid()
        {
            if (_instantiatedCells.Count > 0)
            {
                ClearGrid();
            }
            grid = new Grid(width, height, 1);

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    var pos = grid.GetWorldPositionCenter(x, y);
                    var cellInstantiated = Instantiate(cellPrefab, pos, Quaternion.identity);
                    cellInstantiated.transform.SetParent(cellParent);
                    _instantiatedCells.Add(cellInstantiated);
                    grid.AddCell(x, y, cellInstantiated.GetComponent<Cell>());
                }
            }
        }
        

        [Button]
        private void ClearGrid()
        {
            grid = null;
            foreach (var t in _instantiatedCells)
            {
                if (t == null) continue;
                    var cell = t.GetComponent<Cell>();
                    
                if (cell is null) continue;
                    cell.ClearCell();
                DestroyImmediate(t);
            }
            
            _instantiatedCells.Clear();
        }
        #endif

        public bool CheckIsClear()
        {
            var isClear = true;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var cell = grid.GetCell(x, y);
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
            if (grid == null) return;
            if (grid.GetGrid().Length == 0) return;

            var gridArray = grid.GetGrid();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.DrawWireCube(grid.GetWorldPositionCenter(x, y), new Vector3(cellSize, cellSize, cellSize));
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

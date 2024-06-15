using System.Collections.Generic;
using UnityEngine;

namespace Grid_System
{
    public class Grid
    {
        private int _width;
        private int _height;
        private float _cellSize;
        private Cell[,] _gridArray;

        public Grid(int width, int height, float cellSize)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;

            _gridArray = new Cell[width, height];
        }

        public void AddCell(int x, int y, Cell cell)
        {
            _gridArray[x, y] = cell;
        }

        public Cell GetCell(int x, int y)
        {
            if (InsideBounds(x, y))
                return _gridArray[x, y];
            else return null;
        }
        
        public Vector3 GetWorldPosition(int x, int y)
        {
            if(!InsideBounds(x, y)) return new Vector3(-1, -1, -1);

            return new Vector3(x, 0, y) * _cellSize;
        }

        public Vector3 GetWorldPositionCenter(int x, int y)
        {
            if(!InsideBounds(x, y)) return new Vector3(-1, -1, -1);
            
            return new Vector3(x, 0, y) * _cellSize + new Vector3(_cellSize, 0, _cellSize) * 0.5f;
        }

        public (int, int) GetGridIndex(Vector3 worldPosition)
        {
            if(!InsideBounds(worldPosition)) return(-1, -1);

            int xIndex = Mathf.FloorToInt(worldPosition.x / _cellSize);
            int yIndex = Mathf.FloorToInt(worldPosition.z / _cellSize);

            return (xIndex, yIndex);
        }

        public void AddObject(int x, int y, Cell cell)
        {
            if(!InsideBounds(x, y)) return;

            _gridArray[x, y] = cell;
        }

        public bool InsideBounds(Vector3 worldPosition)
        {
            return worldPosition.x >= 0 && worldPosition.x <= _width * _cellSize &&
                   worldPosition.z >= 0 && worldPosition.z <= _height * _cellSize;
        }

        public bool InsideBounds(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public Cell[,] GetGrid()
        {
            return _gridArray;
        }
    }
}

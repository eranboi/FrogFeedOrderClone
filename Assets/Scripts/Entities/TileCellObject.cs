using UnityEngine;

namespace Entities
{
    public class TileCellObject : CellObject
    {
        public SpawnableCellObject spawnableCellObject;
    }
    
    [System.Serializable]
    public class SpawnableCellObject
    {
        public CellObject _cellObjectToSpawn;
        public CellObject cellObjectToSpawn
        {
            get => _cellObjectToSpawn;
            set
            {
                if (_cellObjectToSpawn != value)
                {
                    Debug.Log($"cellObjectToSpawn changed from {_cellObjectToSpawn} to {value}");
                    _cellObjectToSpawn = value;
                }
            }
        }

        public float _yRotation;
        public float yRotation
        {
            get => _yRotation;
            set
            {
                if (Mathf.Abs(_yRotation - value) > Mathf.Epsilon)
                {
                    Debug.Log($"yRotation changed from {_yRotation} to {value}");
                    _yRotation = value;
                }
            }
        }
    }
}
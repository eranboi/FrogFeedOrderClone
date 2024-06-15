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
        public CellObject cellObjectToSpawn;
        public float yRotation;
    }
}
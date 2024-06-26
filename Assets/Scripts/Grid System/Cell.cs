using System.Collections.Generic;
using System.Linq;
using Entities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Grid_System
{
    public class Cell : MonoBehaviour
    {
        public List<CellObject> objectStackList = new();

        [SerializeField] private List<GrapeCellObject> allGrapes;

        

        [Button]
        public void FixHierarchy()
        {
            foreach (var cellObject in objectStackList)
            {
                cellObject.transform.SetParent(transform);
            }
        }
        private void Start()
        {
            GenerateContent();
        }

        public void GenerateContent()
        {
            if (objectStackList.Count == 0) return;
            var topCellObject = objectStackList[^1];

            if (topCellObject is not TileCellObject) return;
            
            var tileCellObject = topCellObject as TileCellObject;
            
            var color = topCellObject.cellObjectColor;

            var cellObjectSpawnable = tileCellObject.spawnableCellObject;
            var toSpawn = cellObjectSpawnable.cellObjectToSpawn;
            
            
            
            if (toSpawn == null)
            {
                toSpawn = allGrapes.Find(x => x.cellObjectColor == color);
            }
            else
            {
                toSpawn.cellObjectColor = topCellObject.cellObjectColor;
            }
            
            var yPosition = 0f;
            
            var cumulativeHeight = objectStackList.Sum(cellObject => cellObject.height);

            yPosition = cumulativeHeight + toSpawn.height / 2;
            var newPos = new Vector3(transform.position.x, yPosition, transform.position.z);
            var instantiatedCellObject = Instantiate(toSpawn.gameObject, newPos, Quaternion.Euler(0, cellObjectSpawnable.yRotation, 0));
            objectStackList.Add(instantiatedCellObject.GetComponent<CellObject>());
        }

        private void ArrangePositions()
        {
            float cumulativeHeight = 0f;

            for (var i = 0; i < objectStackList.Count; i++)
            {
                var cellObject = objectStackList[i];

                var currentPos = transform.position;

                // Calculate the position taking into account the height of the current cell object
                cellObject.transform.position =
                    new Vector3(currentPos.x, currentPos.y + cumulativeHeight + cellObject.height / 2f, currentPos.z);

                // Increment the cumulative height by the full height of the current cell object
                cumulativeHeight += cellObject.height;

                // Push the cell object onto the stack
            }
        }




        public void AddCellObject(CellObject cellObject)
        {
            objectStackList.Add(cellObject);

            ArrangePositions();
        }

        public void ClearCell()
        {
            
            for (var i = objectStackList.Count - 1; i >= 0; i--)
            {
                var t = objectStackList[i];
                if (t == null)
                {
                    objectStackList.RemoveAt(i);
                    continue;
                }
                
                #if UNITY_EDITOR
                DestroyImmediate(t.gameObject);
                #else
                Destroy(t.gameObject);
                #endif
            }
            objectStackList.Clear();
        }

        public void ClearEmptyItems()
        {
            for (int i = 0; i < objectStackList.Count; i++)
            {
                var cellObj = objectStackList[i];
                if (cellObj == null)
                {
                    objectStackList.RemoveAt(i);
                }
            }
        }

    }

    
}
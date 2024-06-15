using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Grid_System;
using Managers;
using UnityEngine;
using Grid = Grid_System.Grid;

namespace Entities
{
    public class TongueController : MonoBehaviour
    {
        [SerializeField] private float tongueSpeed = .25f;
        [SerializeField] public TongueStatus tongueStatus;

        private CellObjectColor frogColor;
        private LineRenderer lineRenderer;
        [SerializeField] private Vector3 direction;
        private Vector3 currentTonguePosition;

        private List<Transform> collectedGrapes = new();
        private List<Cell> processedCells = new();

        public List<Vector3> path;
        
        public Action TongueIsIdle;

        private GridController gridController;
        private Grid grid;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            frogColor = transform.parent.GetComponent<CellObject>().cellObjectColor;
            transform.position = transform.parent.position;
            lineRenderer.SetPosition(0, transform.parent.position);
            lineRenderer.SetPosition(1, transform.parent.position);

            gridController = GridController.Instance;
            grid = gridController.grid;
        }

        private void Update()
        {
            if (tongueStatus == TongueStatus.Idle) return;

            currentTonguePosition = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            switch (tongueStatus)
            {
                case TongueStatus.GoingOut:
                    MoveTongue(tongueSpeed);
                    break;
                case TongueStatus.GoingIn:
                    MoveTongue(tongueSpeed);
                    GrapeMovement();
                    CheckTongueIdle();
                    break;
            }
        }

        private void MoveTongue(float speed)
        {
            var newX = currentTonguePosition.x + speed * direction.x * Time.deltaTime;
            var newZ = currentTonguePosition.z + speed * direction.z * Time.deltaTime;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(newX, currentTonguePosition.y, newZ));
        }

        private void GrapeMovement()
        {
            for (var i = 0; i < collectedGrapes.Count; i++)
            {
                var grape = collectedGrapes[i];
                if (grape == null)
                {
                    collectedGrapes.RemoveAt(i);
                    continue;
                }
                else
                {
                    var index = path.FindIndex(pos => Math.Abs(pos.x - grape.position.x) < .2f&& Math.Abs(pos.z - grape.position.z) < .2f);
                    List<Vector3> newPath = new ();
                    for (var j = index; j > 0; j--)
                    {
                        newPath.Add(path[j]);
                    }
                    newPath.Add(transform.position);
                    grape.DOPath(newPath.ToArray(), newPath.Count * .2f).SetEase(Ease.Linear).OnComplete(()=> Destroy(grape.gameObject));
                    
                    collectedGrapes.RemoveAt(i);
                }
            }
        }

        private void CheckTongueIdle()
        {
            if (Vector3.Distance(currentTonguePosition, transform.position) < .2f)
            {
                tongueStatus = TongueStatus.Idle;
                TongueIsIdle?.Invoke();
                GameManager.OnClickProcessed?.Invoke();
                collectedGrapes.Clear();
            }
        }

        public void SetTongueDirection(Vector3 newDirection, TongueStatus newTongueStatus)
        {
            direction = newDirection;
            tongueStatus = newTongueStatus;
        }

        public void AddGrape(Transform grapeTransform)
        {
            if (grapeTransform != null && !collectedGrapes.Contains(grapeTransform))
            {
                collectedGrapes.Add(grapeTransform);
            }
        }

        public void DropGrapes()
        {
            collectedGrapes.Clear();
            processedCells.Clear();
        }

        public void ChangeDirection(Vector3 newDirection)
        {
            if (tongueStatus == TongueStatus.GoingOut)
            {
                var positionCount = lineRenderer.positionCount;
                positionCount += 1;
                lineRenderer.positionCount = positionCount;
                lineRenderer.SetPosition(positionCount - 1, lineRenderer.GetPosition(positionCount - 2));
            }
            else
            {
                var positionCount = lineRenderer.positionCount;
                positionCount -= 1;
                lineRenderer.positionCount = positionCount;
            }
            direction = newDirection;
            direction.y = 0;
        }

        public void ProcessCells()
        {
            StartCoroutine(ProcessCellsCoroutine());
        }

        private IEnumerator ProcessCellsCoroutine()
        {
            if (processedCells.Count == 0) yield break;

            var (selfX, selfY) = grid.GetGridIndex(transform.parent.position);
            var selfCell = grid.GetCell(selfX, selfY);
            if (selfCell == null) yield break;

            processedCells.Insert(0, selfCell);

            var thisFrog = transform.parent.GetComponent<CellObject>();
            for (var i = processedCells.Count - 1; i >= 0; i--)
            {
                var cell = processedCells[i];
                cell.ClearEmptyItems();

                var cellObject = cell.objectStackList.Count > 0 ? cell.objectStackList[^1] : null;

                if (cellObject is ArrowCellObject arrowCellObject)
                {
                    arrowCellObject.ShrinkAndDestroy();
                    cell.objectStackList.Remove(arrowCellObject);
                    cellObject = cell.objectStackList[^1];
                }
                if (cellObject == thisFrog && cell.objectStackList.Count > 1)
                {
                    cellObject = cell.objectStackList[^2];
                }

                if (cellObject != null)
                {
                    cellObject.ShrinkAndDestroy();
                    cell.objectStackList.Remove(cellObject);
                    cell.ClearEmptyItems();
                    cell.GenerateContent();
                    selfCell.objectStackList.Remove(thisFrog);
                }
            }
            thisFrog.ShrinkAndDestroy();
            GameManager.OnClickProcessed?.Invoke();
            
            

            processedCells.Clear();
            TongueIsIdle?.Invoke();
        }

        public void AddProcessedCell(Cell cell)
        {
            if (!processedCells.Contains(cell))
            {
                processedCells.Add(cell);
            }
        }

        public void StartCollecting()
        {
            StartCoroutine(CollectRoutine());
        }

        private IEnumerator CollectRoutine()
        {
            grid = gridController.grid;
            var (x, y) = grid.GetGridIndex(lineRenderer.GetPosition(lineRenderer.positionCount - 1));
            direction = -transform.parent.forward;
            x += (int)direction.x;
            y += (int)direction.z;

            var (selfX, selfY) = grid.GetGridIndex(transform.position);

            SetTongueDirection(direction, TongueStatus.GoingOut);
            TongueIsIdle += ProcessCells;

            while (tongueStatus is not TongueStatus.Idle)
            {
                if (!grid.InsideBounds(x, y))
                {
                    SetTongueDirection(-direction, TongueStatus.GoingIn);
                    x += (int)direction.x;
                    y += (int)direction.z;
                    continue;
                }

                yield return new WaitForSeconds(.25f);

                var currentCell = grid.GetCell(x, y);
                
                if (tongueStatus == TongueStatus.GoingOut)
                {
                    if (currentCell == null || currentCell.objectStackList.Count == 0)
                    {
                        SetTongueDirection(-direction, TongueStatus.GoingIn);
                        continue;
                    }
                
                    if(currentCell.objectStackList.Count == 0)
                    {
                        continue;
                    }
                }
                
                if (tongueStatus == TongueStatus.GoingIn)
                {
                    if(x == selfX && y == selfY) continue;
                    if(currentCell == null) continue;
                    if(currentCell.objectStackList.Count == 0) continue;
                }

                if (tongueStatus == TongueStatus.Idle) break;

                var topObject = currentCell.objectStackList[^1];
                if (topObject == null)
                {
                    if (tongueStatus == TongueStatus.GoingOut)
                    {
                        SetTongueDirection(-direction, TongueStatus.GoingIn);
                        continue;
                    }
                }

                switch (topObject)
                {
                    case GrapeCellObject grapeCellObject when grapeCellObject.cellObjectColor == frogColor:

                        if (tongueStatus == TongueStatus.GoingOut)
                        {
                            AddGrape(grapeCellObject.transform);
                            AddProcessedCell(currentCell);
                            grapeCellObject.Pop();
                        }
                        break;

                    case GrapeCellObject grapeCellObject:

                        DropGrapes();
                        SetTongueDirection(-direction, TongueStatus.GoingIn);
                        grapeCellObject.Pop();
                        grapeCellObject.FlashRed();


                        break;

                    case FrogCellObject frogCellObject:
                        if (frogCellObject == transform.parent.GetComponent<CellObject>())
                            break;


                        DropGrapes();
                        SetTongueDirection(-direction, TongueStatus.GoingIn);
                        frogCellObject.Pop();

                        break;

                    case ArrowCellObject arrowCellObject:

                        if (tongueStatus == TongueStatus.GoingOut)
                        {
                            var newDirection = arrowCellObject.transform.forward;
                            arrowCellObject.tongueOriginalDirection = direction;
                            ChangeDirection(newDirection);
                            AddProcessedCell(currentCell);
                        }
                        else if (tongueStatus == TongueStatus.GoingIn)
                        {
                            var newDirection = -arrowCellObject.tongueOriginalDirection;
                            ChangeDirection(newDirection);
                        }


                        break;

                    default:
                        SetTongueDirection(direction, TongueStatus.GoingIn);
                        break;
                }

                if(tongueStatus == TongueStatus.GoingOut)
                    path.Add(topObject.transform.position);
                
                x += (int)direction.x;
                y += (int)direction.z;

            }
            
        }
    }

    public enum TongueStatus
    {
        Idle,
        GoingOut,
        GoingIn,
    }
}

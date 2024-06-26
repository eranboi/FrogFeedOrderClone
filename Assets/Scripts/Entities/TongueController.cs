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
        [SerializeField] private float tongueSpeed = 4f;
        [SerializeField] public TongueStatus tongueStatus;

        private CellObjectColor _frogColor;
        private LineRenderer _lineRenderer;
        [SerializeField] private Vector3 direction;
        private Vector3 _currentTonguePosition;

        [SerializeField] private List<Transform> _collectedGrapes = new();
        [SerializeField] private List<Cell> _processedCells = new();

        public List<Vector3> path;
        
        public Action TongueIsIdle;

        private GridController _gridController;
        private Grid _grid;
        
        private int _grapesInMotionCount = 0;


        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        private void Start()
        {
            _frogColor = transform.parent.GetComponent<CellObject>().cellObjectColor;
            transform.position = transform.parent.position;
            _lineRenderer.SetPosition(0, transform.parent.position);
            _lineRenderer.SetPosition(1, transform.parent.position);

            _gridController = GridController.Instance;
            _grid = _gridController.Grid;
        }

        private void Update()
        {
            if (tongueStatus == TongueStatus.Idle) return;

            _currentTonguePosition = _lineRenderer.GetPosition(_lineRenderer.positionCount - 1);
            switch (tongueStatus)
            {
                case TongueStatus.GoingOut:
                case TongueStatus.GoingIn:
                    MoveTongue(tongueSpeed);
                    if (tongueStatus == TongueStatus.GoingIn)
                    {
                        if(_collectedGrapes.Count > 0)
                            GrapeMovement();
                        if(_grapesInMotionCount == 0)
                            CheckTongueIdle();
                    }
                    break;
            }
        }

        private void MoveTongue(float speed)
        {
            var newX = _currentTonguePosition.x + speed * direction.x * Time.deltaTime;
            var newZ = _currentTonguePosition.z + speed * direction.z * Time.deltaTime;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, new Vector3(newX, _currentTonguePosition.y, newZ));
        }

        private void GrapeMovement()
        {
            for (var i = 0; i < _collectedGrapes.Count; i++)
            {
                var grape = _collectedGrapes[i];
                if (grape == null)
                {
                    _collectedGrapes.RemoveAt(i);
                    continue;
                }
                
                var index = path.FindIndex(pos => Math.Abs(pos.x - grape.position.x) < .2f && Math.Abs(pos.z - grape.position.z) < .2f);
                List<Vector3> newPath = new();
                for (var j = index; j > 0; j--)
                {
                    newPath.Add(path[j]);
                }
                newPath.Add(transform.position -transform.forward * .45f);
                
                grape.DOPath(newPath.ToArray(), newPath.Count * 1/ (tongueSpeed)).SetEase(Ease.Linear).OnComplete(() =>
                {
                    Destroy(grape.gameObject);
                    _grapesInMotionCount--;
                    if (_grapesInMotionCount <= 0)
                    {
                        CheckTongueIdle();
                    }
                });

                _collectedGrapes.RemoveAt(i);
                _grapesInMotionCount++;
            }
        }

        private void CheckTongueIdle()
        {
            if (Vector3.Distance(_currentTonguePosition, transform.position) > .5f) return;
            tongueStatus = TongueStatus.Idle;
            _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, transform.position);
            TongueIsIdle?.Invoke();
            _collectedGrapes.Clear();
            path.Clear();

            TongueLogger("Tongue Idle");
        }

        public void SetTongueDirection(Vector3 newDirection, TongueStatus newTongueStatus)
        {
            direction = newDirection;
            tongueStatus = newTongueStatus;
        }

        public void AddGrape(Transform grapeTransform)
        {
            if (grapeTransform != null && !_collectedGrapes.Contains(grapeTransform))
            {
                _collectedGrapes.Add(grapeTransform);
                grapeTransform.GetComponent<CellObject>().isPickedUp = true;
            }
        }

        public void DropGrapes()
        {
            foreach (var grapeTransform in _collectedGrapes)
            {
                grapeTransform.GetComponent<CellObject>().isPickedUp = false;
            }
            
            _collectedGrapes.Clear();
            _processedCells.Clear();
        }

        public void ChangeDirection(Vector3 newDirection)
        {
            var positionCount = _lineRenderer.positionCount;
            if (tongueStatus == TongueStatus.GoingOut)
            {
                _lineRenderer.positionCount = ++positionCount;
                _lineRenderer.SetPosition(positionCount - 1, _lineRenderer.GetPosition(positionCount - 2));
            }
            else
            {
                _lineRenderer.positionCount = --positionCount;
            }
            direction = newDirection;
            direction.y = 0;
        }

        private void ProcessCells()
        {
            StartCoroutine(ProcessCellsCoroutine());
        }
        
        private IEnumerator ProcessCellsCoroutine()
        {
            if (_processedCells.Count == 0)
            {
                GameManager.OnClickProcessed?.Invoke();
                yield break;
            }

            var (selfX, selfY) = _grid.GetGridIndex(transform.parent.position);
            var selfCell = _grid.GetCell(selfX, selfY);
            if (selfCell == null)
            {
                GameManager.OnClickProcessed?.Invoke();
                yield break;
            }
            GetComponentInParent<FrogCellObject>().isDone = true;
            yield return null;
            yield return null;
            
            //Add the tile cell that the frog stands on.
            _processedCells.Insert(0, selfCell);

            var thisFrog = transform.parent.GetComponent<CellObject>();
            for (var i = _processedCells.Count - 1; i >= 0; i--)
            {
                var cell = _processedCells[i];
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
                    TongueLogger("Generate content.");
                    selfCell.objectStackList.Remove(thisFrog);
                }
            }
            thisFrog.ShrinkAndDestroy();
            GameManager.OnClickProcessed?.Invoke();

            _processedCells.Clear();
            // TongueIsIdle?.Invoke();
        }

        public void AddProcessedCell(Cell cell)
        {
            if (!_processedCells.Contains(cell))
            {
                _processedCells.Add(cell);
            }
        }

        public void StartCollecting()
        {
            if(tongueStatus != TongueStatus.Idle) return;
            StartCoroutine(CollectRoutine());
        }

        private IEnumerator CollectRoutine()
        {
            _grid = _gridController.Grid;
            var (x, y) = _grid.GetGridIndex(_lineRenderer.GetPosition(_lineRenderer.positionCount - 1));
            direction = -transform.parent.forward;
            x += (int)direction.x;
            y += (int)direction.z;

            var (selfX, selfY) = _grid.GetGridIndex(transform.position);

            SetTongueDirection(direction, TongueStatus.GoingOut);
            TongueIsIdle += ProcessCells;

            while (tongueStatus != TongueStatus.Idle)
            {
                // If grid is not in bounds, we'll wait for half the normal time
                // for visuals to be accurate
                if (!_grid.InsideBounds(x, y))
                {
                    yield return new WaitForSeconds(1/ (tongueSpeed ));

                    TongueLoggerErr("!_grid.InsideBounds(x, y)", gameObject);
                    SetTongueDirection(-direction, TongueStatus.GoingIn);
                    x += (int)direction.x;
                    y += (int)direction.z;
                    continue;
                }
                
                yield return new WaitForSeconds(1/tongueSpeed);
                
                var currentCell = _grid.GetCell(x, y);

                switch (tongueStatus)
                {
                    case TongueStatus.GoingOut when (currentCell == null || currentCell.objectStackList.Count == 0):
                        TongueLoggerErr($"currentCell == null : {currentCell == null}", gameObject);
                        TongueLoggerErr($"currentCell.objectStackList.Count == 0 : {currentCell.objectStackList.Count == 0}", gameObject);
                        DropGrapes();
                        SetTongueDirection(-direction, TongueStatus.GoingIn);
                        continue;
                    case TongueStatus.GoingIn when (x == selfX && y == selfY || currentCell == null || currentCell.objectStackList.Count == 0):
                        continue;
                }

                if (tongueStatus == TongueStatus.Idle) break;

                var topObject = currentCell.objectStackList[^1];
                if (topObject == null && tongueStatus == TongueStatus.GoingOut)
                {
                    TongueLoggerErr("topObject is null", currentCell.gameObject);
                    DropGrapes();
                    SetTongueDirection(-direction, TongueStatus.GoingIn);
                    continue;
                }

                switch (topObject)
                {
                    case GrapeCellObject grapeCellObject when grapeCellObject.cellObjectColor == _frogColor:
                        if (tongueStatus == TongueStatus.GoingOut)
                        {
                            grapeCellObject.Pop();
                            AddGrape(grapeCellObject.transform);
                            AddProcessedCell(currentCell);
                            TongueLoggerSucc("Added grape", grapeCellObject.gameObject);
                        }
                        break;

                    case GrapeCellObject grapeCellObject:
                        if (tongueStatus == TongueStatus.GoingOut)
                        {
                            TongueLoggerErr("Touched a different grape.", grapeCellObject.gameObject);
                            DropGrapes();
                            SetTongueDirection(-direction, TongueStatus.GoingIn);
                            grapeCellObject.Pop();
                            grapeCellObject.FlashRed();
                        }
                        break;

                    case FrogCellObject frogCellObject:
                        if (frogCellObject != transform.parent.GetComponent<CellObject>() && tongueStatus == TongueStatus.GoingOut)
                        {
                            TongueLoggerErr("Touched a different frog.", frogCellObject.gameObject);

                            DropGrapes();
                            SetTongueDirection(-direction, TongueStatus.GoingIn);
                            frogCellObject.Pop();
                        }
                        break;

                    case ArrowCellObject arrowCellObject when arrowCellObject.cellObjectColor == _frogColor:
                        switch (tongueStatus)
                        {
                            case TongueStatus.GoingOut:
                            {
                                TongueLogger($"Touched an arrow {tongueStatus}");
                                var newDirection = arrowCellObject.transform.forward;
                                arrowCellObject.tongueOriginalDirection = direction;
                                ChangeDirection(newDirection);
                                AddProcessedCell(currentCell);
                                break;
                            }
                            case TongueStatus.GoingIn:
                            {
                                TongueLogger($"Touched an arrow {tongueStatus}");
                                var newDirection = -arrowCellObject.tongueOriginalDirection;
                                ChangeDirection(newDirection);
                                break;
                            }
                        }
                        break;

                    default:
                        TongueLoggerErr("Touched something and going in.");
                        DropGrapes();
                        SetTongueDirection(-direction, TongueStatus.GoingIn);
                        break;
                }

                if (tongueStatus == TongueStatus.GoingOut)
                {
                    path.Add(topObject.transform.position);
                }

                x += (int)direction.x;
                y += (int)direction.z;
            }
        }

        private void TongueLogger(string msg, GameObject go = null)
        {
            Debug.Log($"<color=cyan>{msg}</color>", go);

        }
        private void TongueLoggerErr(string msg, GameObject go = null)
        {
            Debug.Log($"<color=red>{msg}</color>", go);
        }
        private void TongueLoggerSucc(string msg, GameObject go = null)
        {
            Debug.Log($"<color=green>{msg}</color>", go);

        }
    }

    public enum TongueStatus
    {
        Idle,
        GoingOut,
        GoingIn,
    }
}

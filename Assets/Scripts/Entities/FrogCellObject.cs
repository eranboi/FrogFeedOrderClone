using System.Collections;
using Managers;
using UnityEngine;

namespace Entities
{
    public class FrogCellObject : CellObject
    {
        private TongueController _tongue;
        private bool _isClickable;
        public bool isDone = false;
        
        private void Awake()
        {
            _tongue = GetComponentInChildren<TongueController>();
            _isClickable = true;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                HandleMouseInput(Input.mousePosition);
            }
        }

        private void HandleMouseInput(Vector2 mousePosition)
        {
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform)
            {
                if (!_isClickable) return;
                OnMouseClick();
            }
        }

        private void OnMouseClick()
        {
            GameManager.OnFrogClick?.Invoke();
            _tongue.StartCollecting();
            _tongue.TongueIsIdle += ResetIsClickable;
            _isClickable = false;
        }

        private void ResetIsClickable()
        {
            _tongue.TongueIsIdle -= ResetIsClickable;
            if (isDone) return;
            _isClickable = true;

        }
    }
}
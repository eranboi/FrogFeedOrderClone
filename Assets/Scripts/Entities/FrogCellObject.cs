using System.Collections;
using Managers;
using UnityEngine;

namespace Entities
{
    public class FrogCellObject : CellObject
    {
        private TongueController _tongue;

        private void Awake()
        {
            _tongue = GetComponentInChildren<TongueController>();
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
                OnMouseClick();
            }
        }

        private void OnMouseClick()
        {
            GameManager.OnFrogClick?.Invoke();
            _tongue.StartCollecting();
        }
    }
}
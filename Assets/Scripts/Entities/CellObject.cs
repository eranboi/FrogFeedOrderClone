using System;
using DG.Tweening;
using Grid_System;
using UnityEngine;

namespace Entities
{
    public abstract class CellObject : MonoBehaviour
    {
        public bool isPickedUp;
        public CellObjectColor cellObjectColor;
        public float height;

        private void Start()
        {
            isPickedUp = false;
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, .2f);
        }

        public void ShrinkAndDestroy()
        {
            transform.DOScale(Vector3.zero, .2f).OnComplete(() =>
            {
                Destroy(gameObject);
                
            });
        }

        public void Pop()
        {
            if (isPickedUp) return;
            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 1.1f, .25f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * height);
        }
    }

    public enum CellObjectColor
    {
        Blue,
        Green,
        Purple,
        Red,
        Yellow
    }
    
}
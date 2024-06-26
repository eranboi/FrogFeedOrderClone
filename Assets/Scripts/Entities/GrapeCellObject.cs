using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Entities
{
    public class GrapeCellObject : CellObject
    {
        public void FlashRed()
        {
            if (isPickedUp) return;
            var material = GetComponent<MeshRenderer>().material;
            material.DOColor(Color.red, .15f).OnComplete(() =>
            {
                material.DOColor(Color.white, .15f);
            });
        }
    }
}
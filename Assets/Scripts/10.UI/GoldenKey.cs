using System;
using UnityEngine;
using DG.Tweening;
using Study.Utilities;
using UnityEngine.InputSystem;


namespace BlueMarble
{
    public class GoldenKey:MonoBehaviour
    {
        private bool flipped = false;


        private void Update()
        {
            if (SimpleInput.GetKeyDown(Key.Digit1))
            {
                Flip();
            }
        }


        private void Flip()
        {
            flipped = !flipped;
            transform.DORotate(new(0, flipped ? 0f : 180f, 0), 0.25f);
        }
    }
}
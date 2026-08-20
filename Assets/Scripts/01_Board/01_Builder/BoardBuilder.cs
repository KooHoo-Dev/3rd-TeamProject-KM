using System;
using UnityEngine;

namespace BlueMarble.Board.Builder
{
    public class BoardBuilder : MonoBehaviour
    {
        private DataManager data;

        private void Start()
        {
            data = DataManager.Instance;
        }
    }
}
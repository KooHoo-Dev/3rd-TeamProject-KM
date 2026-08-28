using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble.TestCode
{
    public class BoardRoundTest : MonoBehaviour
    {
        private RoundManager Round => RoundManager.Instance;

        [Button]
        private void InitTest()
        {
            var joined = PlayerRegistry.All
                .Select(pr => pr.Id)
                .ToList();

            Round.Initialize(joined);
        }
        
        [Button]
        private async UniTaskVoid RoundTestAsync()
        {
            await Round.ExecuteRound();
        }
    }
}
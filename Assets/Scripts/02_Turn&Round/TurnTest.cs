using System.Collections.Generic;
using System.Threading;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Epic2
{
    public class TurnTest : MonoBehaviour
    {
        [SerializeField]
        public Dictionary<int, GameObject> player = new Dictionary<int, GameObject>();

        public GameObject[] players = new GameObject[4];
        
        public TurnManager turnManager;
        public DiceManager diceManager;

        
        
        private static int myIndex = 0;

        private void Start()
        {
            for (int i = 0; i < players.Length; i++)
            {
                player.Add(i,players[i]);
            }
            // 지금 InitializePlayers 이 함수는 List로 받는데 여기서 하는거는 Dictionary야.
            
            List<int> joinedPlayers = new List<int>(player.Keys);
            
            turnManager.InitializePlayers(joinedPlayers);
            
            
        }
        [Button]
        public void OnClickStart()
        {
            turnManager.TurnDeadline().Forget();
        }
        [Button]
        public void OnClickNextTurn()
        {
            turnManager.AdvanceTurn();
        }
        [Button]
        public void OnRollDice()
        {
            diceManager.RollDice(myIndex);
        }
    }
}
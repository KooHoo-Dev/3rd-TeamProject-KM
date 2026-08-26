using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Epic2
{
    public class DiceManager : MonoBehaviour
    {
        // 주사위 굴림 연출과 난수 생성
        public event Action<int, int, int> OnDiceRolled;
        
        public TurnManager turnManager;
        
        public void RollDice(int playerId)
        {
            if (turnManager.IsMyTurn(playerId) == false)
            {
                Debug.Log($"현재 순서는 {turnManager.GetCurrentPlayerID()}. 본인은 {playerId}");
                return;
            } 
            
            int dice1 = Random.Range(1, 7);
            int dice2 = Random.Range(1, 7);
            
            SendToDiceResult(playerId, dice1, dice2);
        }

        // 테스트 더미 서버 전송 함수
        private void SendToDiceResult(int playerId, int dice1, int dice2)
        {
            Debug.Log($"Player {playerId} rolls {dice1} and {dice2}");
            
            RollDiceReceived(playerId, dice1, dice2);
        }

        // 수신부에서 호출할 함수
        public void RollDiceReceived(int playerId, int dice1, int dice2)
        {
            OnDiceRolled?.Invoke(playerId, dice1, dice2);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Epic2
{
    public class BoardManager : MonoBehaviour
    {
        // 서버로부터 주사위 값이나 강제 이동 명령을 수신하면, 실제로 말을 목적지 타일로 움직이는 역할
        
        // BoardManager가 활성화 될때 DiceManager의 주사위 결고 이벤트에 내 함수를 등록한다
        // 주사위 결과 이벤트가 발생하면, 등록한 함수를 실행
        public const int TILE_COUNT = 40;
        
        //public DiceManager diceManager;
        
        private Dictionary<int, int> playerPosition = new Dictionary<int, int>();

        private void OnEnable()
        {
            //diceManager.OnDiceRolled += HandleDiceResult;
        }

        private void OnDisable()
        {
            //diceManager.OnDiceRolled -= HandleDiceResult;
        }

        private void HandleDiceResult(int playerId, int dice1, int dice2)
        {
            int moveDistance = dice1 + dice2;
            
            Debug.Log($"{playerId}: Dice Result: {dice1}, {dice2}: {moveDistance}");
            
            MovePosition(playerId, moveDistance);
        }

        private void MovePosition(int playerId, int moveDistance)
        {
            if (playerPosition.ContainsKey(playerId)== false) playerPosition.Add(playerId, 0);
            
            int currentPosition = playerPosition[playerId];
            
            int nextPosition = moveDistance + currentPosition;
            
            if(nextPosition>TILE_COUNT) nextPosition -= TILE_COUNT;
            
            playerPosition[playerId] = nextPosition;
            Debug.Log($"{playerId}: {currentPosition}에서 {nextPosition}으로 이동");
            
            // 실제 이동 로직
        }

    }
}
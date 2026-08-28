using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;

namespace BlueMarble
{
    public class TurnManager : MonoBehaviour
    {
        private readonly struct DiceResult
        {
            public readonly int D1;
            public readonly int D2;
            
            public int Total => D1 + D2;

            public DiceResult(int d1, int d2)
            {
                D1 = d1;
                D2 = d2;
            }
        }
        
        [Title("Settings")]
        [SerializeField] private float turnTime = 30f;

        private CancellationTokenSource timerCTS;
        private CancellationTokenSource turnCTS;
        
        public async UniTask ExecuteTurn(string playerId)
        {
            IPlayer player = PlayerRegistry.GetPlayer(playerId);
            
            // 주사위 굴림
            DiceResult diceResult = await RollDice();

            // 주사위 수 만큼 플레이어 이동
            for (int i = 0; i < diceResult.Total; i++)
            {
                Tile reached = await player.MoveNext();

                bool isLanding = i == diceResult.Total - 1;
                
                // 도착 칸이면 도착 행동 처리
                // 만약 지나가는 칸이면 지나감 행동 처리
                if (isLanding) await ResolveTileLanding(player, reached);
                else await ResolveTilePassing(player, reached);
            }
        }

        private async UniTaskVoid ExecuteTimer(float timeLimit, CancellationToken token)
        {
            await CustomUniTask.DelaySeconds(timeLimit, cancellationToken: token);
            turnCTS?.Cancel();
        }

        private async UniTask<DiceResult> RollDice()
        {
            int d1 = Random.Range(1, 7);
            int d2 = Random.Range(1, 7);
            
            // 여기서 뭔가 연출 대기? 가 들어가겠지만
            // 일단은 생략
            await UniTask.CompletedTask;
            
            return new DiceResult(d1, d2);
        }

        private async UniTask ResolveTileLanding(IPlayer player, Tile tile)
        {
            Debug.Log($"[BoardRouteTest] Player {player.Id} landed on {tile.SpaceDef.Name}.");
            Debug.Log($"[BoardRouteTest] {tile.SpaceDef.Name} landing effect: Triggered");
            
            await UniTask.CompletedTask;
        }

        private async UniTask ResolveTilePassing(IPlayer player, Tile tile)
        {
            Debug.Log($"[BoardRouteTest] Player {player.Id} passed {tile.SpaceDef.Name}.");
            Debug.Log($"[BoardRouteTest] {tile.SpaceDef.Name} pass effect: " +
                $"{(tile.SpaceDef.IsActiveOnPass ? "Triggered" : "Skipped")}.");

            if (tile.SpaceDef.IsActiveOnPass)
            {
                await CustomUniTask.DelaySeconds(1.5f);
                Debug.Log($"[BoardRouteTest] {tile.SpaceDef.Name}'s effect is done.");
            }
        }
    }
}
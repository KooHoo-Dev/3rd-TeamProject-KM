using System;
using System.Collections.Generic;
using System.Linq;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble
{
    public class GoldCardManager : SingletonBase<GoldCardManager>
    {
        [Title("Components")]
        [SerializeField] private GoldCard card;
        
        private Dictionary<GoldCardScopeType, IGoldCardScope> scopes = new();
        private Dictionary<GoldCardPatternType, IGoldCardPattern> patterns = new();
        
        private DataManager Data => DataManager.Instance;
        private RoomClient Client => RoomClient.Instance;

        #region INITIALIZER
        
        protected override void OnInitialize()
        {
            PopulateByEnum(scopes, GoldCardScopeFactory.Create);
            PopulateByEnum(patterns, GoldCardPatternFactory.Create);
        }
        
        private static void PopulateByEnum<TEnum, TValue>(
            Dictionary<TEnum, TValue> dict, Func<TEnum, TValue> factory) 
            where TEnum : struct, Enum
        {
            TEnum[] values = (TEnum[])Enum.GetValues(typeof(TEnum));

            for (int i = 0; i < values.Length; i++)
            {
                TEnum value = values[i];
                dict[value] = factory(value);
            }
        }
        
        #endregion
        
        public async UniTask DrawAndUseAsync(string playerId)
        {
            await Data.EnsureDataLoaded();

            // 카드를 뽑아서 카드에 해당 내용 전달
            GoldCardDef drawn = await WaitUntilCardDrawnAsync(playerId);
            card.Initialize(drawn);

            // 카드를 뽑은 플레이어(현재 턴 플레이어)가 뽑았다는 메시지를 서버에 전달하고
            // 해당 사항을 전달 받은 서버가 모든 플레이어에게 뽑았음을 알리기 전까지 연출 상태 대기
            // 뽑았음을 전달 받으면 card.Flip을 이용하여 카드 연출을 실행함
            // 모든 플레이어의 연출이 끝날 때 까지 대기
            await WaitUntilCardAnimationFinishedAsync();
            
            // 이후, 해당 턴의 플레이어의 클라이언트에서만 카드의 효과를 실행
            // 효과 적용이 끝난 이후 골드 카드 효과를 마무리 짓는다
        }

        private async UniTask<GoldCardDef> WaitUntilCardDrawnAsync(string playerId)
        {
            UniTaskCompletionSource<GoldCardDef> completionSource = new();
            Client.OnGoldCardDrawn += HandleGoldCardDrawn;

            GoldCardDef drawn;
            
            try
            {
                int[] cardIds = Data.GoldCards.GetIds().ToArray();
                Client.RequestDrawGoldCard(Client.TurnId, cardIds);
                
                drawn = await completionSource.Task;
            }
            finally
            {
                Client.OnGoldCardDrawn -= HandleGoldCardDrawn;
            }

            Debug.Log($"[GoldCardManager] {playerId} drew Gold Card " +
                $"{drawn.Id}: {drawn.Description}");
            
            return drawn;
            
            void HandleGoldCardDrawn(User user, int cardId)
            {
                if (user.Id != playerId) return;
                
                GoldCardDef def = Data.GetGoldCardDef(cardId);
                completionSource.TrySetResult(def);
            }
        }

        private async UniTask WaitUntilCardAnimationFinishedAsync()
        {
            await UniTask.CompletedTask;
        }

        private UniTask ApplyCardEffectAsync(string playerId, GoldCardDef drawn)
        {
            IGoldCardScope scope = scopes[drawn.ScopeType];
            string[] targetIds = scope.GetTargetIds(playerId);

            IGoldCardPattern pattern = patterns[drawn.PatternType];

            return pattern.Apply(
                targetIds,
                drawn.GoldAmount,
                drawn.DropTableId,
                drawn.TileId);
        }
    }
}
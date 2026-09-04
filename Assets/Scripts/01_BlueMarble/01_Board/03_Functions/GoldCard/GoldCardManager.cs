using System;
using System.Collections.Generic;
using System.Linq;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using UnityEngine;

namespace BlueMarble
{
    public class GoldCardManager : SingletonBase<GoldCardManager>
    {
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
            
            UniTaskCompletionSource<GoldCardDef> completionSource = new();

            Client.OnGoldCardDrawn += HandleGoldCardDrawn;

            try
            {
                int[] cardIds = Data.GoldCards.GetIds().ToArray();
                Client.RequestDrawGoldCard(Client.TurnId, cardIds);
                
                GoldCardDef drawn = await completionSource.Task;

                Debug.Log($"[GoldCardManager] {playerId} drew Gold Card " +
                          $"{drawn.Id}: {drawn.Description}");
                
                await ApplyCardEffectAsync(playerId, drawn);
            }
            finally
            {
                Client.OnGoldCardDrawn -= HandleGoldCardDrawn;
            }
            
            void HandleGoldCardDrawn(User user, int cardId)
            {
                if (user.Id != playerId) return;
                
                GoldCardDef def = Data.GetGoldCardDef(cardId);
                completionSource.TrySetResult(def);
            }
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
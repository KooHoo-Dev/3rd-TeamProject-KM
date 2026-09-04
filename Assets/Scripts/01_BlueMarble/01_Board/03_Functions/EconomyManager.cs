using System;
using System.Collections.Generic;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using UnityEngine;

namespace BlueMarble
{
    public class EconomyManager : SingletonBase<EconomyManager>
    {
        private readonly Dictionary<string, int> golds = new();

        public event Action OnUpdated;
        
        private RoomClient Client => RoomClient.Instance;

        private void Start()
        {
            Client.OnEconomyUpdated += HandleEconomyUpdated;
        }

        private void OnDisable()
        {
            Client.OnEconomyUpdated -= HandleEconomyUpdated;
        }

        private void HandleEconomyUpdated(UserEconomy[] players)
        {
            golds.Clear();

            for (int i = 0; i < players.Length; i++)
            {
                UserEconomy economy = players[i];
                golds[economy.UserId] = economy.Gold;
            }
            
            OnUpdated?.Invoke();
        }

        public bool TryGetGold(string playerId, out int gold) 
            => golds.TryGetValue(playerId, out gold);
        
        #region GAIN_OR_SPEND_GOLD

        private void UpdateGoldTo(string playerId, int amount)
        {
            Client.RequestUpdateEconomy(new EconomyUpdate
            {
                UserId = playerId,
                Amount = amount
            });
            
            Debug.Log($"[EconomyManager] Update {amount} gold to {playerId}");
        }
        
        private async UniTask UpdateGoldToAsync(string playerId, int amount)
        {
            if (TryGetGold(playerId, out int currentGold) == false)
                return;

            int expectedGold = currentGold + amount;
            UniTaskCompletionSource completionSource = new();

            OnUpdated += HandleUpdated;

            try
            {
                UpdateGoldTo(playerId, amount);
                await completionSource.Task;
            }
            finally
            {
                OnUpdated -= HandleUpdated;
            }

            void HandleUpdated()
            {
                if (TryGetGold(playerId, out int updatedGold) == false)
                    return;

                if (updatedGold != expectedGold)
                    return;

                completionSource.TrySetResult();
            }
        }

        public UniTask GainGoldTo(string playerId, int amount) => UpdateGoldToAsync(playerId, amount);
        public UniTask LoseGoldTo(string playerId, int amount) => UpdateGoldToAsync(playerId, -amount);

        #endregion


    }
}
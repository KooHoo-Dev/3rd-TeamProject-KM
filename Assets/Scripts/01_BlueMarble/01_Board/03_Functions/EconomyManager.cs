using System;
using System.Collections.Generic;
using BlueMarble.Server;
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
        
        private void UpdateGoldTo(string[] playerId, int amount)
        {
            EconomyUpdate[] updates = new EconomyUpdate[playerId.Length];
            
            for (int i = 0; i < playerId.Length; i++)
            {
                updates[i] = new EconomyUpdate
                {
                    UserId = playerId[i],
                    Amount = amount
                };
                Debug.Log($"[EconomyManager] Update {amount} gold to {playerId[i]}");
            }
            
            Client.RequestUpdateEconomy(updates);
        }
        
        public void GainGoldTo(string playerId, int amount) => UpdateGoldTo(playerId, amount);
        public void GainGoldTo(string[] playerId, int amount) => UpdateGoldTo(playerId, amount);
        
        public void SpendGold(string playerId, int amount) => UpdateGoldTo(playerId, -amount);
        public void SpendGoldTo(string[] playerId, int amount) => UpdateGoldTo(playerId, -amount);
        
        #endregion

        
    }
}
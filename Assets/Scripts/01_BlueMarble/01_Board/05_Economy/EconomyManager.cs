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
    }
}
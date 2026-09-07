using System;
using System.Collections.Generic;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble
{
    public class ProfileManager : SingletonBase<ProfileManager>
    {
        [Title("Components")]
        [SerializeField] private Profile[] profiles;
        
        private RoomClient Client => RoomClient.Instance;
        private EconomyManager Economy => EconomyManager.Instance;
        private TerritoryManager Territory => TerritoryManager.Instance;

        private void Start()
        {
            InitProfiles();
            RefreshGoldState();

            Economy.OnUpdated += RefreshGoldState;
            Territory.OnUpdated += RefreshGoldState;
            Client.OnPlayerActivityChanged += HandlePlayerActivityChanged;
        }

        private void OnDisable()
        {
            Economy.OnUpdated -= RefreshGoldState;
            Territory.OnUpdated -= RefreshGoldState;
            Client.OnPlayerActivityChanged -= HandlePlayerActivityChanged;
        }

        private void InitProfiles()
        {
            int idx = 0;
            
            // 내 프로필은 무조건 0번 요소에 초기화
            InitProfile(profiles[idx++], Client.Me);
            
            foreach (User other in Client.Others.Values)
                InitProfile(profiles[idx++], other);

            void InitProfile(Profile profile, User user)
            {
                profile.Initialize(user);
                profile.gameObject.SetActive(true);
            }
        }

        private async void RefreshGoldState()
        {
            for (int i = 0; i < profiles.Length; i++)
            {
                Profile profile = profiles[i];
                
                if (profile.gameObject.activeSelf == false) continue;
                if (Economy.TryGetGold(profile.Id, out int gold) == false) continue;
                
                profile.SetGold(gold);

                int territoryValue = await GetTerritoryValue(profile.Id);
                profile.SetTotalGold(gold + territoryValue);
            }
        }
        
        private async UniTask<int> GetTerritoryValue(string playerId)
        {
            BlueMarbleWorld world = BlueMarbleWorld.Instance;

            await UniTask.WaitUntil(world, _ => world.IsBoardReady);
            
            Board board = world.Board;
            List<Tile> territories = board.GetOccupiedTiles(playerId);

            if (territories.Count == 0) return 0;

            int total = 0;
            
            for (int i = 0; i < territories.Count; i++)
                total += territories[i].GetTotalValue();
            
            return total;
        }

        private void HandlePlayerActivityChanged(string playerId, PlayerActivityType activity)
        {
            for (int i = 0; i < profiles.Length; i++)
            {
                Profile profile = profiles[i];

                if (profile.gameObject.activeSelf == false) continue;
                if (profile.Id != playerId) continue;

                if (activity == PlayerActivityType.None)
                    profile.HideBubble();
                else if (activity == PlayerActivityType.Shop)
                    profile.ShowBubble("상점 둘러보는중...");
                else if (activity == PlayerActivityType.TerritoryPurchase)
                    profile.ShowBubble("영토 구매 검토중...");
                else if (activity == PlayerActivityType.Treasure)
                    profile.ShowBubble("보물 찾는중...");
                else if (activity == PlayerActivityType.Combat)
                    profile.ShowBubble("결투 준비중...");

                return;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using BlueMarble.Server;
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
        }

        private void OnDisable()
        {
            Economy.OnUpdated -= RefreshGoldState;
            Territory.OnUpdated -= RefreshGoldState;
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

        private void RefreshGoldState()
        {
            for (int i = 0; i < profiles.Length; i++)
            {
                Profile profile = profiles[i];
                
                if (profile.gameObject.activeSelf == false) continue;

                if (Economy.TryGetGold(profile.Id, out int gold))
                {
                    profile.SetGold(gold);
                    return;
                }

                int territoryValue = GetTerritoryValue(profile.Id);
                profile.SetTotalGold(gold + territoryValue);
            }
        }

        private int GetTerritoryValue(string playerId)
        {
            Board board = BlueMarbleWorld.Instance.Board;
            List<Tile> territories = board.GetOccupiedTiles(playerId);

            if (territories.Count == 0) return 0;

            int total = 0;
            
            for (int i = 0; i < territories.Count; i++)
                total += territories[i].GetTotalValue();
            
            return total;
        }
    }
}
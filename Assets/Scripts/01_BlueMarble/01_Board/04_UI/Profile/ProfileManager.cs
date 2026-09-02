using System;
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

        private void Start()
        {
            InitProfiles();
            RefreshAll();

            Economy.OnUpdated += RefreshAll;
        }

        private void OnDisable()
        {
            Economy.OnUpdated -= RefreshAll;
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

        private void RefreshAll()
        {
            for (int i = 0; i < profiles.Length; i++)
            {
                Profile profile = profiles[i];
                
                if (profile.gameObject.activeSelf == false) continue;
                
                if (Economy.TryGetGold(profile.Id, out int gold))
                    profile.SetGold(gold);
            }
        }
    }
}
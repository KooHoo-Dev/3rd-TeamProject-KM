using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble
{
    public class PlayerManager : MonoBehaviour
    {
        [Title("Components")]
        [SerializeField] private GameObject[] dummies;
        
        public void Initialize(IReadOnlyList<string> ids, string myId)
        {
            if (ids.Count > dummies.Length)
            {
                Debug.LogError("[PlayerManager] Player count is greater than dummy count.");
                return;
            }

            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i];
                GameObject dummy = dummies[i];
                
                if (id == myId)
                {
                    LocalPlayer player = dummy.AddComponent<LocalPlayer>();
                    player.Initialize(id);
                }
                else
                {
                    RemotePlayer player = dummy.AddComponent<RemotePlayer>();
                    player.Initialize(id);
                }

                dummy.SetActive(true);
            }
        }
    }
}
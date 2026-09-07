using UnityEngine;
using Player;
using Cysharp.Threading.Tasks;

namespace Combat
{
    public class CombatTestSetup : MonoBehaviour
    {
        [Header("플레이어 본체 연결")]
        public PlayerEntity localPlayer;
        public PlayerEntity remotePlayer;

        [Header("하드코딩 세팅값")]
        public string localId = "LocalPlayer";
        public string remoteId = "RemotePlayer";
        public int startHp = 50; 

        private void Start()
        {
            if (localPlayer != null)
            {
                localPlayer.Init(localId, startHp);
                
                // 로컬 플레이어는 조작 권한 켜기
                localPlayer.GetComponent<PlayerController>().isLocalPlayer = true;
                localPlayer.GetComponent<EquipmentCaster>().isLocalPlayer = true;
            }

            if (remotePlayer != null)
            {
                remotePlayer.Init(remoteId, startHp);
                
                // 상대방 플레이어는 조작 권한 끄기
                remotePlayer.GetComponent<PlayerController>().isLocalPlayer = false;
                remotePlayer.GetComponent<EquipmentCaster>().isLocalPlayer = false;
            }

            Debug.Log("[CombatTestSetup] 플레이어 초기화 및 권한 세팅 완료. 전투 시작!");

            CombatSystem.Instance.StartTestCombat(localId, remoteId).Forget();
        }
    }
}
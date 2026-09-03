using System.Collections.Generic;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BlueMarble
{
    public class PlayerManager : SingletonBase<PlayerManager>
    {
        [Title("Components")]
        [SerializeField] private GameObject[] dummies;

        private RoomClient Client => RoomClient.Instance;
        
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

        public async UniTask MoveBy(string playerId, int steps)
        {
            if (TryGetPlayer(playerId, out IPlayer player) == false) return;
            bool isMe = Client.Me.Id == playerId;

            for (int i = 0; i < steps; i++)
            {
                Tile reached = await player.MoveNext();
                
                if (isMe == false) continue; // 타일 효과는 '자신'일 경우만 발동

                bool isActiveEffect = 
                    i == steps - 1 ||                // 도착 타일이거나
                    reached.SpaceDef.IsActiveOnPass; // 통과 시에도 효과를 발동해야하거나

                if (isActiveEffect) await reached.ExecuteEffect(playerId);
            }
        }

        public async UniTask MoveTo(string playerId, int tileIdx)
        {
            if (TryGetPlayer(playerId, out IPlayer player) == false) return;
            bool isMe = Client.Me.Id == playerId;

            Board board = BlueMarbleWorld.Instance.Board;

            while (true)
            {
                Tile reached = await player.MoveNext();

                bool isLanding = board.GetTileIndex(reached) == tileIdx;

                if (isMe && (isLanding || reached.SpaceDef.IsActiveOnPass))
                    await reached.ExecuteEffect(playerId);

                if (isLanding) return;
            }
        }

        private bool TryGetPlayer(string playerId, out IPlayer result)
        {
            if (PlayerRegistry.TryGetPlayer(playerId, out result)) return true;
            
            Debug.LogError($"[TurnManager] Player not found: {playerId}");
            return false;
        }
    }
}
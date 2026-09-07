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
        private TileEffectSyncManager Sync => TileEffectSyncManager.Instance;
        
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
                
                Player player = dummy.AddComponent<Player>();
                player.Initialize(id);

                dummy.SetActive(true);
                dummy.name = id;
            }
        }

        public async UniTask MoveBy(string playerId, int steps, int turnId)
        {
            if (TryGetPlayer(playerId, out Player player) == false) return;
            
            bool isOwner = Client.Me.Id == playerId;

            for (int i = 0; i < steps; i++)
            {
                Tile reached = await player.MoveNext();
                
                bool isActiveEffect = 
                    i == steps - 1 ||                // 도착 타일이거나
                    reached.SpaceDef.IsActiveOnPass; // 통과 시에도 효과를 발동해야하거나

                if (isActiveEffect == false) continue;

                await Sync.ExecuteAsync(
                    playerId, reached, turnId, 
                    moveId: 0, stepIndex: i, isOwner);
            }
        }

        public async UniTask MoveTo(string playerId, int tileIdx, int turnId, long moveId)
        {
            if (TryGetPlayer(playerId, out Player player) == false) return;
            
            bool isMe = Client.Me.Id == playerId;
            Board board = BlueMarbleWorld.Instance.Board;

            int stepIdx = 0;

            while (true)
            {
                Tile reached = await player.MoveNext();

                bool isLanding = board.GetTileIndex(reached) == tileIdx;
                bool isActiveEffect = isLanding || reached.SpaceDef.IsActiveOnPass;

                if (isActiveEffect)
                {
                    await Sync.ExecuteAsync(
                        playerId, reached, turnId,
                        moveId, stepIdx, isMe);
                }

                if (isLanding) return;
                
                stepIdx++;
            }
        }

        private bool TryGetPlayer(string playerId, out Player result)
        {
            if (PlayerRegistry.TryGetPlayer(playerId, out result)) return true;
            
            Debug.LogError($"[TurnManager] Player not found: {playerId}");
            return false;
        }
    }
}
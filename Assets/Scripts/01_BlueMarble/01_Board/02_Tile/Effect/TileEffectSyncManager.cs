using System.Collections.Generic;
using BlueMarble.Server;
using Cysharp.Threading.Tasks;
using Jay;

namespace BlueMarble
{
    public class TileEffectSyncManager : SingletonBase<TileEffectSyncManager>
    {
        private readonly Dictionary<string, UniTaskCompletionSource> waiters = new();
        private readonly HashSet<string> completed = new();

        private RoomClient Client => RoomClient.Instance;
        
        private void OnEnable()
        {
            Client.OnTileEffectSync += HandleSync;
        }

        private void OnDisable()
        {
            Client.OnTileEffectSync -= HandleSync;
        }

        public async UniTask ExecuteAsync(string playerId, Tile tile, 
            int turnId, long moveId, int stepIndex, bool isOwner)
        {
            TileEffectSyncMessage readyMessage = new TileEffectSyncMessage
            {
                TurnId = turnId,
                MoveId = moveId,
                StepIndex = stepIndex,
                Phase = TileEffectSyncPhase.Ready
            };

            await AnnounceAndWaitAsync(readyMessage);

            TileEffectSyncMessage resolvedMessage = new TileEffectSyncMessage
            {
                TurnId = turnId,
                MoveId = moveId,
                StepIndex = stepIndex,
                Phase = TileEffectSyncPhase.Resolved
            };

            if (isOwner)
            {
                await tile.ExecuteEffect(playerId);
                await AnnounceAndWaitAsync(resolvedMessage);
            }
            else await WaitAsync(resolvedMessage);
        }

        private UniTask WaitAsync(TileEffectSyncMessage msg)
        {
            string key = CreateKey(msg);

            // 서버 응답이 빨랐을 경우
            if (completed.Remove(key))
                return UniTask.CompletedTask;

            if (waiters.TryGetValue(key, out UniTaskCompletionSource source) == false)
            {
                source = new UniTaskCompletionSource();
                waiters.Add(key, source);
            }
            
            return source.Task;
        }

        private async UniTask AnnounceAndWaitAsync(TileEffectSyncMessage msg)
        {
            UniTask waitTask = WaitAsync(msg);

            await Client.AnnounceTileEffectSync(msg);
            await waitTask;
        }

        private void HandleSync(TileEffectSyncMessage msg)
        {
            string key = CreateKey(msg);

            if (waiters.Remove(key, out UniTaskCompletionSource source))
            {
                source.TrySetResult();
                return;
            }
            
            completed.Add(key);
        }

        private static string CreateKey(TileEffectSyncMessage msg)
            => $"{msg.TurnId}:{msg.MoveId}:{msg.StepIndex}:{msg.Phase}";
    }
}

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlueMarble
{
    public interface IGoldCardPattern
    {
        UniTask Apply(string[] targetIds, int goldAmount, int dropTableId, int tileId);
    }

    public class GainGoldPattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int goldAmount, int _, int tileId)
        {
            EconomyManager economy = EconomyManager.Instance;
            
            List<UniTask> tasks = new();
            
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                tasks.Add(economy.GainGoldTo(targetId, goldAmount));
                
                Debug.Log($"[GoldCardPattern] Gain {goldAmount} gold to {targetId}");
            }

            return UniTask.WhenAll(tasks);
        }
    }

    public class LoseGoldPattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int goldAmount, int _, int tileId)
        {
            EconomyManager economy = EconomyManager.Instance;
            
            List<UniTask> tasks = new();
            
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                tasks.Add(economy.LoseGoldTo(targetId, goldAmount));
                
                Debug.Log($"[GoldCardPattern] Lose {goldAmount} gold to {targetId}");
            }

            return UniTask.WhenAll(tasks);
        }
    }

    public class GainTreasurePattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int _, int dropTableId, int tileId)
        {
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                
                // Economy를 만들고 여기에 해당 부분 집어넣어야 함
                Debug.Log($"[GoldCardPattern] Gain treasure to {targetId}");
            }
            return UniTask.CompletedTask;
        }
    }

    public class LoseTreasurePattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int _, int dropTableId, int tileId)
        {
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                
                // Economy를 만들고 여기에 해당 부분 집어넣어야 함
                Debug.Log($"[GoldCardPattern] Lose treasure to {targetId}");
            }
            return UniTask.CompletedTask;
        }
    }

    public class GoToPattern : IGoldCardPattern
    {
        public async UniTask Apply(string[] targetIds, int _, int __, int tileId)
        {
            TeleportManager teleport = TeleportManager.Instance;
            
            List<UniTask<bool>> tasks = new();
            
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                tasks.Add(teleport.MoveToAsync(targetId, tileId));
            }

            await UniTask.WhenAll(tasks);
        }
    }
}
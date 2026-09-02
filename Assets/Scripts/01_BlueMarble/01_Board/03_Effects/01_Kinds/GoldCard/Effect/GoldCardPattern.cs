using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BlueMarble
{
    public interface IGoldCardPattern
    {
        UniTask Apply(string[] targetIds, int goldAmount, int dropTableId, int spaceId);
    }

    public class GainGoldPattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int goldAmount, int _, int __)
        {
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                
                // Economy를 만들고 여기에 해당 부분 집어넣어야 함
                Debug.Log($"[GoldCardPattern] Gain {goldAmount} gold to {targetId}");
            }
            return UniTask.CompletedTask;
        }
    }

    public class LoseGoldPattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int goldAmount, int _, int __)
        {
            for (int i = 0; i < targetIds.Length; i++)
            {
                string targetId = targetIds[i];
                
                // Economy를 만들고 여기에 해당 부분 집어넣어야 함
                Debug.Log($"[GoldCardPattern] Lose {goldAmount} gold to {targetId}");
            }
            return UniTask.CompletedTask;
        }
    }

    public class GainTreasurePattern : IGoldCardPattern
    {
        public UniTask Apply(string[] targetIds, int _, int dropTableId, int __)
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
        public UniTask Apply(string[] targetIds, int _, int dropTableId, int __)
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
        public async UniTask Apply(string[] targetIds, int _, int __, int spaceId)
        {
            List<UniTask<Tile>> tasks = new();
            
            for (int i = 0; i < targetIds.Length; i++)
            {
                // 음...
            }
        }
    }
}
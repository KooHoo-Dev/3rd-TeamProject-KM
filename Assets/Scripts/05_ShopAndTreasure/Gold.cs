using UnityEngine;

namespace BlueMarble
{
    public class Gold : IGold
    {
        public int CurrentGold { get; private set; }

        public Gold(int startGold = 1000)
        {
            CurrentGold = startGold;
        }

        public bool TrySpend(int amount)
        {
            if (CurrentGold < amount)
            {
                Debug.Log($"[MockGold] 잔액 부족: 보유 {CurrentGold}, 필요 {amount}");
                return false;
            }

            CurrentGold -= amount;
            Debug.Log($"[MockGold] {amount} 지출. 잔액: {CurrentGold}");
            return true;
        }

        public void AddGold(int amount)
        {
            CurrentGold += amount;
            Debug.Log($"[MockGold] {amount} 지급. 잔액: {CurrentGold}");
        }
    }
}